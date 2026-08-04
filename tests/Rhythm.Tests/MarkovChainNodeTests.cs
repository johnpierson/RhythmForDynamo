using System.Collections.Generic;
using NUnit.Framework;

namespace Rhythm.Tests
{
    /// <summary>
    /// Regression tests for Rhythm.Math.MarkovChain.PredictNext.
    ///
    /// The node called Chain(), a weighted random *walk* returning a sequence of words, took
    /// .First() of it to get one randomly chosen word, and then indexed that string - so
    /// "likelyNext" was a single character and "otherOptions" was the rest of that word's letters.
    /// Chain() also returns an empty sequence for a token that never appears in the training data,
    /// or appears only as the final token, so .First() threw on ordinary inputs.
    /// </summary>
    [TestFixture]
    public class PredictNextTests
    {
        private static readonly string[] Training = { "the", "cat", "sat", "the", "cat", "ran", "the", "dog", "sat" };

        [Test]
        public void ReturnsAWholeWordNotACharacter()
        {
            var result = global::Rhythm.Math.MarkovChain.PredictNext(Training, "the");

            Assert.That(result["likelyNext"], Is.InstanceOf<string>(),
                "likelyNext used to be a char taken from inside a word.");
            Assert.That((string)result["likelyNext"], Has.Length.GreaterThan(1));
        }

        [Test]
        public void ReturnsTheMostFrequentSuccessorFirst()
        {
            // "the" is followed by cat, cat, dog - so "cat" is the likely next word.
            var result = global::Rhythm.Math.MarkovChain.PredictNext(Training, "the");

            Assert.That((string)result["likelyNext"], Is.EqualTo("cat"));
        }

        [Test]
        public void OffersTheRemainingSuccessorsAsOtherOptions()
        {
            var result = global::Rhythm.Math.MarkovChain.PredictNext(Training, "the");
            var others = (List<object>)result["otherOptions"];

            Assert.That(others, Does.Contain("dog"));
            Assert.That(others, Does.Not.Contain("cat"), "the likely next word should not be repeated");
        }

        [Test]
        public void DoesNotThrowForATokenThatIsNotInTheTrainingData()
        {
            Dictionary<string, object> result = null;

            Assert.DoesNotThrow(() => result = global::Rhythm.Math.MarkovChain.PredictNext(Training, "zebra"));
            Assert.That(result["likelyNext"], Is.Null);
            Assert.That((List<object>)result["otherOptions"], Is.Empty);
        }

        [Test]
        public void DoesNotThrowForATokenThatOnlyAppearsLast()
        {
            // "sat" is the final token, so it has no recorded successor.
            Assert.DoesNotThrow(() => global::Rhythm.Math.MarkovChain.PredictNext(Training, "sat"));
        }
    }
}

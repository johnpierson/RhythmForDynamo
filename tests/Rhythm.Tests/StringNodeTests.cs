using NUnit.Framework;

namespace Rhythm.Tests
{
    /// <summary>
    /// Regression tests for Rhythm.String.Modify.ParseRegularExpression.
    ///
    /// The node declared regexString and replacement parameters and then ignored both, hardcoding
    /// the pattern and the replacement in its body. Because the hardcoded values happened to equal
    /// the declared defaults, default usage looked correct and the defect went unnoticed - anyone
    /// supplying their own pattern silently got the default behaviour instead.
    /// </summary>
    [TestFixture]
    public class ParseRegularExpressionTests
    {
        [Test]
        public void UsesTheSuppliedPatternAndReplacement()
        {
            var result = global::Rhythm.String.Modify.ParseRegularExpression("a-b-c", "-", "_");

            Assert.That(result, Is.EqualTo("a_b_c"));
        }

        [Test]
        public void UsesTheSuppliedPatternWhenTheReplacementIsEmpty()
        {
            var result = global::Rhythm.String.Modify.ParseRegularExpression("a1b2c3", @"\d", "");

            Assert.That(result, Is.EqualTo("abc"));
        }

        /// <summary>
        /// The defaults are the documented behaviour ("removes all whitespace and special
        /// characters"), so they must survive the fix unchanged.
        /// </summary>
        [Test]
        public void DefaultsStillStripNonAlphanumerics()
        {
            var result = global::Rhythm.String.Modify.ParseRegularExpression("Hello, World! 123");

            Assert.That(result, Is.EqualTo("HelloWorld123"));
        }
    }
}

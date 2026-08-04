using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Markov;

namespace Rhythm.Math
{
    /// <summary>
    /// 
    /// </summary>
    public class MarkovChain
    {
        private MarkovChain()
        {
        }
        /// <summary>
        /// Prediction with a markov chain
        /// </summary>
        /// <param name="trainingData"></param>
        /// <param name="previous"></param>
        /// <returns></returns>
        [MultiReturn(new[] { "likelyNext", "otherOptions" })]
        public static Dictionary<string, object> PredictNext(string[] trainingData, string previous)
        {
            var chain = new MarkovChain<string>(1);
            chain.Add(trainingData);

            // GetNextStates returns each candidate successor with the weight the training data gave
            // it, which is what "likely next" means. The previous implementation called Chain(),
            // a weighted random *walk* that returns a sequence of words; taking .First() of it gave
            // one randomly chosen word, and indexing that string yielded its first character - so
            // likelyNext was a char and otherOptions was the rest of that word's letters. Chain()
            // also returns an empty sequence for a token that never appears, or appears only as the
            // final token, so .First() threw on ordinary inputs.
            var nextStates = chain.GetNextStates(new List<string> { previous });

            if (nextStates == null || nextStates.Count == 0)
            {
                return new Dictionary<string, object>
                {
                    { "likelyNext", null },
                    { "otherOptions", new List<object>() }
                };
            }

            var ranked = nextStates.OrderByDescending(state => state.Value)
                                   .ThenBy(state => state.Key)
                                   .Select(state => state.Key)
                                   .ToList();

            return new Dictionary<string, object>
            {
                { "likelyNext", ranked[0] },
                { "otherOptions", ranked.Skip(1).Cast<object>().ToList() }
            };
        }
    }
}


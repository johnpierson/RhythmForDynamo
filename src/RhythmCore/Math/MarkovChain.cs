﻿using System.Collections.Generic;
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
            var list = chain.Chain(new List<string> { previous }).First();

            return new Dictionary<string, object>
            {
                { "likelyNext", list[0] },
                { "otherOptions", list.Cast<object>().Skip(1).ToList() }
            };
        }
    }
}


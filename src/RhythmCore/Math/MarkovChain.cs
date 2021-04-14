using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;
using Markov;

namespace Rhythm.Math
{
    public class MarkovChain
    {
        private MarkovChain()
        {
        }
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


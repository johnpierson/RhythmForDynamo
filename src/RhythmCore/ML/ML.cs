using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using Accord.MachineLearning;

namespace Rhythm.ML
{
    public class ML
    {
        private ML(){}

        /// <summary>
        /// Gaussian Mixture classification.
        /// This node is made possible thanks to LunchboxML.
        /// Available at the following link under the GNU Lesser General Public License: https://bitbucket.org/archinate/lunchboxml/src/master/
        /// </summary>
        /// <param name="inputList">The list of training inputs.</param>
        /// <param name="components">Number of clusters</param>
        /// <param name="seed">Random seed for solver</param>
        /// <returns name="Result">Predicted result</returns>
        /// <returns name="Likelihood">Log-likelyhood that an input belongs to a cluster.</returns>
        /// <returns name="Probabilities">Probabilities for each possible answer</returns>
        /// <search>classification,machine learning</search>
        [MultiReturn(new[] { "Result", "Likelihood", "Probabilities" })]
        public static Dictionary<string, object> GaussianMixture(List<List<double>> inputList, int components, int seed)
        {

            Tuple<int[], double[], double[][]> result = clsML.GaussianMixture(inputList, components, seed);

            // return a dictionary
            return new Dictionary<string, object>
            {
                {"Result", result.Item1},
                {"Likelihood", result.Item2},
                {"Probabilities", result.Item3 }
            };
        }
    }

    internal class clsML
    {
        /// <summary>
        /// Gaussian Mixture Classifier
        /// </summary>
        /// <param name="inputList">Learning samples</param>
        /// <param name="components">Components</param>
        /// <returns>Result</returns>
        public static Tuple<int[], double[], double[][]> GaussianMixture(List<List<double>> inputList, int coms, int seed)
        {
            // Test Samples
            double[][] inputData = inputList.Select(a => a.ToArray()).ToArray();
            Accord.Math.Random.Generator.Seed = seed;

            //MultivariateNormalDistribution[] components = new MultivariateNormalDistribution[coms];
            //for (int i = 0; i < coms; i++)
            //{
            //    MultivariateNormalDistribution mnd = new MultivariateNormalDistribution(inputData[i].Count());
            //    components[i] = mnd;
            //}

            //MixtureOptions fittingOptions = new MixtureOptions()
            //{
            //    InnerOptions = new NormalOptions()
            //    {
            //        Regularization = 1e-10
            //    },
            //};

            //MultivariateMixture<MultivariateNormalDistribution> mixture = new MultivariateMixture<MultivariateNormalDistribution>(components);

            //mixture.Fit(inputData, fittingOptions);
            //GaussianMixtureModel gmm = new GaussianMixtureModel(mixture);

            GaussianMixtureModel gmm2 = new GaussianMixtureModel(coms);
            // Estimate the Gaussian Mixture
            var clusters = gmm2.Learn(inputData);

            // Predict cluster labels for each sample
            int[] predicted = clusters.Decide(inputData);
            double[] proportions = clusters.Proportions;
            double[][] variance = clusters.Variance;

            return Tuple.Create<int[], double[], double[][]>(predicted, proportions, variance);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Utilities;

namespace Rhythm.Geometry
{
    /// <summary>
    /// Wrapper for geometry
    /// </summary>
    public class Geometry
    {
        private Geometry(){}
        /// <summary>
        /// Find the 'Shortest Walk' within a curve network.  This node uses ported open source code by Giulio Piacentino of McNeel and Associates.
        /// Made possible thanks to Proving Ground open-sourcing their package, Lunchbox. provingground.org
        /// </summary>
        /// <param name="CurveNetwork">A list of curve segments defining a network.</param>
        /// <param name="Lengths">A list of lengths for each curve segment. Length does not need to be "actual" if you want to weight the curves.</param>
        /// <param name="Paths">A list lines defining the start and end of the path.</param>
        /// <returns name="Shortest Walk">The shortest walk path.</returns>
        /// <returns name="Links">Resulting links.</returns>
        /// <returns name="Directions">Resulting directions.</returns>
        /// <returns name="Length">Resulting lengths.</returns>
        /// <search>lunchbox, curves, shortest walk, sort, path, distance</search>
        [MultiReturn(new[] { "Shortest Walk", "Links", "Direction", "Lengths" })]
        public static Dictionary<string, object> LunchboxShortestWalk(List<Curve> CurveNetwork, List<double> Lengths, List<Line> Paths)
        {

            // calcuate stuff here
            ShortestWalkUtils m_swu = new ShortestWalkUtils(CurveNetwork, Lengths, Paths);
            m_swu.SolveShortestWalk();

            List<Curve> m_resultingcurves = m_swu.ResultCurves;
            List<int[]> m_links = m_swu.ResultLinks;
            List<bool[]> m_directions = m_swu.ResultDirections;
            List<double> m_lengths = m_swu.ResultLengths;

            return new Dictionary<string, object>
            {
                {"Shortest Walk", m_resultingcurves},
                {"Links", m_links },
                {"Direction", m_directions},
                {"Lengths", m_lengths}
            };
        }
    }
}

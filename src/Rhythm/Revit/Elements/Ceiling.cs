using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using RevitServices.Persistence;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for floors
    /// </summary>
    public class Ceiling
    {
        private Ceiling() { }


        /// <summary>
        /// Collect the first ceiling type available.
        /// </summary>
        /// <returns name="ceilingType">The first (default) ceiling type.</returns>
        [NodeCategory("Query")]
        public static object DefaultCeilingType()
        {
            string versionNumber = DocumentManager.Instance.CurrentUIApplication.Application.VersionNumber;
            //check if it is Revit 2022
            if (!versionNumber.Contains("2022"))
            {
                throw new Exception(@"This node only works in Revit 2022 as that is when this API was added. ¯\_(ツ)_/¯");
            }
            return Utilities.CommandHelpers.InvokeNode("RhythmRevit2022.dll", "Ceiling.DefaultCeilingType", new object[] { });
        }

        /// <summary>
        /// Create a ceiling by multiple curve loops.
        /// </summary>
        /// <param name="curves">The input curves as a list of lists.</param>
        /// <param name="ceilingType">Ceiling type to use.</param>
        /// <param name="level">The level to host on.</param>
        /// <returns name="ceiling">The newly created ceiling.</returns>
        [NodeCategory("Actions")]
        public static object ByCurveLoops(List<List<Curve>> curves, global::Revit.Elements.Element ceilingType, Level level)
        {
            string versionNumber = DocumentManager.Instance.CurrentUIApplication.Application.VersionNumber;
            //check if it is Revit 2022
            if (!versionNumber.Contains("2022"))
            {
                throw new Exception(@"This node only works in Revit 2022 as that is when this API was added. ¯\_(ツ)_/¯");
            }
            return Utilities.CommandHelpers.InvokeNode("RhythmRevit2022.dll", "Ceiling.ByCurveLoops", new object[] { curves, ceilingType, level });
        }
    }
}

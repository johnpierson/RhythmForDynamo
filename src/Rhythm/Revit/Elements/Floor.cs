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
    public class Floor
    {
        private Floor() { }

        /// <summary>
        /// Collect the first floor type available.
        /// </summary>
        /// <returns name="floorType">The first (default) floor type.</returns>
        [NodeCategory("Query")]
        public static object DefaultFloorType()
        {
            string versionNumber = DocumentManager.Instance.CurrentUIApplication.Application.VersionNumber;
            //check if it is Revit 2022
            if (!versionNumber.Contains("2022"))
            {
                throw new Exception(@"This node only works in Revit 2022 as that is when this API was added. ¯\_(ツ)_/¯");
            }
            return Utilities.CommandHelpers.InvokeNode("RhythmRevit2022.dll", "Floor.DefaultFloorType", new object[] {});
        }

        /// <summary>
        /// Create a floor with multiple loops.
        /// </summary>
        /// <param name="curves">The input curves as a list of lists.</param>
        /// <param name="floorType">Floor type to use.</param>
        /// <param name="level">The level to host on.</param>
        /// <returns name="floor">The new floor.</returns>
        /// <exception cref="Exception"></exception>
        [NodeCategory("Actions")]
        public static object ByCurveLoops(List<List<Curve>> curves, FloorType floorType, Level level)
        {
            string versionNumber = DocumentManager.Instance.CurrentUIApplication.Application.VersionNumber;
            //check if it is Revit 2022
            if (!versionNumber.Contains("2022"))
            {
                throw new Exception(@"This node only works in Revit 2022 as that is when this API was added. ¯\_(ツ)_/¯");
            }
            return Utilities.CommandHelpers.InvokeNode("RhythmRevit2022.dll", "Floor.ByCurveLoops", new object[] { curves, floorType, level });
        }
    }
}

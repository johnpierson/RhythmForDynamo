using Autodesk.DesignScript.Runtime;
using System.Collections.Generic;
using Dynamo.Graph.Nodes;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for grids.
    /// </summary>
    internal class Grids
    {
        private Grids()
        { }
        /// <summary>
        /// This node will get the underlying curve of the reference plane in a given view.
        /// </summary>
        /// <param name="grid">The grid to get leader ends from.</param>
        /// <param name="view">The view to use.</param>
        /// <returns name="leaderEnd0">The leader at end 0 (if available).</returns>
        /// <returns name="leaderEnd1">The leader at end 1 (if available).</returns>
        /// <search>
        /// grid.getleaderends
        /// </search>
        [MultiReturn(new[] { "leaderEnd0", "leaderEnd1" })]
        [NodeCategory("Query")]
        public static Dictionary<string, object>GetLeaderEnds(global::Revit.Elements.Element grid, global::Revit.Elements.Views.View view)
        {
            //convert to internal representation
            Autodesk.Revit.DB.DatumPlane internalGrid = (Autodesk.Revit.DB.DatumPlane)grid.InternalElement;
            //get the leader ends.
            Autodesk.Revit.DB.Leader leaderEnd0 = internalGrid.GetLeader(Autodesk.Revit.DB.DatumEnds.End0, (Autodesk.Revit.DB.View)view.InternalElement);
            Autodesk.Revit.DB.Leader leaderEnd1 = internalGrid.GetLeader(Autodesk.Revit.DB.DatumEnds.End1, (Autodesk.Revit.DB.View)view.InternalElement);

            //returns the outputs
            var outInfo = new Dictionary<string, object>
                {
                    { "leaderEnd0", leaderEnd0},
                    { "leaderEnd1", leaderEnd1}
                };
            return outInfo;
        }
    }
}

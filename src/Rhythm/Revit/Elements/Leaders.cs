using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper for leaders
    /// </summary>
    public class Leaders
    {
        private Leaders()
        {
        }

        /// <summary>
        /// This will get the position of the leader's elbow. Note: Obtain the leader element from the text note with TextNote.GetLeaders
        /// </summary>
        /// <param name="leader">The leader to get the elbow position of</param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static Point GetLeaderElbow(Leader leader)
        {
            try
            {
                return leader.Elbow.ToPoint();
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// This will get the position of the leader's end. Note: Obtain the leader element from the text note with TextNote.GetLeaders
        /// </summary>
        /// <param name="leader">The leader to get the end position of</param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static Point GetLeaderEnd(Leader leader)
        {
            try
            {
                return leader.End.ToPoint();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// This will set a leader's end position. Note: Obtain the leader element from the text note with TextNote.GetLeaders
        /// </summary>
        /// <param name="leader">The leader to set the end position of.</param>
        /// <param name="location">The new location for the leader end.</param>
        [NodeCategory("Actions")]
        public static void SetLeaderEndPosition(Leader leader, Point location)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            TransactionManager.Instance.EnsureInTransaction(doc);
            leader.End = location.ToXyz();
            TransactionManager.Instance.TransactionTaskDone();
        }
        /// <summary>
        /// This will set a leader's elbow position. Note: Obtain the leader element from the text note with TextNote.GetLeaders
        /// </summary>
        /// <param name="leader">The leader to set the elbow position of.</param>
        /// <param name="location">The new location for the leader elbow.</param>
        [NodeCategory("Actions")]
        public static void SetLeaderElbowPosition(Leader leader, Point location)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            TransactionManager.Instance.EnsureInTransaction(doc);
            leader.Elbow = location.ToXyz();
            TransactionManager.Instance.TransactionTaskDone();
        }
    }
}
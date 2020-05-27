using Autodesk.DesignScript.Runtime;
using RevitServices.Persistence;
using RevitServices.Transactions;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using Revit.GeometryConversion;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for tag.
    /// </summary>
    public class Tags
    {
        private Tags()
        {
        }
        /// <summary>
        /// The position of the head of tag in model coordinates (if available).
        /// </summary>
        /// <param name="tag">The tag to get head position from.</param>
        /// <returns name="point">The tag's head position as a point.</returns>
        /// <search>
        /// Tag, Tag.Location
        /// </search>
        [NodeCategory("Query")]
        public static List<Point> GetHeadPosition(List<global::Revit.Elements.Tag> tag)
        {
            //declare a list for the points
            List<Point> point = new List<Point>();
            foreach (var t in tag)
            {
                var internalTag = (Autodesk.Revit.DB.IndependentTag)t.InternalElement;
                try
                {
                    point.Add(internalTag.TagHeadPosition.ToPoint());
                }
                catch
                {
                    point.Add(null);
                }
            }

            return point;
        }
        /// <summary>
        /// The position of the leader end for a tag using free end leader behavior. 
        /// </summary>
        /// <param name="tag">The tag to get leader end position from.</param>
        /// <returns name="point">The tag's leader end position as a point.</returns>
        /// <search>
        /// Tag, Tag.Location
        /// </search>
        [NodeCategory("Query")]
        public static List<Point> GetLeaderEnd(List<global::Revit.Elements.Tag> tag)
        {
            //declare a list for the points
            List<Point> point = new List<Point>();
            foreach (var t in tag)
            {
                var internalTag = (Autodesk.Revit.DB.IndependentTag)t.InternalElement;
                try
                {
                    point.Add(internalTag.LeaderEnd.ToPoint());
                }
                catch
                {
                    point.Add(null);
                }
            }

            return point;
        }
        /// <summary>
        /// The position of the elbow of the tag's leader.  
        /// </summary>
        /// <param name="tag">The tag to get leader elbow position from.</param>
        /// <returns name="point">The tag's leader elbow position as a point.</returns>
        /// <search>
        /// Tag, Tag.Location
        /// </search>
        [NodeCategory("Query")]
        public static List<Point> GetLeaderElbow(List<global::Revit.Elements.Tag> tag)
        {
            //declare a list for the points
            List<Point> point = new List<Point>();
            foreach (var t in tag)
            {
                var internalTag = (Autodesk.Revit.DB.IndependentTag)t.InternalElement;
                try
                {
                    point.Add(internalTag.LeaderElbow.ToPoint());
                }
                catch
                {
                    point.Add(null);
                }
            }

            return point;
        }
        /// <summary>
        /// This will attempt to set the leader end position of the tag.
        /// </summary>
        /// <param name="tag">The tag to set leader end position of.</param>
        /// <param name="location">The new location for the leader end.</param>
        /// <search>
        /// Tag, Tag.Location
        /// </search>
        [NodeCategory("Actions")]
        public static void SetLeaderEndPosition(global::Revit.Elements.Tag tag, Point location)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.IndependentTag internalTag = (Autodesk.Revit.DB.IndependentTag)tag.InternalElement;
            TransactionManager.Instance.EnsureInTransaction(doc);
            internalTag.LeaderEnd = location.ToXyz();
            TransactionManager.Instance.TransactionTaskDone();
        }
        /// <summary>
        /// This will attempt to set the head position of the tag.
        /// </summary>
        /// <param name="tag">The tag to set head position of.</param>
        /// <param name="location">The new location for the head.</param>
        /// <search>
        /// Tag, Tag.Location
        /// </search>
        [NodeCategory("Actions")]
        public static void SetHeadPosition(global::Revit.Elements.Tag tag, Point location)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.IndependentTag internalTag = (Autodesk.Revit.DB.IndependentTag)tag.InternalElement;
            TransactionManager.Instance.EnsureInTransaction(doc);
            internalTag.TagHeadPosition = location.ToXyz();
            TransactionManager.Instance.TransactionTaskDone();
        }
        /// <summary>
        /// This will attempt to set the leader elbow position of the tag.
        /// </summary>
        /// <param name="tag">The tag to set leader elbow position of.</param>
        /// <param name="location">The new location for the leader elbow.</param>
        /// <search>
        /// Tag, Tag.Location
        /// </search>
        [NodeCategory("Actions")]
        public static void SetLeaderElbowPosition(global::Revit.Elements.Tag tag, Point location)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.IndependentTag internalTag = (Autodesk.Revit.DB.IndependentTag)tag.InternalElement;
            TransactionManager.Instance.EnsureInTransaction(doc);
            internalTag.LeaderElbow = location.ToXyz();
            TransactionManager.Instance.TransactionTaskDone();
        }
        /// <summary>
        /// This will return the tag's text value
        /// </summary>
        /// <param name="tag">The tag to get text of.</param>
        [NodeCategory("Query")]
        public static string TagText(global::Revit.Elements.Element tag)
        {
            var internalTag = tag.InternalElement;
            string internalType = internalTag.GetType().BaseType.Name;

            if (internalType.Equals("SpatialElementTag"))
            {
                Autodesk.Revit.DB.SpatialElementTag spatialTag = internalTag as Autodesk.Revit.DB.SpatialElementTag;
                return spatialTag.TagText;
            }

            IndependentTag independentTag = internalTag as IndependentTag;
            return independentTag.TagText;
        }
    }
}

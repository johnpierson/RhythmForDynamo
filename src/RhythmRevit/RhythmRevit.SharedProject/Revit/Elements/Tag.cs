using RevitServices.Persistence;
using RevitServices.Transactions;
using System.Linq;
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
        public static Point GetHeadPosition(global::Revit.Elements.Element tag)
        {
            var internalElement = tag.InternalElement;

            if (internalElement is Autodesk.Revit.DB.IndependentTag internalTag)
            {
                return internalTag.TagHeadPosition.ToPoint();
            }

            if (internalElement is Autodesk.Revit.DB.Architecture.RoomTag roomTag)
            {
             return roomTag.TagHeadPosition.ToPoint();
            }

            if (internalElement is Autodesk.Revit.DB.Mechanical.SpaceTag spaceTag)
            {
                return spaceTag.TagHeadPosition.ToPoint();
            }

            return null;
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
        public static object GetLeaderEnd(global::Revit.Elements.Tag tag)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            var internalElement = tag.InternalElement;

            if (internalElement is Autodesk.Revit.DB.IndependentTag internalTag)
            {
#if Revit2020 || Revit2021
                return internalTag.LeaderEnd.ToPoint();

#endif
#if Revit2022 || Revit2023 || Revit2024
                                return internalTag.GetTaggedReferences().Select(r => internalTag.GetLeaderEnd(r).ToPoint()).ToList();
#endif
            }

            if (internalElement is Autodesk.Revit.DB.Architecture.RoomTag roomTag)
            {
                return roomTag.LeaderEnd.ToPoint();
            }

            if (internalElement is Autodesk.Revit.DB.Mechanical.SpaceTag spaceTag)
            {
                return spaceTag.LeaderEnd.ToPoint();
            }

            return null;
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
        public static object GetLeaderElbow(global::Revit.Elements.Tag tag)
        {
            var internalElement = tag.InternalElement;

            if (internalElement is Autodesk.Revit.DB.IndependentTag internalTag)
            {
#if Revit2020 || Revit2021
                return internalTag.LeaderElbow.ToPoint();

#endif
#if Revit2022 || Revit2023 || Revit2024
                return internalTag.GetTaggedReferences().Select(r => internalTag.GetLeaderElbow(r).ToPoint()).ToList();

#endif
            }

            if (internalElement is Autodesk.Revit.DB.Architecture.RoomTag roomTag)
            {
                return roomTag.LeaderElbow.ToPoint();
            }

            if (internalElement is Autodesk.Revit.DB.Mechanical.SpaceTag spaceTag)
            {
                return spaceTag.LeaderElbow.ToPoint();
            }

            return null;
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
            var internalElement = tag.InternalElement;

            TransactionManager.Instance.EnsureInTransaction(doc);
            if (internalElement is Autodesk.Revit.DB.IndependentTag internalTag)
            {
                internalTag.TagHeadPosition = location.ToXyz();
            }

            if (internalElement is Autodesk.Revit.DB.Architecture.RoomTag roomTag)
            {
                roomTag.TagHeadPosition = location.ToXyz();
            }

            if (internalElement is Autodesk.Revit.DB.Mechanical.SpaceTag spaceTag)
            {
                spaceTag.TagHeadPosition = location.ToXyz();
            }
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

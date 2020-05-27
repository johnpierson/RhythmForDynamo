using System.Collections.Generic;
using System.Linq;
using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using Revit.Elements;
using RevitServices.Transactions;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for room tags.
    /// </summary>
    public class RoomTag
    {
        private RoomTag()
        { }
        /// <summary>
        /// This node will retrieve the room that a tag is tagging.
        /// </summary>
        /// <param name="roomTag">The room tag to retrieve elements from.</param>
        /// <returns name="room">The room that is tagged.</returns>
        /// <search>
        /// roomtag, rhythm
        /// </search>
        [NodeCategory("Query")]
        public static List<global::Revit.Elements.Element> TaggedRoom(List<global::Revit.Elements.Element> roomTag)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //convert room tags to internal representations
            List<global::Autodesk.Revit.DB.Architecture.RoomTag> internalRoomTagList = 
                new List<global::Autodesk.Revit.DB.Architecture.RoomTag>(roomTag.Select(tag => (Autodesk.Revit.DB.Architecture.RoomTag)tag.InternalElement));
            //get the tagged rooms
            List<global::Revit.Elements.Element> taggedRoomList = 
                new List<global::Revit.Elements.Element>(internalRoomTagList.Select(tag => doc.GetElement(tag.TaggedLocalRoomId).ToDSType(true)));

            return taggedRoomList;
        }

        /// <summary>
        /// This node will set the room tag to the same as the room location.
        /// </summary>
        /// <param name="roomTag">The room tag to set.</param>
        /// <returns name="roomTag">The room tag.</returns>
        /// <search>
        /// roomtag, rhythm
        /// </search>
        [NodeCategory("Actions")]
        public static List<global::Revit.Elements.Element> CenterOnRoomLocation(List<global::Revit.Elements.Element> roomTag)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            foreach (var tag in roomTag)
            {
                if (tag.InternalElement is Autodesk.Revit.DB.FamilyInstance)
                {
                    continue;
                }
                Autodesk.Revit.DB.Architecture.RoomTag internalRoomTag =
                    (Autodesk.Revit.DB.Architecture.RoomTag) tag.InternalElement;
                Autodesk.DesignScript.Geometry.Point roomLocation =
                    (Autodesk.DesignScript.Geometry.Point)
                    doc.GetElement(internalRoomTag.TaggedLocalRoomId).ToDSType(true).GetLocation();
                TransactionManager.Instance.EnsureInTransaction(doc);
                tag.SetLocation(roomLocation);
                TransactionManager.Instance.TransactionTaskDone();
                roomLocation.Dispose();
            }
            return roomTag;
        }
    }
}

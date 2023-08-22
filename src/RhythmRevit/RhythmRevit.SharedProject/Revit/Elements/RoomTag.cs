using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Transactions;
using Point = Autodesk.DesignScript.Geometry.Point;

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
                new List<global::Revit.Elements.Element>(internalRoomTagList.Select(tag => doc.GetElement(tag.TaggedLocalRoomId).ToDSType(false)));

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
                    doc.GetElement(internalRoomTag.TaggedLocalRoomId).ToDSType(false).GetLocation();
                TransactionManager.Instance.EnsureInTransaction(doc);
                tag.SetLocation(roomLocation);
                TransactionManager.Instance.TransactionTaskDone();
                roomLocation.Dispose();
            }
            return roomTag;
        }

        /// <summary>
        /// Place or update an existing room tag.
        /// </summary>
        /// <param name="view">Tags are view specific. This is the specific view to use.</param>
        /// <param name="room">The room to be tagged.</param>
        /// <param name="location">The location to place the tag. If null, this will place on room origin.</param>
        /// <param name="tagType">The tag type to use. If null, the default one is used</param>
        /// <param name="tryUpdateExisting">Toggle to true to try and update existing room tags in the view.</param>
        /// <returns name="roomTag">The new or updated room tag.</returns>
        [NodeCategory("Actions")]
        public static global::Revit.Elements.Element PlaceOrUpdate(global::Revit.Elements.Views.FloorPlanView view, global::Revit.Elements.Room room, [DefaultArgument("Rhythm.Utilities.MiscUtils.GetNull()")] Point location, [DefaultArgument("Rhythm.Utilities.MiscUtils.GetNull()")] global::Revit.Elements.FamilyType tagType, bool tryUpdateExisting = false)
        {
            var internalRoom = room.InternalElement as Autodesk.Revit.DB.Architecture.Room;

            if (internalRoom.Area <= 0) return null;

            
            var doc = internalRoom.Document;

            LinkElementId linkElementId = new LinkElementId(internalRoom.Id);

            Point locationPoint = location ?? room.Location;

            Autodesk.Revit.DB.Architecture.RoomTag roomTag;

            if (tryUpdateExisting && Rooms.RoomTagsInView(room, view).Any())
            {
                var firstTag =
                    Rooms.RoomTagsInView(room, view).First();

                //set the location if a tag is found and that option is selected
                TransactionManager.Instance.EnsureInTransaction(doc);
                firstTag.SetLocation(locationPoint);
                TransactionManager.Instance.TransactionTaskDone();
                roomTag = firstTag.InternalElement as Autodesk.Revit.DB.Architecture.RoomTag;
            }
            else
            {
                var revitPoint = locationPoint.ToRevitType(true);
                UV uv = new UV(revitPoint.X, revitPoint.Y);

                //create a new tag if none are found
                TransactionManager.Instance.EnsureInTransaction(doc);
                roomTag = doc.Create.NewRoomTag(linkElementId, uv, view.InternalElement.Id);

                roomTag.ToDSType(false).SetLocation(locationPoint);
                TransactionManager.Instance.TransactionTaskDone();
            }

            //if the given tag type is valid, set the tag to that type
            if (tagType == null) return roomTag.ToDSType(false);

#if Revit2020 ||Revit2021 || Revit2022|| Revit2023
            ElementId elementId = new ElementId(-2000480);
#endif

#if Revit2024
            ElementId elementId = new ElementId(Convert.ToInt64(-2000480));
#endif

            if (tagType.InternalElement.Category.Id != elementId) return roomTag.ToDSType(false);

            var roomTagType = doc.GetElement(tagType.InternalElement.Id) as RoomTagType;
            TransactionManager.Instance.EnsureInTransaction(doc);
            roomTag.ChangeTypeId(roomTagType.Id);
            TransactionManager.Instance.TransactionTaskDone();

            return roomTag.ToDSType(false);
        }
    }
}

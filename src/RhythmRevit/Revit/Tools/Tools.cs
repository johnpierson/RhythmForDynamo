using System;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using FamilyInstance = Revit.Elements.FamilyInstance;
using Room = Autodesk.Revit.DB.Architecture.Room;

namespace Rhythm.Revit.Tools
{
    /// <summary>
    /// Wrapper class for tools
    /// </summary>
    public partial class Tools
    {
        private Tools(){}

        /// <summary>
        /// Create 3d room tags given the input rooms!
        ///
        /// Superseded on Revit 2025 and up by Tools.ThreeDeeSpatialTags, which does the whole job
        /// from one dialog: every room or space in a phase, from this document or a link, updated
        /// in place on a second run instead of piling up a fresh set of duplicates. This node
        /// places one tag per room and knows nothing about the tags already there.
        ///
        /// On those versions it is hidden from the library. It still works, so a graph already
        /// built on it keeps running; it just cannot be found and dropped onto a new one.
        ///
        /// On Revit 2020 to 2024 it is still the way to place 3d room tags, and still in the
        /// library, because the node that supersedes it does not exist there: the tag family that
        /// one places is saved in Revit 2025, and families do not load backward.
        /// </summary>
        /// <param name="room">The rooms to place 3d room tags on.</param>
        /// <param name="tagType">The 3d room tag to use. (There is a sample RFA in the extra folder for Rhythm)</param>
        /// <param name="roomNameParameter">The name of your Name parameter, the sample has the parameter named as Room Name</param>
        /// <param name="roomNumberParameter">The name of your Number parameter, the sample has the parameter named as Room Number</param>
        /// <returns></returns>
        // Taken out of the library, and marked obsolete, only where the node that supersedes it
        // exists. Saying "use ThreeDeeSpatialTags instead" on a 2020-2024 build would point at
        // nothing, and hiding it there would leave those versions with no 3d room tag node at all.
        //
        // Hidden rather than deleted: the method is still here and still works, so a graph already
        // built on it keeps running. It just cannot be found and dropped onto a new one.
#if R25_OR_GREATER
        [IsVisibleInDynamoLibrary(false)]
        [IsObsolete("Superseded by Tools.ThreeDeeSpatialTags, which tags every room in a phase and updates the ones already placed")]
#endif
        public static global::Revit.Elements.FamilyInstance ThreeDeeRoomTags(global::Revit.Elements.Room room, global::Revit.Elements.FamilyType tagType, string roomNameParameter = "Name", string roomNumberParameter = "Number")
        {
            Room internalRoom = room.InternalElement as Room;

            if (internalRoom.Area <= 0)
            {
                return null;
            }
            Autodesk.Revit.DB.Document currentDoc = DocumentManager.Instance.CurrentDBDocument;

            var locationPoint = room.Location;

            //check to see if it is from a link
            Autodesk.Revit.DB.Document doc = internalRoom.Document;
            if (doc.IsLinked)
            {
                RevitLinkInstance linkInstance = new FilteredElementCollector(currentDoc)
                    .OfClass(typeof(RevitLinkInstance)).Cast<RevitLinkInstance>().FirstOrDefault(l => l.GetLinkDocument().Equals(doc));
                if (linkInstance != null)
                {
                    var transform = linkInstance.GetTransform();
                    locationPoint = transform.OfPoint(locationPoint.ToRevitType()).ToPoint();
                }
            }

            var newFamilyInstance = FamilyInstance.ByPoint(tagType, locationPoint);

            try
            {
                newFamilyInstance.SetParameterByName(roomNameParameter, room.Name);
            }
            catch (Exception)
            {
                Dynamo.Logging.LogMessage.Error("Room name parameter not found");
            }

            try
            {
                newFamilyInstance.SetParameterByName(roomNumberParameter, room.Number);
            }
            catch (Exception)
            {
                Dynamo.Logging.LogMessage.Error("Room number parameter not found");
            }
            
            return newFamilyInstance;
        }
#if !R20
        /// <summary>
        /// Create 3d space tags given the input spaces!
        ///
        /// Superseded on Revit 2025 and up by Tools.ThreeDeeSpatialTags, which tags spaces as
        /// readily as rooms, from one dialog: every space in a phase, from this document or a
        /// link, updated in place on a second run instead of piling up a fresh set of duplicates.
        /// This node places one tag per space and knows nothing about the tags already there.
        ///
        /// On those versions it is hidden from the library. It still works, so a graph already
        /// built on it keeps running; it just cannot be found and dropped onto a new one.
        ///
        /// On Revit 2021 to 2024 it is still the way to place 3d space tags, and still in the
        /// library, because the node that supersedes it does not exist there: the tag family that
        /// one places is saved in Revit 2025, and families do not load backward.
        /// </summary>
        /// <param name="space">The spaces to place 3d space tags on.</param>
        /// <param name="tagType">The 3d space tag to use. (There is a sample RFA in the extra folder for Rhythm)</param>
        /// <param name="spaceNameParameter">The name of your Name parameter, the sample has the parameter named as Space Name</param>
        /// <param name="spaceNumberParameter">The name of your Number parameter, the sample has the parameter named as Space Number</param>
        /// <returns></returns>
#if R25_OR_GREATER
        [IsVisibleInDynamoLibrary(false)]
        [IsObsolete("Superseded by Tools.ThreeDeeSpatialTags, which tags every space in a phase and updates the ones already placed")]
#endif
        public static global::Revit.Elements.FamilyInstance ThreeDeeSpaceTags(global::Revit.Elements.Space space, global::Revit.Elements.FamilyType tagType, string spaceNameParameter = "Name", string spaceNumberParameter = "Number")
        {
            Autodesk.Revit.DB.Mechanical.Space internalSpace = space.InternalElement as Autodesk.Revit.DB.Mechanical.Space;

            if (internalSpace.Area <= 0)
            {
                return null;
            }
            Autodesk.Revit.DB.Document currentDoc = DocumentManager.Instance.CurrentDBDocument;

            var locationPoint = space.Location;

            //check to see if it is from a link
            Autodesk.Revit.DB.Document doc = internalSpace.Document;
            if (!doc.Title.Equals(currentDoc.Title))
            {
                RevitLinkInstance linkInstance = new FilteredElementCollector(currentDoc)
                    .OfClass(typeof(RevitLinkInstance)).Cast<RevitLinkInstance>().FirstOrDefault(l => l.GetLinkDocument().Equals(doc));
                if (linkInstance != null)
                {
                    var transform = linkInstance.GetTransform();
                    locationPoint = transform.OfPoint(locationPoint.ToRevitType()).ToPoint();
                }
            }
            var newFamilyInstance = FamilyInstance.ByPoint(tagType, locationPoint);

            try
            {
                newFamilyInstance.SetParameterByName(spaceNameParameter, space.Name);
            }
            catch (Exception)
            {
                Dynamo.Logging.LogMessage.Error("Space name parameter not found");
            }

            try
            {
                newFamilyInstance.SetParameterByName(spaceNumberParameter, space.Number);
            }
            catch (Exception)
            {
                Dynamo.Logging.LogMessage.Error("Space number parameter not found");
            }

            return newFamilyInstance;
        }   
#endif

    }
}

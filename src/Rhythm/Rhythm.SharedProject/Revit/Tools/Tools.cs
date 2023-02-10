using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using FamilyInstance = Revit.Elements.FamilyInstance;
using Room = Autodesk.Revit.DB.Architecture.Room;

namespace Rhythm.Revit.Tools
{
    /// <summary>
    /// Wrapper class for tools
    /// </summary>
    public class Tools
    {
        private Tools(){}

        /// <summary>
        /// Create 3d room tags given the input rooms!
        /// </summary>
        /// <param name="room">The rooms to place 3d room tags on.</param>
        /// <param name="tagType">The 3d room tag to use. (There is a sample RFA in the extra folder for Rhythm)</param>
        /// <param name="roomNameParameter">The name of your Name parameter, the sample has the parameter named as Room Name</param>
        /// <param name="roomNumberParameter">The name of your Number parameter, the sample has the parameter named as Room Number</param>
        /// <returns></returns>
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
#if Revit2021 || Revit2022 || Revit2023
     /// <summary>
        /// Create 3d space tags given the input spaces!
        /// </summary>
        /// <param name="space">The spaces to place 3d space tags on.</param>
        /// <param name="tagType">The 3d space tag to use. (There is a sample RFA in the extra folder for Rhythm)</param>
        /// <param name="spaceNameParameter">The name of your Name parameter, the sample has the parameter named as Space Name</param>
        /// <param name="spaceNumberParameter">The name of your Number parameter, the sample has the parameter named as Space Number</param>
        /// <returns></returns>
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

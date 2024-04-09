using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Revit.Elements;
using RevitServices.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using Revit.GeometryConversion;
using Curve = Autodesk.DesignScript.Geometry.Curve;
using FloorType = Autodesk.Revit.DB.FloorType;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for groups
    /// </summary>
    public class Group
    {
        private Group()
        {
        }

        /// <summary>
        /// This node is a pretty neat group creator, that allows for you to pick an origin at creation time.
        /// </summary>
        /// <param name="elements">The elements to group</param>
        /// <param name="name">Optional Name</param>
        /// <param name="origin">Optional origin. (Note: This node will fix whatever Z Value you input to match the group's Z value)</param>
        /// <returns name="newGroup">The new group</returns>
        public static global::Revit.Elements.Element ByElementsAndOrigin(
            List<global::Revit.Elements.Element> elements,
            [DefaultArgument("Rhythm.Utilities.MiscUtils.GetNull()")] string name,
            [DefaultArgument("Rhythm.Utilities.MiscUtils.GetNull()")] Point origin)
        {
            Document doc = elements.First().InternalElement.Document;

            //if any of the elements belong to a group, cancel
            if (elements.Any(e => e.InternalElement.GroupId != ElementId.InvalidElementId))
            {
                throw new Exception(@"One or more of the given elements are already a part of a group.");
            }

            //if you just want to create a group without the crazy workaround
            if (origin is null)
            {
                TransactionManager.Instance.EnsureInTransaction(doc);
                //create a group from our elements by default
                var defaultOriginGroup = doc.Create.NewGroup(elements.Select(e => e.InternalElement.Id).ToList());

                //name it if there is a specific name you want
                if (!string.IsNullOrWhiteSpace(name))
                {
                    defaultOriginGroup.GroupType.Name = name;
                }

                TransactionManager.Instance.TransactionTaskDone();
                return defaultOriginGroup.ToDSType(false);
            }
            //now for the magic

            //we do this to find the level and to fix the origin provided
            var bBoxes = elements.Select(e => e.BoundingBox.ToCuboid()).ToList();
            var encompassBoundingBox = BoundingBox.ByGeometry(bBoxes);
            var bottomPoint = encompassBoundingBox.MinPoint;
            var topPoint = encompassBoundingBox.MaxPoint;
            var radius =
                bottomPoint.DistanceTo(topPoint) *
                10; //make the floor 10x the size of the bounding box. (hopefully works well)

            //find the closest level
            var closestLevel =
                doc.GetElement(elements.First(e => e.InternalElement.LevelId != null).InternalElement.LevelId)
                    .ToDSType(false) as global::Revit.Elements.Level;

            //find the first floor type and create a dispensable one
            var foundFloorType = new FilteredElementCollector(doc).OfClass(typeof(FloorType))
                .WhereElementIsElementType().Cast<FloorType>().FirstOrDefault();

            var newFloorType =
                foundFloorType.Duplicate("secretinternalfloor").ToDSType(false) as global::Revit.Elements.FloorType;

            //make a big ol circle to force the origin and add it to a curve list. We also use the group's z origin to fix what the user inputs.
            var bigCircle = Circle.ByCenterPointRadius(Point.ByCoordinates(origin.X, origin.Y, bottomPoint.Z), radius);

            List<Curve> floorSketch = new List<Curve>();
            floorSketch.AddRange(Polygon.RegularPolygon(bigCircle, 10).Curves());

            TransactionManager.Instance.EnsureInTransaction(doc);
            //create the big floor (temporary), based on the type
            var newFloor =
                global::Revit.Elements.Floor.ByOutlineTypeAndLevel(floorSketch.ToArray(), newFloorType, closestLevel);
            TransactionManager.Instance.TransactionTaskDone();

            //dispose of the geometry so dynamo will calm down
            foreach (var c in floorSketch)
            {
                c.Dispose();
            }

            //add our ids to a list
            List<ElementId> groupIds = new List<ElementId>();
            groupIds.AddRange(elements.Select(e => e.InternalElement.Id).ToList());
            groupIds.Add(newFloor.InternalElement.Id);

            TransactionManager.Instance.EnsureInTransaction(doc);
            //create a group from our elements by default
            var specificOriginGroup = doc.Create.NewGroup(groupIds);

            //name it if there is a specific name you want
            if (!string.IsNullOrWhiteSpace(name))
            {
                specificOriginGroup.GroupType.Name = name;
            }

            TransactionManager.Instance.TransactionTaskDone();

            //now delete the floor type
            TransactionManager.Instance.EnsureInTransaction(doc);
            doc.Delete(newFloorType.InternalElement.Id);
            TransactionManager.Instance.TransactionTaskDone();

            return specificOriginGroup.ToDSType(false);
        }

        //public static global::Revit.Elements.Element RemoveElementsFromGroup(global::Revit.Elements.Element group,
        //    List<global::Revit.Elements.Element> elementsToRemove)
        //{
        //    Document doc = group.InternalElement.Document;

        //    Autodesk.Revit.DB.Group internalGroup = group.InternalElement as Autodesk.Revit.DB.Group;

        //    var originalLocation = internalGroup.Location as LocationPoint;
        //    var originalLocationPoint = originalLocation.Point;

        //    var toRemoveIds = elementsToRemove.Select(e => e.InternalElement.Id).ToList();

        //    var originalGroupIds = internalGroup.GetMemberIds().ToList();

        //    var newGroupIds = originalGroupIds.Except(toRemoveIds).ToList();

        //    var newGroupElements = newGroupIds.Select(e => doc.GetElement(e).ToDSType(false)).ToList();

        //    var offsetLocation = new XYZ(originalLocationPoint.X, originalLocationPoint.Y,
        //        originalLocationPoint.Z + 1000);


        //    return ByElementsAndOrigin(newGroupElements, group.Name, offsetLocation.ToPoint());
        //}
    }

}
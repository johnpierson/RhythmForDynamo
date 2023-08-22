using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Transactions;
using Curve = Autodesk.DesignScript.Geometry.Curve;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for rooms.
    /// </summary>
    public class Rooms
    {
        private Rooms()
        { }
        /// <summary>
        /// This node will center the room.
        /// </summary>
        /// <param name="room">The room to center.</param>
        /// <returns name="room">The room.</returns>
        /// <search>
        /// roomtag, rhythm
        /// </search>
        [NodeCategory("Actions")]
        public static List<global::Revit.Elements.Room> CenterRoom(List<global::Revit.Elements.Room> room)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            foreach (var r in room)
            {
                Autodesk.Revit.DB.Architecture.Room internalRoom = (Autodesk.Revit.DB.Architecture.Room)r.InternalElement;
                Autodesk.Revit.DB.Options geoOpts = new Autodesk.Revit.DB.Options();
                var geo = internalRoom.get_Geometry(geoOpts);
                Autodesk.DesignScript.Geometry.Point centroid = geo.GetBoundingBox().ToProtoType(true).ToCuboid().Centroid();
                Autodesk.DesignScript.Geometry.Point roomLocation = (Autodesk.DesignScript.Geometry.Point)r.GetLocation();
                var newPoint =
                    Autodesk.DesignScript.Geometry.Point.ByCoordinates(centroid.X, centroid.Y, roomLocation.Z);

                if (r.IsInsideRoom(newPoint))
                {
                    TransactionManager.Instance.EnsureInTransaction(doc);
                    r.SetLocation(newPoint);
                    TransactionManager.Instance.TransactionTaskDone();
                }
                else
                {
                    TransactionManager.Instance.EnsureInTransaction(doc);
                    r.SetLocation(r.Location);
                    TransactionManager.Instance.TransactionTaskDone();
                }
                //dispose of the temp points
                centroid.Dispose();
                roomLocation.Dispose();
                newPoint.Dispose();
            }

            return room;
        }

        /// <summary>
        /// This node will center the room.
        /// </summary>
        /// <param name="room">The room to center.</param>
        /// <returns name="room">The room.</returns>
        /// <search>
        /// roomtag, rhythm
        /// </search>
        [NodeCategory("Actions")]
        public static global::Revit.Elements.Room CenterRoom2(global::Revit.Elements.Room room)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            IEnumerable<IEnumerable<Autodesk.DesignScript.Geometry.Curve>> coreBoundary = room.CoreBoundary;

            List<Autodesk.DesignScript.Geometry.Point> pointList = new List<Autodesk.DesignScript.Geometry.Point>();

            Polygon poly = null;
            foreach (IEnumerable<Autodesk.DesignScript.Geometry.Curve> curve in coreBoundary)
            {
                foreach (Autodesk.DesignScript.Geometry.Curve c in curve)
                {

                    pointList.Add(c.StartPoint);
                }
                poly = Polygon.ByPoints(pointList);
            }
            if (room.IsInsideRoom(poly.Center()))
            {
                TransactionManager.Instance.EnsureInTransaction(doc);
                room.SetLocation(poly.Center());
                TransactionManager.Instance.TransactionTaskDone();
            }
            else
            {
                TransactionManager.Instance.EnsureInTransaction(doc);
                room.SetLocation(room.Location);
                TransactionManager.Instance.TransactionTaskDone();
            }
            return room;
        }

        /// <summary>
        /// Return room tags for given room in given view
        /// </summary>
        /// <param name="room">The room to check.</param>
        /// <param name="view">The view to check in.</param>
        /// <returns name="roomTags">We return a list here because a room can have more than one tag. Whether or not it should, is a different conversation.</returns>
        public static List<global::Revit.Elements.Element> RoomTagsInView(global::Revit.Elements.Room room,
            global::Revit.Elements.Views.FloorPlanView view)
        {
            var internalRoom = room.InternalElement as Autodesk.Revit.DB.Architecture.Room;
            var doc = internalRoom.Document;

            ElementCategoryFilter roomTagFilter = new ElementCategoryFilter(BuiltInCategory.OST_RoomTags);
            var roomTags = internalRoom.GetDependentElements(roomTagFilter).Select(id => doc.GetElement(id)).ToList();

            return roomTags.Cast<Autodesk.Revit.DB.Architecture.RoomTag>()
                .Where(rt => rt.View.Id.Equals(view.InternalElement.Id)).Select(r => r.ToDSType(false)).ToList();
        }

        /// <summary>
        /// This node will center the room.
        /// </summary>
        /// <param name="room">The room to center.</param>
        /// <param name="category">The room to center.</param>
        /// <returns name="elements">The room.</returns>
        /// <search>
        /// roomtag, rhythm
        /// </search>
        [NodeCategory("Query")]
        public static List<global::Revit.Elements.Element> IntersectingElementsInRoom(global::Revit.Elements.Room room, global::Revit.Elements.Category category)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            Autodesk.DesignScript.Geometry.Solid roomSolid = Autodesk.DesignScript.Geometry.Solid.ByUnion(room.Solids);

            //get built in category from user viewable category
            BuiltInCategory myCatEnum = (BuiltInCategory)Enum.Parse(typeof(BuiltInCategory), category.Id.ToString());
       
            //start the transaction
            TransactionManager.Instance.EnsureInTransaction(doc);
            //builds an id collection for later deletion
            ICollection<ElementId> idCollection = new List<ElementId>();
            global::Revit.Elements.DirectShape directShape = global::Revit.Elements.DirectShape.ByGeometry(roomSolid, global::Revit.Elements.Category.ByName("Generic Models"), global::Revit.Elements.DirectShape.DynamoPreviewMaterial, "DirectShape");
            idCollection.Add(directShape.InternalElement.Id);
            TransactionManager.Instance.TransactionTaskDone();
            doc.Regenerate();
            ElementIntersectsElementFilter filter = new ElementIntersectsElementFilter(directShape.InternalElement);
            //build a collector
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Autodesk.Revit.DB.Element> intersectingElementsInternaList = collector.OfCategory(myCatEnum).WhereElementIsNotElementType().WherePasses(filter).ToElements();
            //build a list to output geometry
            List<global::Revit.Elements.Element> intersectingElements = new List<global::Revit.Elements.Element>();
            //append the intersecting elements to the output list
            foreach (Autodesk.Revit.DB.Element thing in intersectingElementsInternaList)
            {
                intersectingElements.Add(thing.ToDSType(false));
            }
            //delete the direct shapes as we do not need them any longer
            TransactionManager.Instance.EnsureInTransaction(doc);
            foreach (ElementId id in idCollection)
            {
                doc.Delete(id);
            }
            TransactionManager.Instance.TransactionTaskDone();

            return intersectingElements;
        }

        /// <summary>
        /// Provides a more stable method of intersecting a curve with a room element for room renumbering workflows.
        /// </summary>
        /// <param name="room"></param>
        /// <param name="curve"></param>
        /// <returns></returns>
        [NodeCategory("Actions")]
        public static List<Curve> IntersectWithCurve(global::Revit.Elements.Room room, Curve curve)
        {
            List<Curve>curveList = null;

            Autodesk.Revit.DB.Architecture.Room internalRoom =
                (Autodesk.Revit.DB.Architecture.Room) room.InternalElement;

            Autodesk.Revit.DB.GeometryElement roomGeometryElement = internalRoom.ClosedShell;

            foreach (GeometryObject geoObj in roomGeometryElement)
            {
                if (geoObj is Autodesk.Revit.DB.Solid)
                {
                    Autodesk.Revit.DB.Solid geoSolid = geoObj as Autodesk.Revit.DB.Solid;
                    Autodesk.Revit.DB.SolidCurveIntersection intersectingCurve = geoSolid.IntersectWithCurve(curve.ToRevitType(), new SolidCurveIntersectionOptions());
                    curveList = intersectingCurve.Select(c => c.ToProtoType()).ToList();
                }
            }

            return curveList;
        }
        /// <summary>
        /// This will return the approximate room dimensions. This is achieved by taking the longest edge and using that to derive the estimated shorter edge.
        /// </summary>
        /// <returns name="dim1">The first dimension. (not sorted as we simply get the longest edge)</returns>
        /// <returns name="dim2">The second dimension. (not sorted as we simply get the longest edge)</returns>
        [MultiReturn(new[] { "dim1", "dim2" })]
        [NodeCategory("Query")]
        public static Dictionary<string, double> ApproximateDimensions(global::Revit.Elements.Room room)
        {
            Autodesk.Revit.DB.Architecture.Room internalRoom =
                room.InternalElement as Autodesk.Revit.DB.Architecture.Room;

            var roomArea = internalRoom.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble();

            var longestEdge = internalRoom.GetBoundarySegments(new SpatialElementBoundaryOptions())[0]
                .Max(bound => bound.GetCurve().ApproximateLength);
            //returns the outputs
            var outInfo = new Dictionary<string, double>
            {
                {"dim1", longestEdge},
                {"dim2", roomArea/longestEdge}
            };
            return outInfo;
        }


        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="room"></param>
        ///// <returns></returns>
        //public static List<global::Revit.Elements.Element> FindNeighbors(global::Revit.Elements.Room room)
        //{
        //    Autodesk.Revit.DB.Architecture.Room internalRoom =
        //        room.InternalElement as Autodesk.Revit.DB.Architecture.Room;

        //    SpatialElementBoundaryOptions spatialElementBoundaryOptions = new SpatialElementBoundaryOptions();
        //    spatialElementBoundaryOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish ;
        //    var spat =  internalRoom.GetBoundarySegments(spatialElementBoundaryOptions).First().First().E .GetCurve().ComputeDerivatives(0.5,true).Origin;
        //    spat.First().
        //    return null;
        //}
    }
}

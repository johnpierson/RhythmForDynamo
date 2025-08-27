using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using Rhythm.Utilities;
using Point = Autodesk.DesignScript.Geometry.Point;
using Solid = Autodesk.DesignScript.Geometry.Solid;

namespace Rhythm.Revit.Elements
{
    // ReSharper disable PossibleNullReferenceException
    /// <summary>
    /// Wrapper class for Area.
    /// </summary>
    public class Areas
    {
        
        private Areas()
        {
        }
        /// <summary>
        /// Get the input area's scheme.
        /// </summary>
        /// <param name="area">The area to check.</param>
        /// <returns name="areaScheme"></returns>
        [NodeCategory("Query")]
        public static global::Revit.Elements.Element AreaScheme(global::Revit.Elements.Element area)
        {
            Autodesk.Revit.DB.Area internalArea = area.InternalElement as Autodesk.Revit.DB.Area;
          
            
            return internalArea.AreaScheme.ToDSType(false);
        }


        /// <summary>
        /// This will report whether or not the area contains the given point.
        /// </summary>
        /// <param name="area">The area to check.</param>
        /// <param name="point">The point to check.</param>
        /// <returns></returns>
        [NodeCategory("Actions")]
        public static bool ContainsPoint(global::Revit.Elements.Element area, Point point)
        {
            Autodesk.Revit.DB.Area internalArea = area.InternalElement as Autodesk.Revit.DB.Area;
            if (internalArea.Area == 0)
            {
                return false;
            }
            XYZ xyz = point.ToXyz();
            return internalArea.AreaContains(xyz);
        }

        /// <summary>
        /// This will return the area at the given point.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <returns name="area">The area found at the point (if available).</returns>
        [NodeCategory("Actions")]
        public static List<global::Revit.Elements.Element> AreaAtPoint(Point point)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            //collect the areas to do some cool stuff
            var area = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Areas).Cast<Autodesk.Revit.DB.Area>().Select(a => a.ToDSType(false)).ToList();

            return area.Where(a => ContainsPoint(a,point)).ToList();
        }

        /// <summary>
        /// This node will retrieve the area's boundaries. The first list is typically the outermost boundaries.
        /// </summary>
        /// <param name="area">The area to extract curves from.</param>
        /// <returns name="boundaries">The boundaries.</returns>
        /// <search>
        /// Area.Boundaries
        /// </search>
        [IsObsolete("Node removed, use the OOTB node, Area.Boundaries")]
        [NodeCategory("Query")]
        public static List<List<Autodesk.DesignScript.Geometry.Curve>> Boundaries(global::Revit.Elements.Element area)
        {
            Autodesk.Revit.DB.Area internalArea = (Autodesk.Revit.DB.Area)area.InternalElement;

            var boundaries = new List<List<Autodesk.DesignScript.Geometry.Curve>>();
            foreach (var segments in internalArea.GetBoundarySegments(new SpatialElementBoundaryOptions()))
            {
                var boundary = new List<Autodesk.DesignScript.Geometry.Curve>();

                foreach (Autodesk.Revit.DB.BoundarySegment segment in segments)
                {
                    boundary.Add(segment.GetCurve().ToProtoType());
                }

                boundaries.Add(boundary);
            }

            return boundaries;
        }

        /// <summary>
        /// This node will retrieve the area's solid geometry.
        /// </summary>
        /// <param name="area">The area to extract solid from.</param>
        /// <param name="areaHeight">A manually input area height. Default value is 10.</param>
        /// <returns name="solid">The solid.</returns>
        /// <search>
        /// Area.Boundaries
        /// </search>
        [NodeCategory("Query")]
        public static Solid Solid(global::Revit.Elements.Element area, double areaHeight = 10.0)
        {

            Autodesk.Revit.DB.Area internalArea = (Autodesk.Revit.DB.Area)area.InternalElement;

            var boundaries = new List<List<Autodesk.DesignScript.Geometry.Curve>>();
            foreach (var segments in internalArea.GetBoundarySegments(new SpatialElementBoundaryOptions()))
            {
                var boundary = new List<Autodesk.DesignScript.Geometry.Curve>();

                foreach (Autodesk.Revit.DB.BoundarySegment segment in segments)
                {
                    boundary.Add(segment.GetCurve().ToProtoType());
                }

                boundaries.Add(boundary);
            }

            Autodesk.DesignScript.Geometry.Solid solid = null;
            List<Autodesk.DesignScript.Geometry.Solid> solidCollection = new List<Solid>();
            int flag = 0;

            while (flag < boundaries.Count)
            {
                if (flag == 0)
                {
                    List<Point> pointList = new List<Point>();
                    foreach (Autodesk.DesignScript.Geometry.Curve b in boundaries[flag])
                    {
                        pointList.Add(b.StartPoint);
                    }

                    Polygon polycurveOutline = Polygon.ByPoints(pointList);
                    solid = polycurveOutline.ExtrudeAsSolid(Vector.ByCoordinates(0, 0, 1), areaHeight);
                }
                else
                {
                    List<Point> pointList = new List<Point>();
                    foreach (Autodesk.DesignScript.Geometry.Curve b in boundaries[flag])
                    {
                        pointList.Add(b.StartPoint);
                    }
                    Polygon polycurveOutlineVoid = Polygon.ByPoints(pointList);
                    solidCollection.Add(polycurveOutlineVoid.ExtrudeAsSolid(Vector.ByCoordinates(0, 0, 1), areaHeight));
                }

                flag++;
            }

            if (solidCollection.Count > 0)
            {
                solid = solid.DifferenceAll(solidCollection);
            }

            return solid;
        }


        /// <summary>
        /// *BETA* This node will retrieve the area(s) at the given point.
        /// This is a VERY SLOW method using solid intersection tests. You have been warned....
        /// </summary>
        /// <param name="point">The point to select area(s) at.</param>
        /// <param name="areaHeight">A manually input area height. Default value is 10.</param>
        /// <returns name="area">The area.</returns>
        /// <search>
        /// Area.GetAreaAtPoint
        /// </search>
        [NodeCategory("Query")]
        public static List<global::Revit.Elements.Element> GetAreaAtPoint(Autodesk.DesignScript.Geometry.Point point, double areaHeight = 10.0)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            List<global::Revit.Elements.Element> areaLocations = new List<global::Revit.Elements.Element>();

            //collect the areas to do some cool stuff
            FilteredElementCollector areaColl = new FilteredElementCollector(doc);
            IList<Autodesk.Revit.DB.Element> areas = areaColl.OfCategory(BuiltInCategory.OST_Areas).ToElements();
            foreach (var area in areas)
            {
                Solid solid = Rhythm.Revit.Elements.Areas.Solid(area.ToDSType(false), areaHeight);
                if (solid.DoesIntersect(point))
                {
                    areaLocations.Add(area.ToDSType(false));
                }
                solid.Dispose();
            }
            return areaLocations;
        }
        /// <summary>
        /// This node will retrieve the area's outermost boundary.
        /// </summary>
        /// <param name="area">The area to extract outermost boundary from.</param>
        /// <returns name="polygon">The outermost polygon.</returns>
        /// <search>
        /// Area.OuterBoundary
        /// </search>
        [NodeCategory("Query")]
        public static Polygon OuterBoundary(global::Revit.Elements.Element area)
        {
            List<List<Autodesk.DesignScript.Geometry.Curve>> boundaries = Rhythm.Revit.Elements.Areas.Boundaries(area);

            List<Point> pointList = new List<Point>();
            foreach (Autodesk.DesignScript.Geometry.Curve b in boundaries[0])
            {
                pointList.Add(b.StartPoint);
            }
            Polygon polycurveOutline = Polygon.ByPoints(pointList);

            return polycurveOutline;
        }

    }
}

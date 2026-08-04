using System;
using System.Collections.Generic;
using System.Windows.Shapes;
using Autodesk.DesignScript.Geometry;

namespace Rhythm.Geometry
{
    public class Polygon
    {
        private Polygon()
        {
        }

        public static Autodesk.DesignScript.Geometry.Point GetPolyLabel(Autodesk.DesignScript.Geometry.Polygon polygon)
        {
            List<double[]> points = new List<double[]>();
            foreach (var point in polygon.Points)
            {
                var pt = new double[] { point.X, point.Y };
                points.Add(pt);
            }


            var internalPolygon = new double[][][]
            {
                points.ToArray()
            };

            var result = Polylabel.Polylabel.GetPolylabel(internalPolygon);

            return Autodesk.DesignScript.Geometry.Point.ByCoordinates(result[0], result[1]);
        }

        public static Circle MinimumCircle(Autodesk.DesignScript.Geometry.Polygon polygon)
        {
            List<double[]> points = new List<double[]>();
            foreach (var point in polygon.Points)
            {
                var pt = new double[] { point.X, point.Y };
                points.Add(pt);
            }


            var internalPolygon = new double[][][]
            {
                points.ToArray()
            };

            var cell = Polylabel.Polylabel.GetCentroidCell(internalPolygon);

            // cell.Max is the centroid's distance to the outline, i.e. the radius. It used to be
            // passed as the point's Z coordinate with no radius argument, so every call returned a
            // circle of the default radius 1 floating at Z = the intended radius.
            // The distance is signed: it is negative when the centroid falls outside the outline,
            // which a concave polygon can produce.
            if (cell.Max <= 0)
            {
                throw new InvalidOperationException(
                    "This polygon's centroid lies outside its own outline, so there is no circle at that point. " +
                    "GetPolyLabel returns a point that is always inside, including for concave polygons.");
            }

            return Circle.ByCenterPointRadius(
                Autodesk.DesignScript.Geometry.Point.ByCoordinates(cell.X, cell.Y, 0), cell.Max);
        }
    }
}

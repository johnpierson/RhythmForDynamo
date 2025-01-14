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

            return Circle.ByCenterPointRadius(Autodesk.DesignScript.Geometry.Point.ByCoordinates(cell.X,cell.Y,cell.Max));
        }
    }
}

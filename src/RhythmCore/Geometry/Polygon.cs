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

            var circle = ComputeInscribedCircle(internalPolygon);

            return Circle.ByCenterPointRadius(
                Autodesk.DesignScript.Geometry.Point.ByCoordinates(circle[0], circle[1], 0), circle[2]);
        }

        /// <summary>
        /// Returns the centre and radius of the largest circle this routine can place inside the
        /// outline, as { x, y, radius }.
        /// </summary>
        /// <remarks>
        /// Split out from <see cref="MinimumCircle"/> so the arithmetic can be tested without
        /// ProtoGeometry: only the final Circle construction needs the geometry libraries, and a
        /// test that cannot run in CI does not protect anything.
        ///
        /// The centroid is the preferred centre, but for a concave outline - an L shape, say - the
        /// centroid can fall outside the polygon entirely, and GetCentroidCell reports a signed
        /// distance that is negative when it does. Those polygons are perfectly valid input, so
        /// rather than reject them the routine falls back to the pole of inaccessibility, which is
        /// by construction the interior point furthest from the outline.
        /// </remarks>
        [Autodesk.DesignScript.Runtime.IsVisibleInDynamoLibrary(false)]
        public static double[] ComputeInscribedCircle(double[][][] internalPolygon)
        {
            var cell = Polylabel.Polylabel.GetCentroidCell(internalPolygon);

            // cell.D is the centroid's signed distance to the outline, i.e. the radius. It used to
            // be passed as the point's Z coordinate with no radius argument at all, so every call
            // returned a circle of the default radius 1 floating at Z = the intended radius.
            if (cell.D > 0)
            {
                return new[] { cell.X, cell.Y, cell.D };
            }

            var pole = Polylabel.Polylabel.GetPolylabel(internalPolygon);
            var poleCell = Polylabel.Polylabel.GetDistanceCell(pole[0], pole[1], internalPolygon);

            if (poleCell.D <= 0)
            {
                throw new InvalidOperationException(
                    "No point inside this polygon could be found, so no circle can be placed in it. " +
                    "Check that the outline is closed and does not self-intersect.");
            }

            return new[] { poleCell.X, poleCell.Y, poleCell.D };
        }
    }
}

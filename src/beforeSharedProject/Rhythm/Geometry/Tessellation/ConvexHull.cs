using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using Revit.GeometryConversion;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace Rhythm.Geometry.Tessellation
{
    /// <summary>
    /// 
    /// </summary>
    public class ConvexHull
    {
        private ConvexHull()
        {
        }

        /// <summary>
        /// Generates a convex hull from given points.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static PolyCurve FromPoints(List<Point> points)
        {
            List<XYZ> xyzList = new List<XYZ>(points.Select(p => p.ToXyz(true)));

            List<Point> pointList = Utilities.ConvexHullUtilities.ConvexHull(xyzList).Select(p => p.ToPoint(true)).ToList();

            return PolyCurve.ByPoints(pointList, true);

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace Rhythm.Utilities
{
    //https://thebuildingcoder.typepad.com/blog/2012/08/room-in-area-predicate-via-point-in-polygon-test.html
    /// <summary>
    /// 
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public static class PointInPolyExtensions
    {
        /// <summary>
        /// Add new point to list, unless already present.
        /// </summary>
        private static void AddToPunten(
          List<XYZ> XYZarray,
          XYZ p1)
        {
            var p = XYZarray.Where(
              c => global::System.Math.Abs(c.X - p1.X) < 0.001
                && global::System.Math.Abs(c.Y - p1.Y) < 0.001)
              .FirstOrDefault();

            if (p == null)
            {
                XYZarray.Add(p1);
            }
        }

        /// <summary>
        /// Return a list of boundary 
        /// points for the given room.
        /// </summary>
        private static List<XYZ> MaakPuntArray(
          Room room)
        {
            SpatialElementBoundaryOptions opt
              = new SpatialElementBoundaryOptions();

            opt.SpatialElementBoundaryLocation
              = SpatialElementBoundaryLocation.Center;

            var boundaries = room.GetBoundarySegments(
              opt);

            return MaakPuntArray(boundaries);
        }

        /// <summary>
        /// Return a list of boundary points 
        /// for the given boundary segments.
        /// </summary>
        private static List<XYZ> MaakPuntArray(
          IList<IList<BoundarySegment>> boundaries)
        {
            List<XYZ> puntArray = new List<XYZ>();
            foreach (var bl in boundaries)
            {
                foreach (var s in bl)
                {
                    Curve c = s.GetCurve();
                    AddToPunten(puntArray, c.GetEndPoint(0));
                    AddToPunten(puntArray, c.GetEndPoint(1));
                }
            }
            puntArray.Add(puntArray.First());
            return puntArray;
        }

        /// <summary>
        /// Return a list of boundary 
        /// points for the given area.
        /// </summary>
        private static List<XYZ> MaakPuntArray(
          Area area)
        {
            SpatialElementBoundaryOptions opt
              = new SpatialElementBoundaryOptions();

            opt.SpatialElementBoundaryLocation
              = SpatialElementBoundaryLocation.Center;

            var boundaries = area.GetBoundarySegments(
              opt);

            return MaakPuntArray(boundaries);
        }

        /// <summary>
        /// Check whether this area contains a given point.
        /// </summary>
        public static bool AreaContains(this Area a, XYZ p1)
        {
            bool ret = false;
            var p = MaakPuntArray(a);
            PointInPoly pp = new PointInPoly();
            ret = pp.PolyGonContains(p, p1);
            return ret;
        }

        /// <summary>
        /// Check whether this room contains a given point.
        /// </summary>
        public static bool RoomContains(this Room r, XYZ p1)
        {
            bool ret = false;
            var p = MaakPuntArray(r);
            PointInPoly pp = new PointInPoly();
            ret = pp.PolyGonContains(p, p1);
            return ret;
        }

        /// <summary>
        /// Project an XYZ point to a UV one in the 
        /// XY plane by simply dropping the Z coordinate.
        /// </summary>
        public static UV TOUV(this XYZ point)
        {
            UV ret = new UV(point.X, point.Y);
            return ret;
        }
    }
}

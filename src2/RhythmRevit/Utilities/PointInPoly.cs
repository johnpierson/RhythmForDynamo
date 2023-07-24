using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Quadrant = System.Int32;

namespace Rhythm.Utilities
{
    //https://thebuildingcoder.typepad.com/blog/2012/08/room-in-area-predicate-via-point-in-polygon-test.html
    /// <summary>
    /// 
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class UVArray
    {
        List<UV> arrayPoints;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="XYZArray"></param>
        public UVArray(List<XYZ> XYZArray)
        {
            arrayPoints = new List<UV>();
            foreach (var p in XYZArray)
            {
                arrayPoints.Add(p.TOUV());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public UV get_Item(int i)
        {
            return arrayPoints[i];
        }

        /// <summary>
        /// 
        /// </summary>
        public int Size
        {
            get
            {
                return arrayPoints.Count;
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class PointInPoly
    {
        /// <summary>
        /// Determine the quadrant of a polygon vertex 
        /// relative to the test point.
        /// </summary>
        Quadrant GetQuadrant(UV vertex, UV p)
        {
            return (vertex.U > p.U)
              ? ((vertex.V > p.V) ? 0 : 3)
              : ((vertex.V > p.V) ? 1 : 2);
        }

        /// <summary>
        /// Determine the X intercept of a polygon edge 
        /// with a horizontal line at the Y value of the 
        /// test point.
        /// </summary>
        double X_intercept(UV p, UV q, double y)
        {
            Debug.Assert(0 != (p.V - q.V),
              "unexpected horizontal segment");

            return q.U
              - ((q.V - y)
                * ((p.U - q.U) / (p.V - q.V)));
        }

        void AdjustDelta(
          ref int delta,
          UV vertex,
          UV next_vertex,
          UV p)
        {
            switch (delta)
            {
                // make quadrant deltas wrap around:
                case 3: delta = -1; break;
                case -3: delta = 1; break;
                // check if went around point cw or ccw:
                case 2:
                case -2:
                    if (X_intercept(vertex, next_vertex, p.V)
                      > p.U)
                    {
                        delta = -delta;
                    }
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xyZArray"></param>
        /// <param name="p1"></param>
        /// <returns></returns>
        public bool PolyGonContains(List<XYZ> xyZArray, XYZ p1)
        {
            UVArray uva = new UVArray(xyZArray);
            return PolygonContains(uva, p1.TOUV());
        }

        /// <summary>
        /// Determine whether given 2D point lies within 
        /// the polygon.
        /// 
        /// Written by Jeremy Tammik, Autodesk, 2009-09-23, 
        /// based on code that I wrote back in 1996 in C++, 
        /// which in turn was based on C code from the 
        /// article "An Incremental Angle Point in Polygon 
        /// Test" by Kevin Weiler, Autodesk, in "Graphics 
        /// Gems IV", Academic Press, 1994.
        /// 
        /// Copyright (C) 2009 by Jeremy Tammik. All 
        /// rights reserved.
        /// 
        /// This code may be freely used. Please preserve 
        /// this comment.
        /// </summary>
        public bool PolygonContains(
          UVArray polygon,
          UV point)
        {
            // initialize
            Quadrant quad = GetQuadrant(
              polygon.get_Item(0), point);

            Quadrant angle = 0;

            // loop on all vertices of polygon
            Quadrant next_quad, delta;
            int n = polygon.Size;
            for (int i = 0; i < n; ++i)
            {
                UV vertex = polygon.get_Item(i);

                UV next_vertex = polygon.get_Item(
                  (i + 1 < n) ? i + 1 : 0);

                // calculate quadrant and delta from last quadrant

                next_quad = GetQuadrant(next_vertex, point);
                delta = next_quad - quad;

                AdjustDelta(
                  ref delta, vertex, next_vertex, point);

                // add delta to total angle sum
                angle = angle + delta;

                // increment for next step
                quad = next_quad;
            }

            // complete 360 degrees (angle of + 4 or -4 ) 
            // means inside

            return (angle == +4) || (angle == -4);

            // odd number of windings rule:
            // if (angle & 4) return INSIDE; else return OUTSIDE;
            // non-zero winding rule:
            // if (angle != 0) return INSIDE; else return OUTSIDE;
        }
    }
}
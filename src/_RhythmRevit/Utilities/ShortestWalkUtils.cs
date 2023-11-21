using Autodesk.DesignScript.Geometry;
using ShortestWalk.Geometry;
using System;
using System.Collections.Generic;
using Rhythm.ShortestWalk.Geometry;

namespace Utilities
{
    internal class ShortestWalkUtils
    {
        private static readonly Predicate<Curve> RemoveNullAndInvalidDelegate = new Predicate<Curve>(ShortestWalkUtils.RemoveNull);
        private static readonly Predicate<Line> RemoveInvalidDelegate = new Predicate<Line>(ShortestWalkUtils.RemoveInvalid);
        private static readonly Predicate<double> _isNegative = new Predicate<double>(ShortestWalkUtils.IsNegative);
        private readonly List<Curve> _curveNetwork;
        private readonly List<double> _lengths;
        private readonly List<Line> _paths;
        private List<Curve> _resultCurves;
        private List<int[]> _resultLinks;
        private List<bool[]> _resultDirections;
        private List<double> _resultLengths;

        public List<Curve> ResultCurves => this._resultCurves;

        public List<int[]> ResultLinks => this._resultLinks;

        public List<bool[]> ResultDirections => this._resultDirections;

        public List<double> ResultLengths => this._resultLengths;

        public ShortestWalkUtils(List<Curve> CurveNetwork, List<double> Lengths, List<Line> Paths)
        {
            this._curveNetwork = CurveNetwork;
            this._lengths = Lengths;
            this._paths = Paths;
        }

        private static bool RemoveNull(Curve obj) => obj == null;

        private static bool RemoveInvalid(Line obj) => obj == null;

        private static bool IsNegative(double number) => number < 0.0;

        public void SolveShortestWalk()
        {
            this._resultCurves = new List<Curve>();
            this._resultLinks = new List<int[]>();
            this._resultDirections = new List<bool[]>();
            this._resultLengths = new List<double>();
            List<Curve> curvenetwork = this._curveNetwork;
            List<double> lengths = this._lengths;
            List<Line> paths = this._paths;
            if (curvenetwork.Count <= 0 || paths.Count <= 0 || lengths.FindIndex(ShortestWalkUtils._isNegative) != -1)
                return;
            curvenetwork.RemoveAll(ShortestWalkUtils.RemoveNullAndInvalidDelegate);
            paths.RemoveAll(ShortestWalkUtils.RemoveInvalidDelegate);
            if (curvenetwork.Count < 1)
                return;
            CurvesTopology top = new CurvesTopology((IList<Curve>)curvenetwork, 0.001);
            PathMethod pathMethod;
            if (lengths.Count == 0)
            {
                IList<double> dist = (IList<double>)top.MeasureAllEdgeLengths();
                pathMethod = (PathMethod)new AStar(top, dist);
            }
            else if (lengths.Count == 1)
            {
                pathMethod = (PathMethod)new Dijkstra(top, lengths[0]);
            }
            else
            {
                IList<double> doubleList = (IList<double>)lengths;
                if (doubleList.Count < top.EdgeLength)
                    doubleList = (IList<double>)new ListByPattern<double>(doubleList, top.EdgeLength);
                bool flag = true;
                for (int index = 0; index < top.EdgeLength; ++index)
                {
                    if (top.LinearDistanceAt(index) > doubleList[index])
                    {
                        flag = false;
                        break;
                    }
                }
                pathMethod = !flag ? (PathMethod)new Dijkstra(top, doubleList) : (PathMethod)new AStar(top, doubleList);
            }
            for (int index = 0; index < paths.Count; ++index)
            {
                Line line = paths[index];
                int closestNode1 = top.GetClosestNode(((Curve)line).StartPoint);
                int closestNode2 = top.GetClosestNode(((Curve)line).EndPoint);
                if (closestNode1 == closestNode2)
                {
                    this._resultCurves.Add((Curve)null);
                }
                else
                {
                    Curve curve = pathMethod.Cross(closestNode1, closestNode2, out int[] _, out var edges, out var eDirs, out var totLength);
                    if (curve != null)
                    {
                        this._resultLinks.Add(edges);
                        this._resultDirections.Add(eDirs);
                        this._resultLengths.Add(totLength);
                    }
                    this._resultCurves.Add(curve);
                }
            }
        }
    }
}
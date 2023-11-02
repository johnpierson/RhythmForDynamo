using Autodesk.DesignScript.Geometry;
using System;
using System.Collections.Generic;

namespace ShortestWalk.Geometry
{
    internal abstract class PathMethod
    {
        protected readonly CurvesTopology m_top;
        protected readonly IList<double> m_dist;

        public PathMethod(CurvesTopology top, IList<double> dist)
        {
            this.m_top = top ?? throw new ArgumentNullException(nameof(top));
            if (dist == null)
                throw new ArgumentNullException(nameof(dist));
            if (dist.Count < this.m_top.EdgeLength)
                throw new ArgumentOutOfRangeException(nameof(dist), "There should be one distance for each edge");
            this.m_dist = dist;
        }

        public virtual Curve Cross(int from, int to) => this.Cross(from, to, out int[] _, out int[] _, out bool[] _, out double _);

        public abstract Curve Cross(
          int from,
          int to,
          out int[] nodes,
          out int[] edges,
          out bool[] eDirs,
          out double totLength);

        protected Curve ReconstructPath(
          int[] cameFrom,
          int currentNode,
          out int[] nodes,
          out int[] edges,
          out bool[] edgeDir,
          out double totLength)
        {
            List<int> intList1 = new List<int>();
            for (; currentNode != -1; currentNode = cameFrom[currentNode])
                intList1.Add(currentNode);
            intList1.Reverse();
            nodes = intList1.ToArray();
            List<int> intList2 = new List<int>();
            List<bool> boolList = new List<bool>();
            currentNode = nodes[0];
            for (int index = 1; index < nodes.Length; ++index)
            {
                int nxt = nodes[index];
                int edge = this.FindEdge(currentNode, nxt, out var rev);
                intList2.Add(edge);
                boolList.Add(rev);
                currentNode = nxt;
            }
            edges = intList2.ToArray();
            edgeDir = boolList.ToArray();
            totLength = 0.0;
            List<Curve> curveList = new List<Curve>();
            for (int index = 0; index < intList2.Count; ++index)
            {
                int num = intList2[index];
                Curve curve = this.m_top.CurveAt(num);
                if (!boolList[index])
                    curve.Reverse();
                curveList.Add(curve);
                totLength += this.m_dist[num];
            }
            return (Curve)PolyCurve.ByJoinedCurves((IEnumerable<Curve>)curveList, 0.001);
        }

        protected int FindEdge(int currentNode, int nxt, out bool rev)
        {
            NodeAddress nodeAddress = this.m_top.NodeAt(currentNode);
            double maxValue = double.MaxValue;
            int num1 = -1;
            rev = false;
            for (int i = 0; i < nodeAddress.EdgeCount; ++i)
            {
                int num2 = nodeAddress.EdgeIndexAt(i, this.m_top);
                if (this.m_top.EdgeAt(num2).OtherVertex(currentNode) == nxt && this.m_dist[num2] < maxValue)
                {
                    rev = nodeAddress.RevAt(i, this.m_top);
                    num1 = num2;
                    maxValue = this.m_dist[num2];
                }
            }
            return num1 != -1 ? num1 : throw new KeyNotFoundException("Vertex currentNode is not linked to nxt");
        }

        protected void CheckArguments(int from, int to)
        {
            if (from < 0)
                throw new ArgumentOutOfRangeException(nameof(from), "from is less than 0");
            if (to < 0)
                throw new ArgumentOutOfRangeException(nameof(to), "to is less than 0");
            if (from >= this.m_top.VertexLength)
                throw new ArgumentOutOfRangeException(nameof(from), "from is more than vertex length");
            if (to >= this.m_top.VertexLength)
                throw new ArgumentOutOfRangeException(nameof(to), "to is more than vertex length");
            if (from == to)
                throw new ArgumentException("Walking indices from and to are the same");
        }

        public static PathMethod FromMode(
          SearchMode sm,
          CurvesTopology crvTopology,
          double[] distances)
        {
            PathMethod pathMethod;
            switch (sm)
            {
                case SearchMode.CurveLength:
                    pathMethod = (PathMethod)new AStar(crvTopology, (IList<double>)distances);
                    break;
                case SearchMode.LinearDistance:
                    pathMethod = (PathMethod)new AStar(crvTopology, (IList<double>)distances);
                    break;
                case SearchMode.Links:
                    if (distances != null)
                        throw new ArgumentException("If you use Links mode, then distances must be null as it will be ignored", nameof(distances));
                    pathMethod = (PathMethod)new Dijkstra(crvTopology);
                    break;
                default:
                    throw new ApplicationException("No behaviour is defined for this enum value");
            }
            return pathMethod;
        }

        protected static int FindMinimumScoreAmongOpen(IList<int> open, double[] fScore)
        {
            int minimumScoreAmongOpen = open[0];
            double num1 = fScore[minimumScoreAmongOpen];
            for (int index1 = 1; index1 < open.Count; ++index1)
            {
                int index2 = open[index1];
                double num2 = fScore[index2];
                if (num2 < num1)
                {
                    minimumScoreAmongOpen = index2;
                    num1 = num2;
                }
            }
            return minimumScoreAmongOpen;
        }
    }
}

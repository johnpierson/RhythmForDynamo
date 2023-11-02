using Autodesk.DesignScript.Geometry;
using System.Collections.Generic;

namespace ShortestWalk.Geometry
{
    internal class AStar : PathMethod
    {
        public AStar(CurvesTopology top, IList<double> dist)
          : base(top, dist)
        {
        }

        public override Curve Cross(
          int from,
          int to,
          out int[] nodes,
          out int[] edges,
          out bool[] eDirs,
          out double totLength)
        {
            this.CheckArguments(from, to);
            Dictionary<int, byte> dictionary = new Dictionary<int, byte>(this.m_top.EdgeLength / 4);
            SortedList<int, byte> sortedList = new SortedList<int, byte>(this.m_top.EdgeLength / 5);
            sortedList.Add(from, (byte)0);
            double[] numArray1 = new double[this.m_top.VertexLength];
            double[] numArray2 = new double[this.m_top.VertexLength];
            double[] fScore = new double[this.m_top.VertexLength];
            int[] cameFrom = new int[this.m_top.VertexLength];
            for (int index = 0; index < cameFrom.Length; ++index)
                cameFrom[index] = -1;
            numArray2[from] = AStar.HeuristicEstimateDistance(this.m_top, from, to);
            fScore[from] = numArray2[from];
            while (sortedList.Count > 0)
            {
                int minimumScoreAmongOpen = PathMethod.FindMinimumScoreAmongOpen(sortedList.Keys, fScore);
                if (minimumScoreAmongOpen == to)
                    return this.ReconstructPath(cameFrom, to, out nodes, out edges, out eDirs, out totLength);
                sortedList.Remove(minimumScoreAmongOpen);
                dictionary.Add(minimumScoreAmongOpen, (byte)0);
                NodeAddress nodeAddress = this.m_top.NodeAt(minimumScoreAmongOpen);
                for (int i = 0; i < nodeAddress.EdgeCount; ++i)
                {
                    int num1 = nodeAddress.EdgeIndexAt(i, this.m_top);
                    int index = this.m_top.EdgeAt(num1).OtherVertex(minimumScoreAmongOpen);
                    if (!dictionary.ContainsKey(index))
                    {
                        double num2 = numArray1[minimumScoreAmongOpen] + this.m_dist[num1];
                        bool flag;
                        if (!sortedList.ContainsKey(index))
                        {
                            sortedList.Add(index, (byte)0);
                            flag = true;
                        }
                        else
                            flag = num2 < numArray1[index];
                        if (flag)
                        {
                            cameFrom[index] = minimumScoreAmongOpen;
                            numArray1[index] = num2;
                            numArray2[index] = AStar.HeuristicEstimateDistance(this.m_top, index, to);
                            fScore[index] = numArray1[index] + numArray2[index];
                        }
                    }
                }
            }
            nodes = edges = (int[])null;
            eDirs = (bool[])null;
            totLength = double.NaN;
            return (Curve)null;
        }

        protected static double HeuristicEstimateDistance(CurvesTopology top, int to, int y) => ((Autodesk.DesignScript.Geometry.Geometry)top.VertexAt(y)).DistanceTo((Autodesk.DesignScript.Geometry.Geometry)top.VertexAt(to));
    }
}
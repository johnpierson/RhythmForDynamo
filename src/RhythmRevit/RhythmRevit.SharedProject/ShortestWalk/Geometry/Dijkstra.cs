using Autodesk.DesignScript.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ShortestWalk.Geometry
{
    internal class Dijkstra : PathMethod
    {
        public Dijkstra(CurvesTopology top)
          : this(top, (IList<double>)new Dijkstra.AlwaysFixed(1.0, top.EdgeLength))
        {
        }

        public Dijkstra(CurvesTopology top, double value)
          : this(top, (IList<double>)new Dijkstra.AlwaysFixed(value, top.EdgeLength))
        {
        }

        public Dijkstra(CurvesTopology top, IList<double> dist)
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
            double[] fScore = new double[this.m_top.VertexLength];
            int[] cameFrom = new int[this.m_top.VertexLength];
            for (int index = 0; index < cameFrom.Length; ++index)
            {
                fScore[index] = double.PositiveInfinity;
                cameFrom[index] = -1;
            }
            List<int> open = new List<int>(cameFrom.Length);
            for (int index = 0; index < cameFrom.Length; ++index)
                open.Add(index);
            fScore[from] = 0.0;
            while (open.Count > 0)
            {
                int minimumScoreAmongOpen = PathMethod.FindMinimumScoreAmongOpen((IList<int>)open, fScore);
                if (double.IsPositiveInfinity(fScore[minimumScoreAmongOpen]))
                {
                    nodes = edges = (int[])null;
                    eDirs = (bool[])null;
                    totLength = double.NaN;
                    return (Curve)null;
                }
                if (minimumScoreAmongOpen == to)
                    return this.ReconstructPath(cameFrom, to, out nodes, out edges, out eDirs, out totLength);
                open.Remove(minimumScoreAmongOpen);
                NodeAddress nodeAddress = this.m_top.NodeAt(minimumScoreAmongOpen);
                for (int i = 0; i < nodeAddress.EdgeCount; ++i)
                {
                    int num1 = nodeAddress.EdgeIndexAt(i, this.m_top);
                    int index = this.m_top.EdgeAt(num1).OtherVertex(minimumScoreAmongOpen);
                    if (open.Contains(index))
                    {
                        double num2 = fScore[minimumScoreAmongOpen] + this.m_dist[num1];
                        if (num2 < fScore[index])
                        {
                            fScore[index] = num2;
                            cameFrom[index] = minimumScoreAmongOpen;
                        }
                    }
                }
            }
            throw new ApplicationException("Error in topoogy.");
        }

        private class AlwaysFixed : IList<double>, ICollection<double>, IEnumerable<double>, IEnumerable
        {
            private int _count;
            private double _value;

            public AlwaysFixed(int count)
              : this(1.0, count)
            {
            }

            public AlwaysFixed(double value, int count)
            {
                this._count = count;
                this._value = value;
            }

            public int IndexOf(double item) => item != this._value || this._count <= 0 ? -1 : 0;

            public void Insert(int index, double item) => throw new NotImplementedException();

            void IList<double>.RemoveAt(int index) => throw new NotImplementedException();

            public double this[int index]
            {
                get => this._value;
                set => throw new NotSupportedException();
            }

            void ICollection<double>.Add(double item) => throw new NotSupportedException();

            void ICollection<double>.Clear() => throw new NotSupportedException();

            bool ICollection<double>.Contains(double item) => this._count > 0 && item == this._value;

            void ICollection<double>.CopyTo(double[] array, int arrayIndex) => throw new NotSupportedException();

            public int Count => this._count;

            bool ICollection<double>.IsReadOnly => true;

            bool ICollection<double>.Remove(double item) => throw new NotSupportedException();

            public IEnumerator<double> GetEnumerator()
            {
                for (int i = 0; i < this.Count; ++i)
                    yield return this._value;
            }

            IEnumerator IEnumerable.GetEnumerator() => (IEnumerator)this.GetEnumerator();
        }
    }
}

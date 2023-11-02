using Autodesk.DesignScript.Geometry;
using System;
using System.Collections.Generic;

namespace ShortestWalk.Geometry
{
    internal class CurvesTopology
    {
        private readonly double _tolerance;
        private readonly Curve[] _curves;
        private readonly NodeAddress[] _vertices;
        private readonly EdgeAddress[] _edges;
        private readonly Point[] _vPositions;
        private readonly int[] _vEdges;
        private readonly bool[] _vEdgesRev;

        public CurvesTopology(IList<Curve> curves)
          : this(curves, 0.0)
        {
        }

        public CurvesTopology(IList<Curve> curves, double tolerance)
        {
            if (curves == null)
                throw new ArgumentNullException(nameof(curves));
            if (tolerance < 0.0)
                throw new ArgumentOutOfRangeException(nameof(tolerance), "tolerance cannot be negative");
            if (((ICollection<Curve>)curves).Count < 1)
                throw new ArgumentException("there are 0 curves in the topology", nameof(curves));
            if (((ICollection<Curve>)curves).Contains((Curve)null))
                throw new ArgumentException("curves contains a null reference", nameof(curves));
            this._tolerance = tolerance;
            this._curves = new Curve[((ICollection<Curve>)curves).Count];
            ((ICollection<Curve>)curves).CopyTo(this._curves, 0);
            CurvesTopology.VertexOnCurve[] vertexOnCurveArray = this.CopyVertexEndings(this._curves);
            Point[] pointArray = new Point[vertexOnCurveArray.Length];
            for (int index = 0; index < vertexOnCurveArray.Length; ++index)
                pointArray[index] = vertexOnCurveArray[index].Locate(this._curves);
            CurvesTopology.VertexEndsPositionalScale endsPositionalScale = new CurvesTopology.VertexEndsPositionalScale(pointArray);
            Array.Sort<CurvesTopology.VertexOnCurve>(vertexOnCurveArray, (IComparer<CurvesTopology.VertexOnCurve>)endsPositionalScale);
            int[] map;
            IList<int> weldedVertexBounds;
            if (tolerance == 0.0)
            {
                weldedVertexBounds = (IList<int>)CurvesTopology.RepetitionCounts(vertexOnCurveArray, this._curves, out map);
                this._vPositions = new Point[weldedVertexBounds.Count];
                this.SetupVerticesPositions(vertexOnCurveArray, weldedVertexBounds);
            }
            else
            {
                for (int index = 0; index < vertexOnCurveArray.Length; ++index)
                    pointArray[index] = vertexOnCurveArray[index].Locate(this._curves);
                weldedVertexBounds = CurvesTopology.RepetitionCounts(pointArray, out map, this._tolerance);
                this._vPositions = new Point[weldedVertexBounds.Count];
                this.SetupVerticesPositionsThatNeedToBeMapped(vertexOnCurveArray, map);
            }
            this._edges = new EdgeAddress[this._curves.Length];
            this.SetupVerticesEndings(vertexOnCurveArray, map);
            this._vertices = new NodeAddress[weldedVertexBounds.Count];
            this.SetupEdgesToVerticesCounts(weldedVertexBounds);
            this._vEdges = new int[this._edges.Length * 2];
            this._vEdgesRev = new bool[this._edges.Length * 2];
            this.SetupVerticesToEdges();
        }

        private CurvesTopology.VertexOnCurve[] CopyVertexEndings(Curve[] curves)
        {
            CurvesTopology.VertexOnCurve[] vertexOnCurveArray = new CurvesTopology.VertexOnCurve[this._curves.Length * 2];
            int vertexPosition1 = 0;
            for (int index = 0; index < this._curves.Length; ++index)
            {
                vertexOnCurveArray[vertexPosition1] = new CurvesTopology.VertexOnCurve(vertexPosition1);
                int vertexPosition2 = vertexPosition1 + 1;
                vertexOnCurveArray[vertexPosition2] = new CurvesTopology.VertexOnCurve(vertexPosition2);
                vertexPosition1 = vertexPosition2 + 1;
            }
            return vertexOnCurveArray;
        }

        private void SetupVerticesPositions(
          CurvesTopology.VertexOnCurve[] vls,
          IList<int> weldedVertexBounds)
        {
            int index1 = 0;
            for (int index2 = 0; index2 < weldedVertexBounds.Count; ++index2)
            {
                CurvesTopology.VertexOnCurve vl = vls[index1];
                this._vPositions[index2] = vl.Locate(this._curves);
                index1 += weldedVertexBounds[index2];
            }
        }

        private void SetupVerticesPositionsThatNeedToBeMapped(
          CurvesTopology.VertexOnCurve[] vls,
          int[] map)
        {
            for (int index = 0; index < map.Length; ++index)
            {
                CurvesTopology.VertexOnCurve vl = vls[index];
                this._vPositions[map[index]] = vl.Locate(this._curves);
            }
        }

        private void SetupVerticesEndings(
          CurvesTopology.VertexOnCurve[] vls,
          int[] verticesToWeldedVertices)
        {
            for (int index = 0; index < vls.Length; ++index)
            {
                CurvesTopology.VertexOnCurve vl = vls[index];
                this._edges[vl.LinePosition].SetStartOrEnd(vl.IsStart, verticesToWeldedVertices[index]);
            }
        }

        private void SetupEdgesToVerticesCounts(IList<int> weldedVertexBounds)
        {
            int num = 0;
            for (int index = 0; index < weldedVertexBounds.Count; ++index)
            {
                this._vertices[index].EdgeStart = num;
                num += weldedVertexBounds[index];
                this._vertices[index].Topology = this;
            }
        }

        private void SetupVerticesToEdges()
        {
            for (int index1 = 0; index1 < this._edges.Length; ++index1)
            {
                int a = this._edges[index1].A;
                int index2 = this._vertices[a].EdgeCount + this._vertices[a].EdgeStart;
                this._vEdges[index2] = index1;
                this._vEdgesRev[index2] = true;
                ++this._vertices[a].EdgeCount;
                int b = this._edges[index1].B;
                this._vEdges[this._vertices[b].EdgeCount + this._vertices[b].EdgeStart] = index1;
                ++this._vertices[b].EdgeCount;
            }
        }

        private static int CountSubsequentDifferent(CurvesTopology.VertexOnCurve[] vls, Curve[] lines)
        {
            int num = 0;
            if (vls.Length != 0)
            {
                Point point1 = vls[0].Locate(lines);
                for (int index = 1; index < vls.Length; ++index)
                {
                    Point point2 = vls[index].Locate(lines);
                    if (point1 != point2)
                    {
                        point1 = point2;
                        ++num;
                    }
                }
            }
            return num;
        }

        private static IList<int> RepetitionCounts(Point[] pts, out int[] map, double tolerance)
        {
            double sqTolerance = tolerance * tolerance;
            int length = pts.Length;
            map = new int[length];
            for (int index = 0; index < length; ++index)
                map[index] = -1;
            List<int> intList = new List<int>(length / 4);
            int num1 = 0;
            for (int index1 = 0; index1 < length; ++index1)
            {
                if (map[index1] == -1)
                {
                    int num2 = 1;
                    Point pt = pts[index1];
                    for (int index2 = index1 + 1; index2 < length; ++index2)
                    {
                        CurvesTopology.ToleranceState toleranceState = CurvesTopology.ArePointsInTolerance(pt, pts[index2], tolerance, sqTolerance);
                        if (toleranceState == CurvesTopology.ToleranceState.InTolerance)
                        {
                            map[index2] = num1;
                            ++num2;
                        }
                        if (toleranceState == CurvesTopology.ToleranceState.OutOfToleranceAndStop)
                            break;
                    }
                    intList.Add(num2);
                    map[index1] = num1++;
                }
            }
            return (IList<int>)intList;
        }

        private static CurvesTopology.ToleranceState ArePointsInTolerance(
          Point p0,
          Point p1,
          double tolerance,
          double sqTolerance)
        {
            double num1 = p1.X - p0.X;
            if (num1 > tolerance)
                return CurvesTopology.ToleranceState.OutOfToleranceAndStop;
            double num2 = p1.Y - p0.Y;
            if (Math.Abs(num2) > tolerance)
                return CurvesTopology.ToleranceState.OutOfToleranceAndContinue;
            double num3 = p1.Z - p0.Z;
            return num1 * num1 + num2 * num2 + num3 * num3 <= sqTolerance ? CurvesTopology.ToleranceState.InTolerance : CurvesTopology.ToleranceState.OutOfToleranceAndContinue;
        }

        private static int[] RepetitionCounts(
          CurvesTopology.VertexOnCurve[] vls,
          Curve[] crvs,
          out int[] map)
        {
            int[] numArray = new int[CurvesTopology.CountSubsequentDifferent(vls, crvs) + 1];
            for (int index = 0; index < numArray.Length; ++index)
                numArray[index] = 1;
            map = new int[vls.Length];
            if (numArray.Length > 2)
            {
                int index1 = 0;
                Point point1 = vls[0].Locate(crvs);
                for (int index2 = 1; index2 < vls.Length; ++index2)
                {
                    Point point2 = vls[index2].Locate(crvs);
                    if (point1 == point2)
                    {
                        ++numArray[index1];
                    }
                    else
                    {
                        point1 = point2;
                        ++index1;
                    }
                    map[index2] = index1;
                }
            }
            return numArray;
        }

        internal int GetVertexToEdgeIndexFromArrayAt(int i) => this._vEdges[i];

        internal bool GetIsVertexEdgeRevOrientedFromArray(int i) => this._vEdgesRev[i];

        public EdgeAddress EdgeAt(int i) => this._edges[i];

        public Curve CurveAt(int i) => this._curves[i];

        public NodeAddress NodeAt(int i) => this._vertices[i];

        public Point VertexAt(int i) => this._vPositions[i];

        public int VertexLength => this._vertices.Length;

        public int EdgeLength => this._edges.Length;

        public int GetVertexIndexOf(Point position)
        {
            double num = this._tolerance * this._tolerance;
            for (int i = 0; i < this.VertexLength; ++i)
            {
                if (((Autodesk.DesignScript.Geometry.Geometry)this.VertexAt(i)).DistanceTo((Autodesk.DesignScript.Geometry.Geometry)position) <= num)
                    return i;
            }
            return -1;
        }

        public int GetClosestNode(Point position)
        {
            int closestNode = 0;
            double num1 = ((Autodesk.DesignScript.Geometry.Geometry)this.VertexAt(0)).DistanceTo((Autodesk.DesignScript.Geometry.Geometry)position);
            for (int i = 1; i < this.VertexLength; ++i)
            {
                double num2 = ((Autodesk.DesignScript.Geometry.Geometry)this.VertexAt(i)).DistanceTo((Autodesk.DesignScript.Geometry.Geometry)position);
                if (num2 < num1)
                {
                    num1 = num2;
                    closestNode = i;
                }
            }
            return closestNode;
        }

        public double[] MeasureAllEdgeLengths()
        {
            double[] numArray = new double[this.EdgeLength];
            for (int i = 0; i < this.EdgeLength; ++i)
                numArray[i] = this.CurveAt(i).Length;
            return numArray;
        }

        public double[] MeasureAllEdgeLinearDistances()
        {
            double[] numArray = new double[this.EdgeLength];
            for (int i = 0; i < this.EdgeLength; ++i)
            {
                EdgeAddress edge = this.EdgeAt(i);
                numArray[i] = this.LinearDistanceAt(edge);
            }
            return numArray;
        }

        public double LinearDistanceAt(EdgeAddress edge) => ((Autodesk.DesignScript.Geometry.Geometry)this.VertexAt(edge.A)).DistanceTo((Autodesk.DesignScript.Geometry.Geometry)this.VertexAt(edge.B));

        public double LinearDistanceAt(int edgeIndex) => this.LinearDistanceAt(this.EdgeAt(edgeIndex));

        private enum ToleranceState
        {
            OutOfToleranceAndContinue,
            InTolerance,
            OutOfToleranceAndStop,
        }

        private struct VertexOnCurve
        {
            private readonly int _vertexPosition;

            public VertexOnCurve(int vertexPosition) => this._vertexPosition = vertexPosition;

            public VertexOnCurve(int curvePosition, bool start) => this._vertexPosition = curvePosition << 1 | (start ? 0 : 1);

            public int LinePosition => this._vertexPosition >> 1;

            public bool IsStart => (this._vertexPosition & 1) == 0;

            public int ConsecutiveVertex => this._vertexPosition;

            public override string ToString() => string.Format("curves[{0}].{1}", (object)this.LinePosition.ToString(), this.IsStart ? (object)"Start" : (object)"End");

            public Point Locate(Curve[] input) => this.IsStart ? input[this.LinePosition].StartPoint : input[this.LinePosition].EndPoint;
        }

        private sealed class VertexEndsPositionalScale : IComparer<CurvesTopology.VertexOnCurve>
        {
            private readonly Point[] _endings;

            public VertexEndsPositionalScale(Point[] endings) => this._endings = endings;

            public int Compare(CurvesTopology.VertexOnCurve left, CurvesTopology.VertexOnCurve right)
            {
                Point ending1 = this._endings[left.ConsecutiveVertex];
                Point ending2 = this._endings[right.ConsecutiveVertex];
                if (ending1.X < ending2.X)
                    return -1;
                if (ending1.X > ending2.X)
                    return 1;
                if (ending1.Y < ending2.Y)
                    return -1;
                if (ending1.Y > ending2.Y)
                    return 1;
                if (ending1.Z < ending2.Z)
                    return -1;
                return ending1.Z > ending2.Z ? 1 : 0;
            }
        }
    }
}

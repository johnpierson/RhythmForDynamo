using System;
using System.Diagnostics;
using System.Text;

namespace ShortestWalk.Geometry
{
    internal struct NodeAddress
    {
        private int _edgeStart;
        private int _edgeCount;
        private CurvesTopology _topology;

        public NodeAddress(int edgeStart, int edgeCount)
        {
            this._edgeStart = edgeStart;
            this._edgeCount = edgeCount;
            this._topology = (CurvesTopology)null;
        }

        internal int EdgeStart
        {
            get => this._edgeStart;
            set
            {
                this.DoNotAllowNegativeIndices(value);
                this._edgeStart = value;
            }
        }

        public int EdgeCount
        {
            get => this._edgeCount;
            internal set
            {
                this.DoNotAllowNegativeIndices(value);
                this._edgeCount = value;
            }
        }

        [Conditional("DEBUG")]
        private void DoNotAllowNegativeIndices(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));
        }

        public int EdgeIndexAt(int i, CurvesTopology top)
        {
            if (i < 0 || i >= this.EdgeCount)
                throw new ArgumentOutOfRangeException(nameof(i), "index must be smaller than EdgeCount and larger than 0");
            return top.GetVertexToEdgeIndexFromArrayAt(this._edgeStart + i);
        }

        public EdgeAddress EdgeAt(int i, CurvesTopology top) => i >= 0 && i < this.EdgeCount ? top.EdgeAt(this.EdgeIndexAt(i, top)) : throw new ArgumentOutOfRangeException(nameof(i), "index must be smaller than EdgeCount and larger than 0");

        public bool RevAt(int i, CurvesTopology top)
        {
            if (i < 0 || i >= this.EdgeCount)
                throw new ArgumentOutOfRangeException(nameof(i), "index must be smaller than EdgeCount and larger than 0");
            return top.GetIsVertexEdgeRevOrientedFromArray(this._edgeStart + i);
        }

        public CurvesTopology Topology
        {
            get => this._topology;
            set => this._topology = value;
        }

        public override string ToString()
        {
            if (this._topology == null)
                return "Set topology for preview";
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(this.EdgeIndexAt(0, this._topology));
            for (int i = 1; i < this._edgeCount; ++i)
            {
                stringBuilder.Append(", ");
                stringBuilder.Append(this.EdgeIndexAt(i, this._topology));
            }
            return stringBuilder.ToString();
        }
    }
}

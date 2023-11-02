using System;

namespace ShortestWalk.Geometry
{
    internal struct EdgeAddress
    {
        private int _a;
        private int _b;

        public EdgeAddress(int a, int b)
        {
            this._a = a;
            this._b = b;
        }

        public int A => this._a;

        public int B => this._b;

        public int this[bool start] => start ? this._a : this._b;

        internal void SetStartOrEnd(bool start, int value)
        {
            if (start)
                this._a = value;
            else
                this._b = value;
        }

        public int OtherVertex(int eitherAOrB)
        {
            if (eitherAOrB == this._a)
                return this._b;
            if (eitherAOrB == this._b)
                return this._a;
            throw new InvalidOperationException(string.Format("oneOfAOrB - currently {0} but only {1} and {2} present", (object)eitherAOrB.ToString(), (object)this._a.ToString(), (object)this._b.ToString()));
        }

        public override string ToString() => string.Format("{0}, {1}", (object)this._a.ToString(), (object)this._b.ToString());
    }
}
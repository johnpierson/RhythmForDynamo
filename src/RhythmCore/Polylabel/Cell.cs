namespace Rhythm.Polylabel
{
    //made possible thanks to https://github.com/eqmiller/polylabel-csharp
    internal class Cell
    {
        private Cell(){}

        public Cell(double x, double y, double h, double[][][] polygon)
        {
            X = x;
            Y = y;
            H = h;
            D = PointToPolygonDist(x, y, polygon);
            Max = this.D + this.H * global::System.Math.Sqrt(2);
        }

        /// <summary>
        /// Cell center X
        /// </summary>
        public double X { get; }

        /// <summary>
        /// Cell center Y
        /// </summary>
        public double Y { get; }

        /// <summary>
        /// Half the cell size
        /// </summary>
        public double H { get; }

        /// <summary>
        /// Distance from cell center to polygon
        /// </summary>
        public double D { get; }

        /// <summary>
        /// Max distance to polygon within a cell
        /// </summary>
        public double Max { get; }

        /// <summary>
        /// Signed distance from point to polygon outline (negative if point is outside)
        /// </summary>
        /// <param name="x">Cell center x</param>
        /// <param name="y">Cell center y</param>
        /// <param name="polygon">Full GeoJson like Polygon</param>
        private double PointToPolygonDist(double x, double y, double[][][] polygon)
        {
            var inside = false;
            var minDistSq = double.PositiveInfinity;

            for (var k = 0; k < polygon.Length; k++)
            {
                var ring = polygon[k];

                var len = ring.Length;
                var j = len - 1;
                for (var i = 0; i < len; j = i++)
                {
                    var a = ring[i];
                    var b = ring[j];

                    if ((a[1] > y != b[1] > y) &&
                        (x < (b[0] - a[0]) * (y - a[1]) / (b[1] - a[1]) + a[0])) inside = !inside;

                    minDistSq = global::System.Math.Min(minDistSq, GetSegDistSq(x, y, a, b));
                }
            }

            return (inside ? 1 : -1) * global::System.Math.Sqrt(minDistSq);
        }

        /// <summary>
        /// Get squared distance from a point to a segment
        /// </summary>
        private double GetSegDistSq(double px, double py, double[] a, double[] b)
        {
            var x = a[0];
            var y = a[1];
            var dx = b[0] - x;
            var dy = b[1] - y;

            if (dx != 0 || dy != 0)
            {
                var t = ((px - x) * dx + (py - y) * dy) / (dx * dx + dy * dy);

                if (t > 1)
                {
                    x = b[0];
                    y = b[1];
                }
                else if (t > 0)
                {
                    x += dx * t;
                    y += dy * t;
                }
            }

            dx = px - x;
            dy = py - y;

            return dx * dx + dy * dy;
        }
    }
}
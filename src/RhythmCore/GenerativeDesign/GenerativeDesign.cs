using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Nuclex.Game.Packing;

namespace Rhythm.GenerativeDesign
{
    /// <summary>
    /// Wrapper class for generative design
    /// </summary>
    public class GenerativeDesign
    {
        private GenerativeDesign()
        {
        }

        /// <summary>
        /// Packs viewport rectangles within a container rectangle, ensuring no overlap with container edges
        /// </summary>
        /// <param name="container">The container rectangle to pack viewports within</param>
        /// <param name="viewportRectangles">List of viewport rectangles to pack</param>
        /// <param name="viewportIds">List of viewport IDs corresponding to the rectangles</param>
        /// <param name="marginFromEdge">Minimum margin distance from container edges</param>
        /// <returns name="viewportsThatFit">The viewports that fit in the titleblock container</returns>
        /// <returns name="proposedLocations">The proposed locations</returns>
        /// <returns name="viewportRectangles">The rectangles</returns>
        [MultiReturn(new[] { "viewportsThatFit", "proposedLocations", "viewportRectangles" })]
        public static Dictionary<string, object> PackViewports(Rectangle container, List<Rectangle> viewportRectangles, List<int> viewportIds, double marginFromEdge = 0)
        {
            List<int> viewportsThatFit = new List<int>();
            List<Point> points = new List<Point>();
            List<Rectangle> rects = new List<Rectangle>();

            var containerBbox = container.BoundingBox;
            var containerWidth = containerBbox.MaxPoint.X - containerBbox.MinPoint.X;
            var containerHeight = containerBbox.MaxPoint.Y - containerBbox.MinPoint.Y; // Fixed: was using X instead of Y
            
            // Apply margin to reduce effective packing area
            var effectiveWidth = containerWidth - (2 * marginFromEdge);
            var effectiveHeight = containerHeight - (2 * marginFromEdge);

            // Ensure effective dimensions are positive
            if (effectiveWidth <= 0 || effectiveHeight <= 0)
            {
                // Return empty results if margin is too large for container
                var emptyInfo = new Dictionary<string, object>
                {
                    {"viewportsThatFit", viewportsThatFit},
                    { "proposedLocations", points},
                    { "viewportRectangles", rects}
                };
                return emptyInfo;
            }

            CygonRectanglePacker packer = new CygonRectanglePacker(effectiveWidth, effectiveHeight);

            UV placement = null;
            for (int i = 0; i < viewportRectangles.Count; i++)
            {
                var current = viewportRectangles[i];


                List<Point> geoPoints = new List<Point>();
                
                var bBox = BoundingBox.ByGeometry(current);

                var height = bBox.MaxPoint.Y - bBox.MinPoint.Y;
                var width = bBox.MaxPoint.X - bBox.MinPoint.X;

                if (packer.TryPack(width, height, out placement))
                {
                    var bottomLeft = containerBbox.MinPoint;
                    double bottomLeftX;
                    double bottomLeftY;

                    if (bottomLeft != null)
                    {
                        bottomLeftX = bottomLeft.X;
                        bottomLeftY = bottomLeft.Y;
                    }
                    else
                    {
                        bottomLeftX = 0;
                        bottomLeftY = 0;
                    }
                    
                    // Account for margin offset in the placement calculation
                    var adjustedX = placement.U + bottomLeftX + marginFromEdge + width / 2;
                    var adjustedY = placement.V + bottomLeftY + marginFromEdge + height / 2;
                    
                    viewportsThatFit.Add(viewportIds[i]);
                    rects.Add(current);

                    points.Add(Point.ByCoordinates(adjustedX, adjustedY, 0));
                }
            }
            //returns the outputs
            var outInfo = new Dictionary<string, object>
            {
                {"viewportsThatFit", viewportsThatFit},
                { "proposedLocations",points},
                { "viewportRectangles",rects}
            };
            return outInfo;
        }
    }
    /// <summary>
    /// Wrapper for random distribution. This section made possible with https://github.com/MarcoFazioRandom/Gaussian-Random under the MIT license
    /// </summary>
    public class RandomDistribution
    {
        private static bool hasSpare = false;
        private static double spare;
        private static Random ran = new Random();
        private RandomDistribution(){}

        //public RandomDistribution(int seed) : base(seed) { }

        private static double NextGaussianStandard()
        {
            if (hasSpare)
            {
                hasSpare = false;
                return spare;
            }
            else
            {
                double u, v, s;
                do
                {
                    u = ran.NextDouble() * 2 - 1;
                    v = ran.NextDouble() * 2 - 1;
                    s = u * u + v * v;
                } while (s > 1 || s == 0);
                s = global::System.Math.Sqrt(-2 * global::System.Math.Log(s) / s);
                spare = v * s;
                hasSpare = true;
                return u * s;
            }
        }

        /// <summary>
        /// Return a number in the range (-1, +1) with a Normal distributed probability using the Marsaglia polar method.
        /// </summary>
        /// <param name="standardDeviation"> 
        /// It is a measure of the amount of variation or dispersion of the values.
        /// </param>
        /// <returns></returns>
        public static double NextGaussian(double standardDeviation = 1)
        {
            double x;
            do
            {
                x = NextGaussianStandard() * standardDeviation / 3;
            } while (x < -1 || x > 1);
            return x;
        }
    }
}


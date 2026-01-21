using Autodesk.DesignScript.Geometry;
using System;
using System.Collections.Generic;

namespace Rhythm.Geometry
{
    /// <summary>
    /// Wrapper class for PolyCurve operations
    /// </summary>
    public class PolyCurve
    {
        private PolyCurve()
        {
        }

        /// <summary>
        /// Creates a PolyCurve from a random distribution of points.
        /// </summary>
        /// <param name="numberOfPoints">The number of random points to generate</param>
        /// <param name="minX">Minimum X coordinate (default: 0)</param>
        /// <param name="maxX">Maximum X coordinate (default: 100)</param>
        /// <param name="minY">Minimum Y coordinate (default: 0)</param>
        /// <param name="maxY">Maximum Y coordinate (default: 100)</param>
        /// <param name="minZ">Minimum Z coordinate (default: 0)</param>
        /// <param name="maxZ">Maximum Z coordinate (default: 0)</param>
        /// <param name="connectToStart">Whether to connect the last point back to the first point (default: false)</param>
        /// <param name="seed">Random seed for reproducible results (default: uses time-based seed)</param>
        /// <returns>A PolyCurve created from randomly distributed points</returns>
        public static Autodesk.DesignScript.Geometry.PolyCurve ByRandomPoints(
            int numberOfPoints,
            double minX = 0,
            double maxX = 100,
            double minY = 0,
            double maxY = 100,
            double minZ = 0,
            double maxZ = 0,
            bool connectToStart = false,
            int seed = -1)
        {
            // Validate input
            if (numberOfPoints < 2)
            {
                throw new ArgumentException("Number of points must be at least 2 to create a polycurve.", nameof(numberOfPoints));
            }

            // Create random number generator with seed if provided
            Random random = seed == -1 ? new Random() : new Random(seed);

            // Generate random points
            List<Autodesk.DesignScript.Geometry.Point> points = new List<Autodesk.DesignScript.Geometry.Point>();
            for (int i = 0; i < numberOfPoints; i++)
            {
                double x = minX + random.NextDouble() * (maxX - minX);
                double y = minY + random.NextDouble() * (maxY - minY);
                double z = minZ + random.NextDouble() * (maxZ - minZ);
                
                points.Add(Autodesk.DesignScript.Geometry.Point.ByCoordinates(x, y, z));
            }

            // Create and return the PolyCurve
            return Autodesk.DesignScript.Geometry.PolyCurve.ByPoints(points, connectToStart);
        }
    }
}

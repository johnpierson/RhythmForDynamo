using System;
using Autodesk.DesignScript.Geometry;
using Dynamo.Graph.Nodes;
using SystemMath = System.Math;

namespace Rhythm.Geometry
{
    /// <summary>
    /// Wrapper class for vectors.
    /// </summary>
    public class Vector
    {
        private Vector()
        { }

        // Cardinal direction vectors as static readonly to avoid repeated allocation
        private static readonly Autodesk.DesignScript.Geometry.Vector North = Autodesk.DesignScript.Geometry.Vector.ByCoordinates(0, 1, 0);
        private static readonly Autodesk.DesignScript.Geometry.Vector South = Autodesk.DesignScript.Geometry.Vector.ByCoordinates(0, -1, 0);
        private static readonly Autodesk.DesignScript.Geometry.Vector East = Autodesk.DesignScript.Geometry.Vector.ByCoordinates(1, 0, 0);
        private static readonly Autodesk.DesignScript.Geometry.Vector West = Autodesk.DesignScript.Geometry.Vector.ByCoordinates(-1, 0, 0);

        /// <summary>
        /// This will determine the vector's cardinal direction (N, S, E, W, NE, NW, SE, SW).
        /// Based on the logic from Walls.Direction.
        /// </summary>
        /// <param name="vector">The vector to get direction from.</param>
        /// <returns name="direction">The cardinal direction.</returns>
        /// <search>
        /// vector, direction, cardinal, NSEW
        /// </search>
        [NodeCategory("Query")]
        public static string Direction(Autodesk.DesignScript.Geometry.Vector vector)
        {
            // Validate input vector
            double magnitude = SystemMath.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
            if (magnitude < 1e-10)
            {
                return string.Empty; // Return empty string for zero or near-zero vectors
            }
            
            // Calculate angles to cardinal directions using dot product
            // Angle between two vectors: angle = acos(dot product / (length1 * length2))
            var angleToNorth = AngleBetweenVectors(vector, North, magnitude);
            var angleToSouth = AngleBetweenVectors(vector, South, magnitude);
            var angleToEast = AngleBetweenVectors(vector, East, magnitude);
            var angleToWest = AngleBetweenVectors(vector, West, magnitude);
            
            string vectorDirection = string.Empty;
            
            // North checks
            if (SystemMath.Abs(angleToNorth) < SystemMath.PI / 4)
            {
                // North
                vectorDirection = "N";
                
                // North east
                if (SystemMath.Abs(angleToEast) < SystemMath.PI / 3)
                {
                    vectorDirection = "NE";
                }
                // North west
                if (SystemMath.Abs(angleToWest) < SystemMath.PI / 3)
                {
                    vectorDirection = "NW";
                }
            }
            
            // South checks
            if (SystemMath.Abs(angleToSouth) < SystemMath.PI / 4)
            {
                // South
                vectorDirection = "S";
                
                // South east
                if (SystemMath.Abs(angleToEast) < SystemMath.PI / 3)
                {
                    vectorDirection = "SE";
                }
                // South west
                if (SystemMath.Abs(angleToWest) < SystemMath.PI / 3)
                {
                    vectorDirection = "SW";
                }
            }
            
            // East checks
            if (SystemMath.Abs(angleToEast) < SystemMath.PI / 4)
            {
                // East
                vectorDirection = "E";
                
                // North east
                if (SystemMath.Abs(angleToNorth) < SystemMath.PI / 3)
                {
                    vectorDirection = "NE";
                }
                // South east
                if (SystemMath.Abs(angleToSouth) < SystemMath.PI / 3)
                {
                    vectorDirection = "SE";
                }
            }
            
            // West checks
            if (SystemMath.Abs(angleToWest) < SystemMath.PI / 4)
            {
                // West
                vectorDirection = "W";
                
                // North west
                if (SystemMath.Abs(angleToNorth) < SystemMath.PI / 3)
                {
                    vectorDirection = "NW";
                }
                // South west
                if (SystemMath.Abs(angleToSouth) < SystemMath.PI / 3)
                {
                    vectorDirection = "SW";
                }
            }
            
            return vectorDirection;
        }
        
        /// <summary>
        /// Helper method to calculate the angle between two vectors.
        /// </summary>
        /// <param name="v1">First vector</param>
        /// <param name="v2">Second vector (cardinal direction with magnitude 1)</param>
        /// <param name="magnitude1">Pre-calculated magnitude of v1</param>
        private static double AngleBetweenVectors(Autodesk.DesignScript.Geometry.Vector v1, Autodesk.DesignScript.Geometry.Vector v2, double magnitude1)
        {
            // Calculate dot product
            double dotProduct = v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
            
            // Cardinal direction vectors have magnitude 1, so we only need magnitude1
            // Calculate angle using dot product formula
            // Clamp the value to handle floating point errors
            double cosAngle = dotProduct / magnitude1;
            cosAngle = SystemMath.Max(-1.0, SystemMath.Min(1.0, cosAngle));
            
            return SystemMath.Acos(cosAngle);
        }
    }
}

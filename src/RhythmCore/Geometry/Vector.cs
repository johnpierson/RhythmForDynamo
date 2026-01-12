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
            // Define cardinal direction vectors
            // North is +Y, East is +X, South is -Y, West is -X
            var north = Autodesk.DesignScript.Geometry.Vector.ByCoordinates(0, 1, 0);
            var south = Autodesk.DesignScript.Geometry.Vector.ByCoordinates(0, -1, 0);
            var east = Autodesk.DesignScript.Geometry.Vector.ByCoordinates(1, 0, 0);
            var west = Autodesk.DesignScript.Geometry.Vector.ByCoordinates(-1, 0, 0);
            
            // Calculate angles to cardinal directions using dot product
            // Angle between two vectors: angle = acos(dot product / (length1 * length2))
            var angleToNorth = AngleBetweenVectors(vector, north);
            var angleToSouth = AngleBetweenVectors(vector, south);
            var angleToEast = AngleBetweenVectors(vector, east);
            var angleToWest = AngleBetweenVectors(vector, west);
            
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
        private static double AngleBetweenVectors(Autodesk.DesignScript.Geometry.Vector v1, Autodesk.DesignScript.Geometry.Vector v2)
        {
            // Calculate dot product
            double dotProduct = v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
            
            // Calculate magnitudes
            double magnitude1 = SystemMath.Sqrt(v1.X * v1.X + v1.Y * v1.Y + v1.Z * v1.Z);
            double magnitude2 = SystemMath.Sqrt(v2.X * v2.X + v2.Y * v2.Y + v2.Z * v2.Z);
            
            // Calculate angle using dot product formula
            // Clamp the value to handle floating point errors
            double cosAngle = dotProduct / (magnitude1 * magnitude2);
            cosAngle = SystemMath.Max(-1.0, SystemMath.Min(1.0, cosAngle));
            
            return SystemMath.Acos(cosAngle);
        }
    }
}

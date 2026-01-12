using System;
using Autodesk.DesignScript.Geometry;
using Dynamo.Graph.Nodes;

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
            
            // Calculate angles to cardinal directions using AngleTo method
            var angleToNorth = vector.AngleTo(north);
            var angleToSouth = vector.AngleTo(south);
            var angleToEast = vector.AngleTo(east);
            var angleToWest = vector.AngleTo(west);
            
            string vectorDirection = string.Empty;
            
            // North checks
            if (Math.Abs(angleToNorth) < Math.PI / 4)
            {
                // North
                vectorDirection = "N";
                
                // North east
                if (Math.Abs(angleToEast) < Math.PI / 3)
                {
                    vectorDirection = "NE";
                }
                // North west
                if (Math.Abs(angleToWest) < Math.PI / 3)
                {
                    vectorDirection = "NW";
                }
            }
            
            // South checks
            if (Math.Abs(angleToSouth) < Math.PI / 4)
            {
                // South
                vectorDirection = "S";
                
                // South east
                if (Math.Abs(angleToEast) < Math.PI / 3)
                {
                    vectorDirection = "SE";
                }
                // South west
                if (Math.Abs(angleToWest) < Math.PI / 3)
                {
                    vectorDirection = "SW";
                }
            }
            
            // East checks
            if (Math.Abs(angleToEast) < Math.PI / 4)
            {
                // East
                vectorDirection = "E";
                
                // North east
                if (Math.Abs(angleToNorth) < Math.PI / 3)
                {
                    vectorDirection = "NE";
                }
                // South east
                if (Math.Abs(angleToSouth) < Math.PI / 3)
                {
                    vectorDirection = "SE";
                }
            }
            
            // West checks
            if (Math.Abs(angleToWest) < Math.PI / 4)
            {
                // West
                vectorDirection = "W";
                
                // North west
                if (Math.Abs(angleToNorth) < Math.PI / 3)
                {
                    vectorDirection = "NW";
                }
                // South west
                if (Math.Abs(angleToSouth) < Math.PI / 3)
                {
                    vectorDirection = "SW";
                }
            }
            
            return vectorDirection;
        }
    }
}

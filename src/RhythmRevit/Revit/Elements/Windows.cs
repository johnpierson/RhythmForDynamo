using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using Revit.GeometryConversion;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for windows.
    /// </summary>
    public class Windows
    {
        private Windows()
        { }

        /// <summary>
        /// This will get the window's facing direction based on the FacingOrientation property.
        /// Windows in Revit are typically FamilyInstance elements with a FacingOrientation that indicates the direction they face.
        /// </summary>
        /// <param name="window">The window to calculate facing from.</param>
        /// <returns name="facingDirection">The estimated facing direction.</returns>
        /// <returns name="facingVector">The facing vector.</returns>
        [MultiReturn(new[] { "facingDirection", "facingVector" })]
        [NodeCategory("Query")]
        public static Dictionary<string, object> Direction(global::Revit.Elements.Element window)
        {
            Autodesk.Revit.DB.FamilyInstance internalWindow = window.InternalElement as Autodesk.Revit.DB.FamilyInstance;

            if (internalWindow == null)
            {
                throw new ArgumentException("The provided element is not a valid window (FamilyInstance).");
            }

            XYZ windowDirection = GetWindowDirection(internalWindow);

            //returns the outputs
            var outInfo = new Dictionary<string, object>
            {
                {"facingDirection", GetFacingDirection(windowDirection)},
                {"facingVector", windowDirection.ToVector()}
            };
            return outInfo;
        }

        /// <summary>
        /// Get the facing direction of a window using its FacingOrientation property.
        /// </summary>
        private static XYZ GetWindowDirection(Autodesk.Revit.DB.FamilyInstance window)
        {
            // Windows have a FacingOrientation property that gives the direction the window faces
            XYZ facingOrientation = window.FacingOrientation;

            if (facingOrientation != null)
            {
                return facingOrientation.Normalize();
            }

            // Fallback: if FacingOrientation is not available, try to compute from location and orientation
            // This is similar to how walls work but adapted for FamilyInstances
            LocationPoint locPoint = window.Location as LocationPoint;
            if (locPoint != null)
            {
                // For face-based or wall-based families, get the facing orientation from the transform
                Transform transform = window.GetTransform();
                if (transform != null)
                {
                    // The Y-axis of the transform typically represents the facing direction for windows
                    return transform.BasisY.Normalize();
                }
            }

            // If all else fails, return a default direction (facing north)
            return XYZ.BasisY;
        }

        /// <summary>
        /// Convert an XYZ direction vector to a cardinal direction string.
        /// This uses the same logic as Walls.Direction for consistency.
        /// </summary>
        private static string GetFacingDirection(XYZ windowDirection)
        {
            var angleToNorth = windowDirection.AngleTo(XYZ.BasisY);
            var angleToSouth = windowDirection.AngleTo(-XYZ.BasisY);
            var angleToEast = windowDirection.AngleTo(XYZ.BasisX);
            var angleToWest = windowDirection.AngleTo(-XYZ.BasisX);

            string windowFacing = string.Empty;

            //north checks
            if (Math.Abs(angleToNorth) < Math.PI / 4)
            {
                //north
                windowFacing = "N";

                //north east
                if (Math.Abs(angleToEast) < Math.PI / 3)
                {
                    windowFacing = "NE";
                }
                //north west
                if (Math.Abs(angleToWest) < Math.PI / 3)
                {
                    windowFacing = "NW";
                }
            }

            //south checks
            if (Math.Abs(angleToSouth) < Math.PI / 4)
            {
                //south
                windowFacing = "S";

                //south east
                if (Math.Abs(angleToEast) < Math.PI / 3)
                {
                    windowFacing = "SE";
                }
                //south west
                if (Math.Abs(angleToWest) < Math.PI / 3)
                {
                    windowFacing = "SW";
                }
            }

            //east checks
            if (Math.Abs(angleToEast) < Math.PI / 4)
            {
                //east
                windowFacing = "E";

                //north east
                if (Math.Abs(angleToNorth) < Math.PI / 3)
                {
                    windowFacing = "NE";
                }
                //south east
                if (Math.Abs(angleToSouth) < Math.PI / 3)
                {
                    windowFacing = "SE";
                }
            }

            //west checks
            if (Math.Abs(angleToWest) < Math.PI / 4)
            {
                //west
                windowFacing = "W";

                //north west
                if (Math.Abs(angleToNorth) < Math.PI / 3)
                {
                    windowFacing = "NW";
                }
                //south west
                if (Math.Abs(angleToSouth) < Math.PI / 3)
                {
                    windowFacing = "SW";
                }
            }

            return windowFacing;
        }
    }
}

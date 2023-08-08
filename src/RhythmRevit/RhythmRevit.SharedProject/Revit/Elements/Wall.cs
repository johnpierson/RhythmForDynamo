using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using Revit.Elements;
using Revit.GeometryConversion;
using Element = Autodesk.Revit.DB.Element;
using Line = Autodesk.Revit.DB.Line;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for walls.
    /// </summary>
    public class Walls
    {
        private Walls()
        { }

        /// <summary>
        /// This node will try to check if the walls profile has been modified using the dependent elements method available in Revit 2018.1+
        /// </summary>
        /// <param name="wall">The walls to check.</param>
        /// <returns name="bool">The result.</returns>
        /// <search>
        /// profile, wall
        /// </search>
        [NodeCategory("Query")]
        public static bool HasEditedProfile(global::Revit.Elements.Element wall)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.Wall internalWall = (Autodesk.Revit.DB.Wall )wall.InternalElement;

            //dependent elements method (available in Revit 2018.1 +)
            Autodesk.Revit.DB.ElementFilter elemFilter = new ElementIsElementTypeFilter(true);
            IList<ElementId> elemIds = internalWall.GetDependentElements(elemFilter);

            //get the elements
            List<Autodesk.Revit.DB.Element> elems = new List<Element>(elemIds.Select(e => doc.GetElement(e)));

            //find out if any of the elements are of sketch type
            return elems.Any(e => e is Autodesk.Revit.DB.Sketch || e is Autodesk.Revit.DB.SketchPlane);
        }
        /// <summary>
        /// This node will try to check if the walls profile has been modified using the dependent elements method available in Revit 2018.1+
        /// </summary>
        /// <param name="wall">The walls to check.</param>
        /// <returns name="modelCurves">The result.</returns>
        /// <search>
        /// profile, wall
        /// </search>
        [NodeCategory("Query")]
        public static global::Revit.Elements.Element[] EditedProfile(global::Revit.Elements.Element wall)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.Element internalWall = wall.InternalElement;

            //dependent elements method (available in Revit 2018.1 +)
            Autodesk.Revit.DB.ElementFilter elemFilter = new ElementIsElementTypeFilter(true);
            IList<ElementId> elemIds = internalWall.GetDependentElements(elemFilter);

            //get the elements
            List<Autodesk.Revit.DB.Element> elems = new List<Element>(elemIds.Select(e => doc.GetElement(e)));

            global::Autodesk.Revit.DB.Element[] internalCurves = elems.Where(e => e is Autodesk.Revit.DB.ModelCurve).ToArray();

            global::Revit.Elements.Element[] modelCurves = internalCurves.Select(e => e.ToDSType(false)).ToArray();

            return modelCurves;
        }

        /// <summary>
        /// This will estimate the wall's facing direction. Credit for the logic in this node goes to CASE.
        /// Without the open source tools provided by Don and the CASE team, this node would probably not exist.
        /// https://github.com/rudderdon/case-apps/blob/master/2017/Case.Directionality/Case.Directionality/Data/clsExternalWalls.vb
        /// </summary>
        /// <param name="wall">The wall to calculate facing from.</param>
        /// <returns name="facingDirection">The estimated facing direction.</returns>
        /// <returns name="facingVector">The facing vector.</returns>
        [MultiReturn(new[] { "facingDirection", "facingVector" })]
        [NodeCategory("Query")]
        public static Dictionary<string, object> Direction(global::Revit.Elements.Element wall)
        {
            Autodesk.Revit.DB.Wall internalWall = wall.InternalElement as Autodesk.Revit.DB.Wall;

            XYZ wallDirection = GetWallDirection(internalWall);

            //returns the outputs
            var outInfo = new Dictionary<string, object>
            {
                {"facingDirection", GetFacingDirection(wallDirection)},
                {"facingVector", wallDirection.ToVector()}
            };
            return outInfo;
        }


        /// https://github.com/rudderdon/case-apps/blob/master/2017/Case.Directionality/Case.Directionality/Data/clsExternalWalls.vb
        private static XYZ GetWallDirection(Autodesk.Revit.DB.Wall wall)
        {
            LocationCurve locCurve = wall.Location as LocationCurve;
            XYZ extDirection;

            var curve = locCurve.Curve;
            XYZ dir;
            if (curve != null && curve is Line)
            {
                dir = curve.ComputeDerivatives(0, true).BasisX.Normalize();
            }
            else
            {
                dir = (curve.GetEndPoint(1) - curve.GetEndPoint(0)).Normalize();
            }

            extDirection = XYZ.BasisZ.CrossProduct(dir);

            if (wall.Flipped)
            {
                extDirection = -extDirection;
            }

            return extDirection;
        }
        /// https://github.com/rudderdon/case-apps/blob/master/2017/Case.Directionality/Case.Directionality/Data/clsExternalWalls.vb

        private static string GetFacingDirection(XYZ wallDirection)
        {
            var angleToNorth = wallDirection.AngleTo(XYZ.BasisY);
            var angleToSouth = wallDirection.AngleTo(-XYZ.BasisY);
            var angleToEast = wallDirection.AngleTo(XYZ.BasisX);
            var angleToWest = wallDirection.AngleTo(-XYZ.BasisX);

            //is to the left?
            string wallFacing = string.Empty;

            //north checks
            if (Math.Abs(angleToNorth) < Math.PI / 4)
            {
                //north
                wallFacing = "N";

                //north east
                if (Math.Abs(angleToEast) < Math.PI / 3)
                {
                    wallFacing = "NE";
                }
                //north west
                if (Math.Abs(angleToWest) < Math.PI / 3)
                {
                    wallFacing = "NW";
                }
            }

            //south checks
            if (Math.Abs(angleToSouth) < Math.PI / 4)
            {
                //south
                wallFacing = "S";

                //south east
                if (Math.Abs(angleToEast) < Math.PI / 3)
                {
                    wallFacing = "SE";
                }
                //south west
                if (Math.Abs(angleToWest) < Math.PI / 3)
                {
                    wallFacing = "SW";
                }
            }

            //east checks
            if (Math.Abs(angleToEast) < Math.PI / 4)
            {
                //east
                wallFacing = "E";

                //north east
                if (Math.Abs(angleToNorth) < Math.PI / 3)
                {
                    wallFacing = "NE";
                }
                //south east
                if (Math.Abs(angleToSouth) < Math.PI / 3)
                {
                    wallFacing = "SE";
                }
            }

            //west checks
            if (Math.Abs(angleToWest) < Math.PI / 4)
            {
                //west
                wallFacing = "W";

                //north west
                if (Math.Abs(angleToNorth) < Math.PI / 3)
                {
                    wallFacing = "NW";
                }
                //south west
                if (Math.Abs(angleToSouth) < Math.PI / 3)
                {
                    wallFacing = "SW";
                }
            }

            return wallFacing;
        }
        private double WallAngle { get; set; }
        private enum AngleDir
        {
            IsNorth,
            IsSouth,
            IsEast,
            IsWest
        }

        private static double GetAngleValue(AngleDir direction, double dir, bool isToLeft)
        {
            double wallAngle = 0;

            var degrees = dir * Math.PI;

            switch (direction)
            {
                case AngleDir.IsNorth:
                    if (isToLeft)
                    {
                        wallAngle = 360 - degrees;
                    }
                    else
                    {
                        wallAngle = degrees;
                    }
                    break;
                case AngleDir.IsSouth:
                    if (isToLeft)
                    {
                        wallAngle = 180 - degrees;
                    }
                    else
                    {
                        wallAngle = 180 + degrees;
                    }
                    break;
                case AngleDir.IsEast:
                    if (isToLeft)
                    {
                        wallAngle = 90 - degrees;
                    }
                    else
                    {
                        wallAngle = 90 + degrees;
                    }
                    break;
                case AngleDir.IsWest:
                    if (isToLeft)
                    {
                        wallAngle = 270 - degrees;
                    }
                    else
                    {
                        wallAngle = 270 + degrees;
                    }
                    break;
            }

            return wallAngle;
        }
    }
}

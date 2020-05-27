using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using Revit.GeometryConversion;
using RevitServices.Transactions;
using Curve = Autodesk.DesignScript.Geometry.Curve;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for curtain gridlines.
    /// </summary>
    public class CurtainGridLine
    {
        private CurtainGridLine()
        {
        }

        /// <summary>
        /// This node will retrieve the geometric curve from the curtain wall.
        /// </summary>
        /// <param name="curtainGridLine">The curtain gridline to get data from.</param>
        /// <returns name="fullCurve">The full geometric curve</returns>
        /// <search>
        /// CurtainGridLine.FullCurve, rhythm
        /// </search>
        [NodeCategory("Query")]
        public static object FullCurve(global::Revit.Elements.Element curtainGridLine)
        {
            Autodesk.Revit.DB.CurtainGridLine internalCurtainGridline = (Autodesk.Revit.DB.CurtainGridLine)curtainGridLine.InternalElement;
            var fullCurve = internalCurtainGridline.FullCurve.ToProtoType();

            return fullCurve;
        }

        /// <summary>
        /// This node will retrieve the geometric curve segments from the curtain wall.
        /// </summary>
        /// <param name="curtainGridLine">The curtain gridline to get data from.</param>
        /// <returns name="AllSegmentCurves">The segments that make up the curtain grid.</returns>
        /// <search>
        /// CurtainGridLine.FullCurve, rhythm
        /// </search>
        [NodeCategory("Query")]
        public static object AllSegmentCurves(global::Revit.Elements.Element curtainGridLine)
        {
            Autodesk.Revit.DB.CurtainGridLine internalCurtainGridline = (Autodesk.Revit.DB.CurtainGridLine)curtainGridLine.InternalElement;
            var AllSegmentCurves = internalCurtainGridline.AllSegmentCurves.ToProtoType();

            return AllSegmentCurves;
        }

        /// <summary>
        /// This node will remove the given curve segments from the curtain grid line.
        /// </summary>
        /// <param name="curtainGridLine">The curtain gridline to remove segments from.</param>
        /// <param name="curves">The curves that represent the grid segment to remove.</param>
        /// <returns name="curtainGridLine">The curtain grid that was supplied.</returns>
        /// <search>
        /// CurtainGridLine.RemoveSegment, rhythm
        /// </search>
        [NodeCategory("Actions")]
        public static global::Revit.Elements.Element RemoveSegment(global::Revit.Elements.Element curtainGridLine, List<Curve> curves)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.CurtainGridLine internalCurtainGridline = (Autodesk.Revit.DB.CurtainGridLine)curtainGridLine.InternalElement;
            foreach (var curve in curves)
            {
                TransactionManager.Instance.EnsureInTransaction(doc);
                internalCurtainGridline.RemoveSegment(curve.ToRevitType());
                TransactionManager.Instance.TransactionTaskDone();
            }
            return curtainGridLine;
        }

        /// <summary>
        /// This node will retrieve the geometric existing curve segments from the curtain wall.
        /// </summary>
        /// <param name="curtainGridLine">The curtain gridline to get data from.</param>
        /// <returns name="existingSegmentCurves">The segments that make up the curtain grid.</returns>
        /// <search>
        /// CurtainGridLine.ExistingSegmentCurves, rhythm
        /// </search>
        [NodeCategory("Query")]
        public static object ExistingSegmentCurves(global::Revit.Elements.Element curtainGridLine)
        {
            Autodesk.Revit.DB.CurtainGridLine internalCurtainGridline = (Autodesk.Revit.DB.CurtainGridLine)curtainGridLine.InternalElement;
            var existingSegmentCurves = internalCurtainGridline.ExistingSegmentCurves.ToProtoType();

            return existingSegmentCurves;
        }
        /// <summary>
        /// This node will retrieve the geometric skipped curve segments from the curtain wall.
        /// </summary>
        /// <param name="curtainGridLine">The curtain gridline to get data from.</param>
        /// <returns name="skippedSegmentCurves">The segments that make up the curtain grid.</returns>
        /// <search>
        /// CurtainGridLine.SkippedSegmentCurves, rhythm
        /// </search>
        [NodeCategory("Query")]
        public static object SkippedSegmentCurves(global::Revit.Elements.Element curtainGridLine)
        {
            Autodesk.Revit.DB.CurtainGridLine internalCurtainGridline = (Autodesk.Revit.DB.CurtainGridLine)curtainGridLine.InternalElement;
            var skippedSegmentCurves = internalCurtainGridline.SkippedSegmentCurves.ToProtoType();

            return skippedSegmentCurves;
        }

        /// <summary>
        /// This node will attempt to set the location of the given grid line to the given point.
        /// NOTE: This will "translate" the grid line parallel to where it is initially. We cannot move a U grid to a V grid and so forth.
        /// </summary>
        /// <param name="curtainGridLine">The curtain grid line to try to set.</param>
        /// <param name="newLocation"></param>
        /// <returns name="curtainGridLine">The translated curtain grid line. (Returns null if failed)</returns>
        [NodeCategory("Actions")]
        public static object SetLocation(global::Revit.Elements.Element curtainGridLine, Point newLocation)
        {
            //the current document
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //cast the grid line to the internal Autodesk.Revit.DB representation
            Autodesk.Revit.DB.CurtainGridLine internalCurtainGridline = (Autodesk.Revit.DB.CurtainGridLine)curtainGridLine.InternalElement;
            //obtains the grid line's full curve and closest point on it's curve to calculate the vector with
            var fullCurve = internalCurtainGridline.FullCurve.ToProtoType();
            var firstPoint = fullCurve.ClosestPointTo(newLocation);
            var vec = Vector.ByTwoPoints(firstPoint, newLocation);
            try
            {
                //start our transaction
                TransactionManager.Instance.EnsureInTransaction(doc);
                internalCurtainGridline.Location.Move(vec.ToRevitType());
                TransactionManager.Instance.TransactionTaskDone();
                return curtainGridLine;
            }
            catch (Exception)
            {
                return null;
            } 
        }
    }
}

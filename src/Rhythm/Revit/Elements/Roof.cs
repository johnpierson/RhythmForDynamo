using Autodesk.Revit.DB;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using System;
using Dynamo.Graph.Nodes;
using Curve = Autodesk.DesignScript.Geometry.Curve;
using Point = Autodesk.DesignScript.Geometry.Point;


namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for roofs.
    /// </summary>
    public class Roofs
    {
        private Roofs()
        {
        }
        /// <summary>
        /// This node will add a split line to the given roof with supplied line and elevation.
        /// </summary>
        /// <param name="roof">The roof to split.</param>
        /// <param name="curve">The geometry curve to use.</param>
        /// <param name="elevation">The elevation to go to.</param>
        /// <returns name="result">The result..</returns>
        /// <search>
        /// Element.CreateParts, rhythm
        /// </search>
        [NodeCategory("Actions")]
        public static string AddSplitLineWithElevation(global::Revit.Elements.Element roof, Curve curve, double elevation)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.RoofBase internalRoof = (Autodesk.Revit.DB.RoofBase)roof.InternalElement;
        
            Point startPoint = Point.ByCoordinates(curve.StartPoint.X, curve.StartPoint.Y,elevation);
            Point endPoint = Point.ByCoordinates(curve.EndPoint.X, curve.EndPoint.Y, elevation);

            string result;
            try
            {
                TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);
                internalRoof.SlabShapeEditor.Enable();
                SlabShapeVertex vertex1 = internalRoof.SlabShapeEditor.DrawPoint(startPoint.ToXyz());
                SlabShapeVertex vertex2 = internalRoof.SlabShapeEditor.DrawPoint(endPoint.ToXyz());
                internalRoof.SlabShapeEditor.DrawSplitLine(vertex1, vertex2);
                TransactionManager.Instance.TransactionTaskDone();
                result = "Success.";
            }
            catch (Exception)
            {
                result = "not so success.";
            }

            return result;
        }

        /// <summary>
        /// This node will add a point to the given roof.
        /// </summary>
        /// <param name="roof">The roof to add shape editing points to.</param>
        /// <param name="point">The points to add.</param>
        /// <returns name="result">The result..</returns>
        /// <search>
        /// Roof.AddPoint, rhythm
        /// </search>
        [NodeCategory("Actions")]
        public static string AddPoint(global::Revit.Elements.Element roof, Point point)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.RoofBase internalRoof = (Autodesk.Revit.DB.RoofBase)roof.InternalElement;

            string result;
            try
            {
                TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);
                internalRoof.SlabShapeEditor.Enable();
                SlabShapeVertex vertex1 = internalRoof.SlabShapeEditor.DrawPoint(point.ToXyz());
                TransactionManager.Instance.TransactionTaskDone();
                result = "Success.";
            }
            catch (Exception)
            {
                result = "not so success.";
            }

            return result;
        }
    }
}

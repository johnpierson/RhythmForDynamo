using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Curve = Autodesk.DesignScript.Geometry.Curve;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for reference planes.
    /// </summary>
    public class ReferencePlanes
    {
        private ReferencePlanes()
        { }
        /// <summary>
        /// This node will get the underlying curve of the reference plane in a given view.
        /// </summary>
        /// <param name="referencePlane">The reference plane to get curves from.</param>
        /// <param name="view">The view to obtain the curves in.</param>
        /// <returns name="curve">The room that is tagged.</returns>
        /// <search>
        /// referenceplane,referenceplane.getcurvesinview
        /// </search>
        [NodeCategory("Query")]
        public static List<Curve> GetCurvesInView(List<global::Revit.Elements.Element> referencePlane, global::Revit.Elements.Views.View view)
        {
            List<Curve> curveList = new List<Curve>();
            Autodesk.Revit.DB.View internalView = (Autodesk.Revit.DB.View)view.InternalElement;
            foreach (var refPlane in referencePlane)
            {
              Autodesk.Revit.DB.DatumPlane internalReferencePlane = (Autodesk.Revit.DB.DatumPlane)refPlane.InternalElement;
                IList<Autodesk.Revit.DB.Curve> internalCurves =
                    internalReferencePlane.GetCurvesInView(DatumExtentType.ViewSpecific, internalView);
                foreach (var curve in internalCurves)
                {
                    curveList.Add(curve.ToProtoType(true));
                }          
            }          
            return curveList;
        }

        /// <summary>
        /// This will create a reference plane by the given curve and the selected direction. True for plan view and false for a section parallel to the line.
        /// </summary>
        /// <param name="curve">The curve to use.</param>
        /// <param name="drawInPlan">Choose whether or not to draw in plan or a section view of the curve, (looking at it).</param>
        /// <returns></returns>
        [NodeCategory("Create")]
        public static global::Revit.Elements.Element ByLine(Curve curve, bool drawInPlan = true)
        {
            Document doc = DocumentManager.Instance.CurrentDBDocument;

            Vector normal = Vector.ZAxis();
            if (!drawInPlan)
            {
                normal = curve.NormalAtParameter(0.5);
            }

            Autodesk.Revit.DB.ReferencePlane referencePlane;
            TransactionManager.Instance.EnsureInTransaction(doc);
            if (doc.IsFamilyDocument)
            {
                referencePlane = doc.FamilyCreate.NewReferencePlane(curve.StartPoint.ToRevitType(), curve.EndPoint.ToRevitType(), normal.ToRevitType(), doc.ActiveView);
            }
            else
            {
                referencePlane = doc.Create.NewReferencePlane(curve.StartPoint.ToRevitType(), curve.EndPoint.ToRevitType(), normal.ToRevitType(), doc.ActiveView);
            }
            TransactionManager.Instance.TransactionTaskDone();

            return referencePlane.ToDSType(true);
        }
        /// <summary>
        ///     Get Null
        /// </summary>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static object XAxis()
        {
            return Vector.XAxis();
        }
    }
}

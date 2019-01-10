using System.Collections.Generic;
using Autodesk.Revit.DB;
using Revit.GeometryConversion;
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Revit.GeometryConversion;
using RevitServices.Transactions;

namespace Rhythm.Revit.Elements
{
    public class Viewport
    {
        private Viewport()
        {
        }

        public static void SetViewTitleLength(global::Revit.Elements.Element viewport, double length)
        {
            var internalViewport = viewport.InternalElement as Autodesk.Revit.DB.Viewport;

            internalViewport.LabelLineLength = length;
        }
        public static void SetViewTitleLocation(global::Revit.Elements.Element viewport, Point location)
        {
            var internalViewport = viewport.InternalElement as Autodesk.Revit.DB.Viewport;
            var doc = internalViewport.Document;

            var xyz = location.ToXyz(true);

            TransactionManager.Instance.EnsureInTransaction(doc);
            internalViewport.LabelOffset = xyz;
            TransactionManager.Instance.TransactionTaskDone();
        }

        public static Point GetViewTitleLocation(global::Revit.Elements.Element viewport)
        {
            var internalViewport = viewport.InternalElement as Autodesk.Revit.DB.Viewport;

            return internalViewport.LabelOffset.ToPoint();
        }
    }
}

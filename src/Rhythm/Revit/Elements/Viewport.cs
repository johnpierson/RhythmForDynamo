using Autodesk.DesignScript.Runtime;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using RevitServices.Persistence;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Transactions;
using Plane = Autodesk.DesignScript.Geometry.Plane;
using Surface = Autodesk.DesignScript.Geometry.Surface;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for viewports.
    /// </summary>
    public class Viewport
    {
        private Viewport()
        { }
        /// <summary>
        /// This node will place the given view on the given sheet, if possible. For floor plan views: They cannot be on any other sheets. Now supports schedules! 
        /// </summary>
        /// <param name="sheet">The sheet to place views on.</param>
        /// <param name="view">The view to place.</param>
        /// <param name="location">The location of the view.</param>
        /// <returns name="Result">The result</returns>
        /// <search>
        /// viewport, addview,rhythm
        /// </search>
        public static global::Revit.Elements.Element Create(global::Revit.Elements.Views.Sheet sheet, global::Revit.Elements.Element view, Autodesk.DesignScript.Geometry.Point location)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //obtain the element id from the sheet
            ElementId sheetId = new ElementId(sheet.Id);
            global::Revit.Elements.Element result = null;

            if (view.InternalElement.ToString() == "Autodesk.Revit.DB.ViewSchedule")
            {
                //obtain the element id from the view
                ElementId viewId = new ElementId(view.Id);
                //chane the dynamo point to a revit point
                var revitPoint = location.ToRevitType(true);
                //start the transaction to place views
                TransactionManager.Instance.EnsureInTransaction(doc);
                result = Autodesk.Revit.DB.ScheduleSheetInstance.Create(doc, sheetId, viewId, revitPoint).ToDSType(true);
                TransactionManager.Instance.TransactionTaskDone();
            }
            else
            {
                //obtain the element id from the view
                ElementId viewId = new ElementId(view.Id);
                //chane the dynamo point to a revit point
                var revitPoint = location.ToRevitType(true);
                //start the transaction to place views
                TransactionManager.Instance.EnsureInTransaction(doc);
                result = Autodesk.Revit.DB.Viewport.Create(doc, sheetId, viewId, revitPoint).ToDSType(true);
                TransactionManager.Instance.TransactionTaskDone();
            }

            return result;
        }
        /// <summary>
        /// This node will obtain the box location data from the provided viewport.
        /// </summary>
        /// <param name="viewport">Viewport to obtain data from.</param>
        /// <returns name="bBox">The bounding box of the viewport.</returns>
        /// <returns name="boxCenter">The center of the viewport.</returns>
        /// <returns name="boxOutline">The outline of the viewport.</returns>
        /// <search>
        /// viewport, Viewport.LocationData,rhythm
        /// </search>
        //this is the node Viewport.LocationData
        [MultiReturn(new[] { "bBox", "boxCenter", "boxOutline" })]
        public static Dictionary<string, object> LocationData(global::Revit.Elements.Element viewport)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //obtain the element id from the sheet
            Autodesk.Revit.DB.Viewport internalViewport = (Autodesk.Revit.DB.Viewport)viewport.InternalElement;
            //obtain the box center of the viewport
            var boxCenterInternal = internalViewport.GetBoxCenter().ToPoint();
            //Construct new point at sheet elevation of 0
            Autodesk.DesignScript.Geometry.Point boxCenter =
                Autodesk.DesignScript.Geometry.Point.ByCoordinates(boxCenterInternal.X, boxCenterInternal.Y, 0);
            //this obtains the box outline
            var boxOutline = internalViewport.GetBoxOutline();
            var bBox = BoundingBox.ByCorners(boxOutline.MaximumPoint.ToPoint(), boxOutline.MinimumPoint.ToPoint());
            var boxCuboid = Cuboid.ByCorners(boxOutline.MaximumPoint.ToPoint(), boxOutline.MinimumPoint.ToPoint());
            //create plane that corresponds to sheet plane
            Plane boxPlane = Plane.ByOriginNormal(boxOutline.MaximumPoint.ToPoint(), Vector.ZAxis());
            var boxSurface = boxCuboid.Intersect(boxPlane);
            List<Autodesk.DesignScript.Geometry.Curve[]> boxCurves = new List<Autodesk.DesignScript.Geometry.Curve[]>();
            foreach (Surface surf in boxSurface)
            {
                boxCurves.Add(surf.PerimeterCurves());
            }
            List<Autodesk.DesignScript.Geometry.Curve> boxSheetCurves = new List<Autodesk.DesignScript.Geometry.Curve>();
            //pull the curves onto a plane at 0,0,0
            foreach (Autodesk.DesignScript.Geometry.Curve[] curve in boxCurves)
            {
                foreach (Autodesk.DesignScript.Geometry.Curve c in curve)
                {
                    boxSheetCurves.Add(c.PullOntoPlane(Plane.XY()));
                }
            }
            //returns the outputs
            var outInfo = new Dictionary<string, object>
                {
                    { "bBox", bBox},
                    { "boxCenter", boxCenter},
                    { "boxOutline", boxSheetCurves}
                };
            return outInfo;
        }

        /// <summary>
        /// This node will obtain the outline of the Viewport title if one is used. This is the label outline.
        /// </summary>
        /// <param name="viewport">Viewport to obtain data from.</param>
        /// <returns name="labelOutline">The label outline of the viewport.</returns>
        /// <search>
        /// viewport, Viewport.LabelOutline, rhythm
        /// </search>
        public static List<Autodesk.DesignScript.Geometry.Curve> LabelOutline(global::Revit.Elements.Element viewport)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //obtain the element id from the sheet
            Autodesk.Revit.DB.Viewport internalViewport = (Autodesk.Revit.DB.Viewport)viewport.InternalElement;

            //this obtains the label outline
            var labelOutline = internalViewport.GetLabelOutline();
            //create plane that corresponds to sheet plane
            Plane labelPlane = Plane.ByOriginNormal(labelOutline.MaximumPoint.ToPoint(), Vector.ZAxis());
            var labelCuboid = Cuboid.ByCorners(labelOutline.MaximumPoint.ToPoint(), labelOutline.MinimumPoint.ToPoint());
            var labelSurface = labelCuboid.Intersect(labelPlane);
            List<Autodesk.DesignScript.Geometry.Curve[]> labelCurves = new List<Autodesk.DesignScript.Geometry.Curve[]>();
            foreach (Surface surf in labelSurface)
            {
                labelCurves.Add(surf.PerimeterCurves());
            }
            List<Autodesk.DesignScript.Geometry.Curve> labelSheetCurves = new List<Autodesk.DesignScript.Geometry.Curve>();
            //pull the curves onto a plane at 0,0,0
            foreach (Autodesk.DesignScript.Geometry.Curve[] curve in labelCurves)
            {
                foreach (Autodesk.DesignScript.Geometry.Curve c in curve)
                {
                    labelSheetCurves.Add(c.PullOntoPlane(Plane.XY()));
                }
            }

            return labelSheetCurves;
        }

        /// <summary>
        /// This node will obtain the view from the given viewport.
        /// </summary>
        /// <param name="viewport">Viewport to obtain view from.</param>
        /// <returns name="view">The view that belongs to the viewport.</returns>
        /// <search>
        /// viewport, location,rhythm
        /// </search>
        public static global::Revit.Elements.Element GetView(global::Revit.Elements.Element viewport)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.Viewport internalViewport = (Autodesk.Revit.DB.Viewport)viewport.InternalElement;
            ElementId viewId = internalViewport.ViewId;
            global::Revit.Elements.Element view = doc.GetElement(viewId).ToDSType(true);

            return view;
        }

        /// <summary>
        /// This node will set the child viewports box center given the parent viewport.
        /// </summary>
        /// <param name="parentViewport">Viewport to get location from.</param>
        /// <param name="childViewports">Viewports to set to location collected.</param>
        /// <returns name="childViewports">The viewports you moved.</returns>
        /// <search>
        /// viewport
        /// </search>
        public static List<global::Revit.Elements.Element> SetLocationBasedOnOther(global::Revit.Elements.Element parentViewport, List<global::Revit.Elements.Element> childViewports)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.Viewport internalViewport = (Autodesk.Revit.DB.Viewport)parentViewport.InternalElement;
            Autodesk.Revit.DB.XYZ locationToUse = internalViewport.GetBoxCenter();

            List<Autodesk.Revit.DB.Viewport> internalChildViewports = new List<Autodesk.Revit.DB.Viewport>();
            foreach (global::Revit.Elements.Element viewport in childViewports)
            {
                internalChildViewports.Add((Autodesk.Revit.DB.Viewport)viewport.InternalElement);
            }

            foreach (Autodesk.Revit.DB.Viewport vPort in internalChildViewports)
            {
                TransactionManager.Instance.EnsureInTransaction(doc);
                vPort.SetBoxCenter(locationToUse);
                TransactionManager.Instance.TransactionTaskDone();
            }

            return childViewports;
        }
    }
}

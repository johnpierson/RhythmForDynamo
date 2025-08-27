using System;
using Autodesk.DesignScript.Runtime;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Transactions;
using Plane = Autodesk.DesignScript.Geometry.Plane;
using Point = Autodesk.DesignScript.Geometry.Point;
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
        /// 
        /// </summary>
        /// <param name="viewport"></param>
        public static void AlignViewTitle(global::Revit.Elements.Element viewport)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            //cast the viewport to the internal Revit DB Type
            Autodesk.Revit.DB.Viewport internalViewport = viewport.InternalElement as Autodesk.Revit.DB.Viewport;
            
            //get the original box center (for when we re-place the viewport)
            var originalBoxCenter = internalViewport.GetBoxCenter();

            var sheetId = internalViewport.SheetId;
            var viewId = internalViewport.ViewId;

            //force close dynamo's open transaction
            TransactionManager.Instance.ForceCloseTransaction();

            //use a transaction group to sequence the events into one
            TransactionGroup tGroup = new TransactionGroup(doc, "Aligning Viewport");
            tGroup.Start();

            //delete the original viewport
            using (Transaction deleteOriginalViewport = new Transaction(doc, "Deleting Original"))
            {
                deleteOriginalViewport.Start();
                doc.Delete(internalViewport.Id);
                deleteOriginalViewport.Commit();
            }

            //place the viewport again to get an aligned viewport title
            using (Transaction replaceViewport = new Transaction(doc, "Placing Viewport with Aligned View Title"))
            {
                replaceViewport.Start();
                Autodesk.Revit.DB.Viewport.Create(doc, sheetId, viewId, originalBoxCenter);
                replaceViewport.Commit();
            }

            tGroup.Assimilate();

        }

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
        [IsObsolete("Node removed, use the OOTB nodes, Viewport.BySheetViewLocation ")]
        [NodeCategory("Create")]
        public static object Create(global::Revit.Elements.Views.Sheet sheet, global::Revit.Elements.Element view, Autodesk.DesignScript.Geometry.Point location)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

           //obtain the element id from the sheet
           Autodesk.Revit.DB.ViewSheet internalSheet = sheet.InternalElement as Autodesk.Revit.DB.ViewSheet;
           ElementId sheetId = internalSheet.Id;
            Autodesk.Revit.DB.Element result = null;
            //change the dynamo point to a revit point
            var revitPoint = location.ToRevitType(true);
            //obtain the element id from the view
            Autodesk.Revit.DB.View internalView = view.InternalElement as Autodesk.Revit.DB.View;
            ElementId viewId = internalView.Id;

            try
            {
                //start the transaction to place views
                TransactionManager.Instance.EnsureInTransaction(doc);
                doc.Regenerate();
                if (view.InternalElement.ToString() == "Autodesk.Revit.DB.ViewSchedule")
                {
                    result = Autodesk.Revit.DB.ScheduleSheetInstance.Create(doc, sheetId, viewId, revitPoint);
                }
                else
                {
                    result = Autodesk.Revit.DB.Viewport.Create(doc, sheetId, viewId, revitPoint);  
                }

                TransactionManager.Instance.TransactionTaskDone();

                return result.ToDSType(false);
            }
            catch (Exception e)
            {
                if (!Autodesk.Revit.DB.Viewport.CanAddViewToSheet(doc, sheetId, viewId))
                {
                    return "Error: View " + view.Id + " cannot be added to the sheet because it is already on another sheet.";
                }
                if (result == null)
                {
                    return "Error: View " + view.Id + " cannot be added to the sheet because it is empty.";
                }

                return "Error: View " + view.Id + e.Message;
            }
          
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
        [IsObsolete("Node removed, use the OOTB nodes, Viewport.BoxCenter and Viewport.BoxOutline ")]
        [MultiReturn(new[] { "bBox", "boxCenter", "boxOutline" })]
        [NodeCategory("Query")]
        public static Dictionary<string, object> LocationData(global::Revit.Elements.Element viewport)
        {
            //obtain the element id from the sheet
            Autodesk.Revit.DB.Viewport internalViewport = (Autodesk.Revit.DB.Viewport)viewport.InternalElement;
            //obtain the box center of the viewport
            var boxCenterInternal = internalViewport.GetBoxCenter().ToPoint();
            //Construct new point at sheet elevation of 0
            Autodesk.DesignScript.Geometry.Point boxCenter =
                Autodesk.DesignScript.Geometry.Point.ByCoordinates(boxCenterInternal.X, boxCenterInternal.Y, 0);
            //this obtains the box outline
            var boxOutline = internalViewport.GetBoxOutline();
            //temporary geometry
            var bBox = BoundingBox.ByCorners(boxOutline.MaximumPoint.ToPoint(), boxOutline.MinimumPoint.ToPoint());
            var boxCuboid = Cuboid.ByCorners(boxOutline.MaximumPoint.ToPoint(), boxOutline.MinimumPoint.ToPoint());
            //create plane that corresponds to sheet plane
            Plane boxPlane = Plane.ByOriginNormal(boxOutline.MaximumPoint.ToPoint(), Vector.ZAxis());
            var boxSurface = boxCuboid.Intersect(boxPlane);
            List<Autodesk.DesignScript.Geometry.Curve[]> boxCurves = new List<Autodesk.DesignScript.Geometry.Curve[]>();
            foreach (var geometry in boxSurface)
            {
                var surf = (Surface)geometry;
                boxCurves.Add(surf.PerimeterCurves());
                surf.Dispose();
            }
            List<Autodesk.DesignScript.Geometry.Curve> boxSheetCurves = new List<Autodesk.DesignScript.Geometry.Curve>();
            //pull the curves onto a plane at 0,0,0
            foreach (Autodesk.DesignScript.Geometry.Curve[] curve in boxCurves)
            {
                foreach (Autodesk.DesignScript.Geometry.Curve c in curve)
                {
                    boxSheetCurves.Add(c.PullOntoPlane(Plane.XY()));
                    c.Dispose();
                }
            }
            
            //dispose of temporary geometries         
            boxCuboid.Dispose();
            boxPlane.Dispose();
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
        [IsObsolete("Node removed, use the OOTB nodes, Viewport.LabelOutline")]
        [NodeCategory("Query")]
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
            List<Autodesk.DesignScript.Geometry.Curve[]> labelCurves = (from Surface surf in labelSurface select surf.PerimeterCurves()).ToList();
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
        [NodeCategory("Query")]
        public static global::Revit.Elements.Element GetView(global::Revit.Elements.Element viewport)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.Viewport internalViewport = (Autodesk.Revit.DB.Viewport)viewport.InternalElement;
            ElementId viewId = internalViewport.ViewId;
            global::Revit.Elements.Element view = doc.GetElement(viewId).ToDSType(false);

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
        [NodeCategory("Actions")]
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

        /// <summary>
        /// This node will set the viewport's box center given the point.
        /// </summary>
        /// <param name="viewport">The viewport to set.</param>
        /// <param name="point">The point to use.</param>
        /// <search>
        /// viewport
        /// </search>
        [IsObsolete("Node removed, use the OOTB nodes, Viewport.SetBoxCenter")]
        [NodeCategory("Actions")]
        public static void SetBoxCenter(global::Revit.Elements.Element viewport, Point point)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.Viewport internalViewport = (Autodesk.Revit.DB.Viewport)viewport.InternalElement;
            TransactionManager.Instance.EnsureInTransaction(doc);
            internalViewport.SetBoxCenter(point.ToRevitType());
            TransactionManager.Instance.TransactionTaskDone();
        }

        /// <summary>
        /// This node will retrieve the viewport's box center.
        /// </summary>
        /// <param name="viewport">The viewport to set.</param>
        /// <returns name="boxCenter">The viewport's box center as a point.</returns>
        /// <search>
        /// viewport
        /// </search>
        [NodeCategory("Query")]
        public static Point BoxCenter(global::Revit.Elements.Element viewport)
        {
            Autodesk.Revit.DB.Viewport internalViewport = (Autodesk.Revit.DB.Viewport)viewport.InternalElement;
            return internalViewport.GetBoxCenter().ToPoint();
        }

#if R22_OR_GREATER
        /// <summary>
        /// Set a viewport's title length. Revit 2022+
        /// </summary>
        /// <param name="viewport">The target viewport.</param>
        /// <param name="length">The length to set it to.</param>
        [NodeCategory("Actions")]
        public static void SetViewTitleLength(global::Revit.Elements.Element viewport, double length)
        {


            var internalViewport = viewport.InternalElement as Autodesk.Revit.DB.Viewport;
            var doc = internalViewport.Document;

            TransactionManager.Instance.EnsureInTransaction(doc);
            internalViewport.LabelLineLength = length;
            TransactionManager.Instance.TransactionTaskDone();

        }
        /// <summary>
        /// Set a viewport's title location (relative to the boundary of the view) Revit 2022+.
        /// </summary>
        /// <param name="viewport">The target viewport.</param>
        /// <param name="location">The location to set it to.</param>
        [NodeCategory("Actions")]
        public static void SetViewTitleLocation(global::Revit.Elements.Element viewport, Point location)
        {


            var internalViewport = viewport.InternalElement as Autodesk.Revit.DB.Viewport;
            var doc = internalViewport.Document;

            var xyz = location.ToXyz(true);

            TransactionManager.Instance.EnsureInTransaction(doc);
            internalViewport.LabelOffset = xyz;
            TransactionManager.Instance.TransactionTaskDone();
        }
        /// <summary>
        /// Get a viewport's title location (relative to the boundary of the view) Revit 2022+.
        /// </summary>
        /// <param name="viewport">The target viewport.</param>
        [NodeCategory("Actions")]
        public static Point GetViewTitleLocation(global::Revit.Elements.Element viewport)
        {
            var internalViewport = viewport.InternalElement as Autodesk.Revit.DB.Viewport;

            return internalViewport.LabelOffset.ToPoint();
        }
#endif
    }

}

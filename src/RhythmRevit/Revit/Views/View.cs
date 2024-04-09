using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Element = Revit.Elements.Element;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace Rhythm.Revit.Views
{
    /// <summary>
    /// Wrapper class for view.
    /// </summary>
    public class View
    {
        private View()
        { }

        /// <summary>
        /// Retrieve the view's viewport(s) if there is one.
        /// </summary>
        /// <param name="view"></param>
        /// <returns name="viewport"></returns>
        public static List<global::Revit.Elements.Element> Viewport(global::Revit.Elements.Element view)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;


#if R20 || R21 || R22 || R23
           var viewports = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Viewports)
                .Cast<Autodesk.Revit.DB.Viewport>().Where(v => v.ViewId.IntegerValue.Equals(view.Id)).ToList();
#endif

#if R24_OR_GREATER
            var viewports = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Viewports)
                .Cast<Autodesk.Revit.DB.Viewport>().Where(v => v.ViewId.Value.Equals(view.Id)).ToList();
#endif



            return !viewports.Any() ? new List<Element>() : viewports.Select(v => doc.GetElement(v.Id).ToDSType(false)).ToList();
        }


        /// <summary>
        /// This node will convert a dependent view to an independent.
        /// </summary>
        /// <param name="dependentView">The view to convert to independent.</param>
        /// <returns name="independentView">The overidden view.</returns>
        /// <search>
        /// view, dependent,rhythm
        /// </search>
        public static List<object> ConvertToIndependent(global::Revit.Elements.Element dependentView)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.View internalView = (Autodesk.Revit.DB.View)dependentView.InternalElement;
            List<object> independentView = new List<object>();
            TransactionManager.Instance.EnsureInTransaction(doc);
            if (internalView.GetPrimaryViewId() != ElementId.InvalidElementId)
            {
                internalView.ConvertToIndependent();
                independentView.Add((global::Revit.Elements.Views.View)internalView.ToDSType(true));
            }
            else
            {
                independentView.Add("Silly Dynamo user. This is not a dependent view.");
            }
            TransactionManager.Instance.TransactionTaskDone();

            return independentView;
        }

        /// <summary>
        /// This node will obtain the crop region element from the view.
        /// </summary>
        /// <param name="view">The view to obtain the crop region element from.</param>
        /// <returns name="cropRegionElement">The crop region as an element.</returns>
        /// <search>
        /// crop region,rhythm
        /// </search>
        public static object GetCropRegionElement(global::Revit.Elements.Element view)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            List<Autodesk.Revit.DB.View> internalView = new List<Autodesk.Revit.DB.View>
            {
                (Autodesk.Revit.DB.View) view.InternalElement
            };

            Autodesk.Revit.DB.Element cropBoxInternalElement = null;
            using (TransactionGroup tGroup = new TransactionGroup(doc))
            {
                TransactionManager.Instance.ForceCloseTransaction();
                tGroup.Start("Temp to find crop box element");
                using (Transaction t2 = new Transaction(doc, "Temp to find crop box element"))
                {
                    foreach (Autodesk.Revit.DB.View v in internalView)
                    {
                        // Deactivate crop box
                        t2.Start();
                        v.CropBoxVisible = false;
                        t2.Commit();
                        // Get all visible elements;
                        // this excludes hidden crop box
                        FilteredElementCollector collector = new FilteredElementCollector(doc, v.Id);
                        ICollection<ElementId> shownElems = collector.ToElementIds();
                        // Activate crop box
                        t2.Start();
                        v.CropBoxVisible = true;
                        t2.Commit();
                        // Get all visible elements excluding
                        // everything except the crop box
                        collector = new FilteredElementCollector(doc, v.Id);
                        collector.Excluding(shownElems);
                        cropBoxInternalElement = collector.FirstElement();
                    }
                }
                tGroup.RollBack();
            }
            ElementId cropBoxId = cropBoxInternalElement.Id;

            var cropBoxElement = doc.GetElement(cropBoxId).ToDSType(true);

            return cropBoxElement;
        }

        /// <summary>
        /// This node will override the given element's projection lineweight in given view.
        /// </summary>
        /// <param name="view">The view to set the overrides in.</param>
        /// <param name="element">The element to override.</param>
        /// <param name="lineweight">The lineweight to use.</param>
        /// <returns name="element">The crop region as an element.</returns>
        /// <search>
        /// crop region,rhythm
        /// </search>
        public static global::Revit.Elements.Element SetElementProjectionLineweight(global::Revit.Elements.Element view, global::Revit.Elements.Element element, int lineweight)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            Autodesk.Revit.DB.View internalView = (Autodesk.Revit.DB.View)view.InternalElement;

            OverrideGraphicSettings gSettings = new OverrideGraphicSettings().SetProjectionLineWeight(lineweight);
            TransactionManager.Instance.EnsureInTransaction(doc);
            internalView.CropBoxVisible = true;
            internalView.SetElementOverrides(element.InternalElement.Id, gSettings);
            TransactionManager.Instance.TransactionTaskDone();

            return element;
        }

        /// <summary>
        /// This node will supply the visibility of the given filter in given view.
        /// </summary>
        /// <param name="view">The view to obtain filter visibility from.</param>
        /// <param name="viewFilter">The view filter.</param>
        /// <returns name="bool">The visibility value.</returns>
        /// <search>
        /// view, dependent,rhythm
        /// </search>
        public static bool GetFilterVisibility(global::Revit.Elements.Element view, global::Revit.Elements.Element viewFilter)
        {
            Autodesk.Revit.DB.View internalView = (Autodesk.Revit.DB.View)view.InternalElement;
            Autodesk.Revit.DB.ParameterFilterElement parameterFilter =
                (Autodesk.Revit.DB.ParameterFilterElement)viewFilter.InternalElement;

            bool mask = internalView.GetFilterVisibility(parameterFilter.Id);

            return mask;
        }

        /// <summary>
        /// Retrieve the input dependent view's parent, (if available).
        /// </summary>
        /// <param name="view">The view to get parent of.</param>
        /// <returns name="parentView">The parent view.</returns>
        /// <search>
        /// dependent, parent, rhythm
        /// </search>
        public static object ParentView(global::Revit.Elements.Element view)
        {
            //the current document
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //the internal revit view
            Autodesk.Revit.DB.View internalView = (Autodesk.Revit.DB.View)view.InternalElement;
            //declare a blank result to append result to
            object result;
            //provide result based on conditions
            if (doc.GetElement(internalView.GetPrimaryViewId()) == null)
            {
                result = "No parent  view found. Is your view in fact dependent?";
            }
            else
            {
                result = doc.GetElement(internalView.GetPrimaryViewId()).ToDSType(true);
            }

            return result;
        }
        /// <summary>
        /// Retrieve the input view's origin, (if available).
        /// </summary>
        /// <param name="view">The view to get origin of.</param>
        /// <returns name="origin">The origin of the view. Also, the origin of a plan view is not meaningful.</returns>
        /// <search>
        /// dependent, parent, rhythm
        /// </search>
        public static object Origin(global::Revit.Elements.Element view)
        {
            //the internal revit view
            Autodesk.Revit.DB.View internalView = (Autodesk.Revit.DB.View)view.InternalElement;
            //declare a blank result to append result to
            Point result;
            //provide result based on conditions
            try
            {
                result = internalView.Origin.ToPoint();
            }
            catch (Exception)
            {
                result = null;
            }

            return result;
        }
        /// <summary>
        /// This node will supply the visibility of the given workset in given view.
        /// </summary>
        /// <param name="view">The view to obtain workset visibility from.</param>
        /// <param name="worksetId">The workset element id as int.</param>
        /// <returns name="result">The workset visibility.</returns>
        /// <search>
        /// view, workset.Visible
        /// </search>
        public static List<string> GetWorksetVisibility(global::Revit.Elements.Element view, List<int> worksetId)
        {
            //the current document
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            List<string> result = new List<string>();
            Autodesk.Revit.DB.View internalView = (Autodesk.Revit.DB.View)view.InternalElement;
            foreach (int i in worksetId)
            {
                Autodesk.Revit.DB.WorksetId worksetElementId = new WorksetId(i);
                if (internalView.GetWorksetVisibility(worksetElementId) == WorksetVisibility.Hidden)
                {
                    result.Add("Hide");
                }
                if (internalView.GetWorksetVisibility(worksetElementId) == WorksetVisibility.Visible)
                {
                    result.Add("Show");
                }
                if (internalView.GetWorksetVisibility(worksetElementId) == WorksetVisibility.UseGlobalSetting)
                {
                    var w = new Autodesk.Revit.DB.FilteredWorksetCollector(doc).OfKind(Autodesk.Revit.DB.WorksetKind.UserWorkset).FirstOrDefault(x => x.Id.IntegerValue == i);
                    if (w.IsVisibleByDefault)
                    {
                        result.Add("Use Global Setting (Visible)");
                    }
                    else
                    {
                        result.Add("Use Global Setting (Not Visible)");
                    }
                }
            }

            return result;
        }
        ///// <summary>
        ///// This will attempt to set the view's crop region given a bounding box.
        ///// </summary>
        ///// <param name="view">The view to set the section box to.</param>
        ///// <param name="bBox">The bounding box to use.</param>
        ///// <returns name="result">The result of the operation.</returns>
        ///// <search>
        ///// cropregion, crop, rhythm
        ///// </search>
        //public static string SetCropRegion(global::Revit.Elements.Element view, BoundingBox bBox)
        //{
        //    //the current document
        //    Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

        //    string result = string.Empty;

        //    //the internal revit view
        //    Autodesk.Revit.DB.View internalView = (Autodesk.Revit.DB.View)view.InternalElement;

        //    //get the views crop region shape and view plane
        //    ViewCropRegionShapeManager cropRegionManager = internalView.GetCropRegionShapeManager();
        //    var direction = internalView.ViewDirection.ToVector();
        //    var origin = internalView.Origin.ToPoint();
        //    var viewPlane = Plane.ByOriginNormal(origin, direction);

        //    //ensure the bounding box is close
        //    var originalCuboid = bBox.ToCuboid();
        //    //var originalOrigin = CoordinateSystem.ByOrigin(originalCuboid.Centroid());
        //    //var newOrigin = CoordinateSystem.ByOrigin(origin);

        //    //var newCuboid = originalCuboid.Transform(originalOrigin, newOrigin);

        //    Surface intersectingSurface;
        //    //get surface, extract perimeter curves and make a curve loop
        //    try
        //    {
        //        intersectingSurface = (Autodesk.DesignScript.Geometry.Surface)originalCuboid.Intersect(viewPlane)[0];
        //    }
        //    catch (Exception)
        //    {
        //        throw new Exception("The input bounding box does not intersect the view plane.");
        //    }

        //    var curves = intersectingSurface.PerimeterCurves();
        //    var newCurveLoop = CurveLoop.Create(curves.Select(c => c.ToRevitType(true)).ToList());

        //   //try to set the crop region
        //    TransactionManager.Instance.EnsureInTransaction(doc);
        //    try
        //    {
        //        cropRegionManager.SetCropShape(newCurveLoop);
        //        result = $"Crop Region Updated for: {view.Name}";
        //    }
        //    catch (Exception e)
        //    {
        //        result = $"Crop Region Not Updated for: {view.Name}. The Error Revit gave us is: {e.Message}.";
        //    }

        //    TransactionManager.Instance.TransactionTaskDone();

        //    return result;
        //}

#if !R20

        /// <summary>
        /// Revit 2021 - Checks if a view filter is enabled in the given view.
        /// </summary>
        /// <param name="view">The view to check the filter on.</param>
        /// <param name="viewFilter"> The view filter to check.</param>
        /// <returns name="bool">Is it enabled?</returns>
        public static bool IsFilterEnabled(global::Revit.Elements.Views.View view,
            global::Revit.Elements.Element viewFilter)
        {
            Autodesk.Revit.DB.View internalView = view.InternalElement as Autodesk.Revit.DB.View;
            return internalView.GetIsFilterEnabled(viewFilter.InternalElement.Id);
        }
        /// <summary>
        /// Revit 2021 - This attempts to enable or disable a filter for a given view.
        /// </summary>
        /// <param name="view">The view to check the filter on.</param>
        /// <param name="viewFilter">The view filter to check.</param>
        /// <param name="toggle">Toggle it to enabled or disabled.</param>
        /// <returns></returns>
        public static void ToggleFilterInView(global::Revit.Elements.Views.View view, global::Revit.Elements.Element viewFilter, bool toggle)
        {
            Autodesk.Revit.DB.View internalView = view.InternalElement as Autodesk.Revit.DB.View;
            internalView.SetIsFilterEnabled(viewFilter.InternalElement.Id, toggle);
        }
        /// <summary>
        /// Revit 2021 - Returns the filters in order for the given view.
        /// </summary>
        /// <param name="view">The view to check the filter on.</param>
        /// <returns></returns>
        public static List<global::Revit.Elements.Element> GetOrderedFilters(global::Revit.Elements.Views.View view)
        {
            Autodesk.Revit.DB.View internalView = view.InternalElement as Autodesk.Revit.DB.View;
            Document doc = internalView.Document;

            return internalView.GetOrderedFilters().Select(f => doc.GetElement(f).ToDSType(true)).ToList();
        }
#endif
    }

}

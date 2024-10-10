using System;
using Autodesk.DesignScript.Runtime;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using Revit.Elements;
using Revit.Elements.Views;
using Revit.GeometryConversion;
using Plane = Autodesk.DesignScript.Geometry.Plane;
using Point = Autodesk.DesignScript.Geometry.Point;
using Surface = Autodesk.DesignScript.Geometry.Surface;
using RevitServices.Persistence;
using RevitServices.Transactions;
using GlobalParameter = Autodesk.Revit.DB.GlobalParameter;
using System.Linq;

namespace Rhythm.Revit.Views
{
    /// <summary>
    /// Wrapper class for plans.
    /// </summary>
    public class ViewPlan
    {
        private ViewPlan()
        {
        }
        /// <summary>
        /// This node will get the bounds of the view in paper space (in feet).
        /// </summary>
        /// <param name="view">The view to get outline from.</param>
        /// <returns name="outline">The bounds of the view in paper space (in feet).</returns>
        /// <search>
        /// view, outline,rhythm
        /// </search>
        public static List<Autodesk.DesignScript.Geometry.Curve[]> GetOutline(global::Revit.Elements.Element view)
        {
            Autodesk.Revit.DB.View internalView = (Autodesk.Revit.DB.View)view.InternalElement;
            var viewBbox = internalView.Outline;
            var viewCuboid = Cuboid.ByCorners(Point.ByCoordinates(viewBbox.Max.U, viewBbox.Max.V, 0),
                Point.ByCoordinates(viewBbox.Min.U, viewBbox.Min.V, 0));
            var viewOutlineSurface = viewCuboid.Intersect(Plane.XY());
            List<Autodesk.DesignScript.Geometry.Curve[]> viewOutlineCurves =
                new List<Autodesk.DesignScript.Geometry.Curve[]>();
            foreach (Surface surf in viewOutlineSurface)
            {
                viewOutlineCurves.Add(surf.PerimeterCurves());
            }

            return viewOutlineCurves;
        }

        /// <summary>
        /// This node will get the bounds of the view in paper space (in feet).
        /// </summary>
        /// <param name="viewPlan">The plan view to get outline from.</param>
        /// <returns name="cropBox">The cropBox.</returns>
        /// <returns name="cropBoxCurves">The curves of the crop region.</returns>
        /// <search>
        /// view, outline,rhythm
        /// </search>
        [MultiReturn(new[] { "cropBox", "cropBoxCurves" })]
        public static Dictionary<string, object> GetCropBox(global::Revit.Elements.Element viewPlan)
        {

            Autodesk.Revit.DB.View internalView = (Autodesk.Revit.DB.View)viewPlan.InternalElement;
            var viewBbox = internalView.CropBox;
            var viewCuboid = Cuboid.ByCorners(viewBbox.Max.ToPoint(), viewBbox.Min.ToPoint());
            var viewOutlineSurface = viewCuboid.Intersect(internalView.SketchPlane.GetPlane().ToPlane());
            List<Autodesk.DesignScript.Geometry.Curve[]> viewOutlineCurves =
                new List<Autodesk.DesignScript.Geometry.Curve[]>();
            foreach (Surface surf in viewOutlineSurface)
            {
                viewOutlineCurves.Add(surf.PerimeterCurves());
            }
            //returns the outputs
            var outInfo = new Dictionary<string, object>
                {
                    {"cropBox", viewBbox},
                    {"cropBoxCurves", viewOutlineCurves}
                };
            return outInfo;
        }

        /// <summary>
        /// This node will attempt to rotate a plan view into a 3D view. Use at your own risk!
        /// </summary>
        /// <param name="viewPlan">The plan view to rotate</param>
        /// <param name="rotationLine">The line to rotate along.</param>
        /// <param name="inputAngle">The plan view to get outline from.</param>
        /// <returns name="cropBox">The cropBox.</returns>
        /// <search>
        /// view, outline,rhythm
        /// </search>
        public static object Rotate(global::Revit.Elements.Element viewPlan, global::Revit.Elements.Element rotationLine, double inputAngle)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.View internalView = (Autodesk.Revit.DB.View)viewPlan.InternalElement;
            var viewBbox = internalView.CropBox;
            Autodesk.Revit.DB.CurveElement internalLine = (Autodesk.Revit.DB.CurveElement)rotationLine.InternalElement;

            Autodesk.Revit.DB.Line rotationLineBound = Autodesk.Revit.DB.Line.CreateBound(internalLine.GeometryCurve.GetEndPoint(0), internalLine.GeometryCurve.GetEndPoint(1));

            Autodesk.Revit.DB.Element cropBoxElement = null;
            using (TransactionGroup tGroup = new TransactionGroup(doc))
            {
                tGroup.Start("Temp to find crop box element");
                using (Transaction t2 = new Transaction(doc, "Temp to find crop box element"))
                {
                    // Deactivate crop box
                    t2.Start();
                    internalView.CropBoxVisible = false;
                    t2.Commit();
                    // Get all visible elements;
                    // this excludes hidden crop box
                    FilteredElementCollector collector = new FilteredElementCollector(doc, internalView.Id);
                    ICollection<ElementId> shownElems = collector.ToElementIds();
                    // Activate crop box
                    t2.Start();
                    internalView.CropBoxVisible = true;
                    t2.Commit();
                    // Get all visible elements excluding
                    // everything except the crop box
                    collector = new FilteredElementCollector(doc, internalView.Id);
                    collector.Excluding(shownElems);
                    cropBoxElement = collector.FirstElement();
                }
                tGroup.RollBack();
            }
            TransactionManager.Instance.EnsureInTransaction(doc);
            ElementTransformUtils.RotateElement(doc, cropBoxElement.Id, rotationLineBound, inputAngle);
            TransactionManager.Instance.TransactionTaskDone();

            return cropBoxElement;
        }

        public static global::Revit.Elements.PlanView ByLevelTypeAndName(global::Revit.Elements.Level level, global::Revit.Elements.Element viewFamilyType, string viewName)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            Autodesk.Revit.DB.ViewFamilyType vft;
            if (viewFamilyType == null)
            {
                vft = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                    .OfClass(typeof(Autodesk.Revit.DB.ViewFamilyType))
                    .Cast<Autodesk.Revit.DB.ViewFamilyType>()
                    .FirstOrDefault(x => x.ViewFamily == Autodesk.Revit.DB.ViewFamily.FloorPlan);
            }
            else
            {
                vft = viewFamilyType.InternalElement as Autodesk.Revit.DB.ViewFamilyType;
            }

            if (vft?.ViewFamily != Autodesk.Revit.DB.ViewFamily.FloorPlan)
                throw new ArgumentException(nameof(viewFamilyType));



            TransactionManager.Instance.ForceCloseTransaction();

            global::Revit.Elements.PlanView newPlanView;

            using (TransactionGroup tGroup = new TransactionGroup(doc, "Create new plan views with names."))
            {
                tGroup.Start();
                //create the view first
                Autodesk.Revit.DB.ViewPlan viewPlan;
                using (Transaction create = new Transaction(doc,"Creating View"))
                {
                    create.Start();
                    viewPlan = Autodesk.Revit.DB.ViewPlan.Create(doc, vft.Id, level.InternalElement.Id);
                    create.Commit();
                }


                var planViewsWithName = new FilteredElementCollector(doc).OfClass(typeof(Autodesk.Revit.DB.ViewPlan)).WhereElementIsNotElementType().Where(v => v.Name.Equals(viewName)).ToList();


                if (!planViewsWithName.Any())
                {
                    //now name that thing
                    using (Transaction name = new Transaction(doc, "Naming View"))
                    {
                        name.Start();
                        viewPlan.Name = viewName;
                        name.Commit();
                    }
                }

                newPlanView = viewPlan.ToDSType(true) as global::Revit.Elements.PlanView;

                tGroup.Assimilate();
            }

            return newPlanView;
        }
    }
}
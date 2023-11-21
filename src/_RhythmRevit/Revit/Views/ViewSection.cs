using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace Rhythm.Revit.Views
{
    /// <summary>
    /// Wrapper class for sections.
    /// </summary>
    public class ViewSection
    {
        private ViewSection()
        { }

        /// <summary>
        /// This node will override the crop region of the given section view based on the pen weight provided.
        /// Slower but more reliable version that uses transaction rollback to isolated the crop region element.
        /// </summary>
        /// <param name="viewSection">The plan view to rotate</param>
        /// <param name="lineWeight">The line weight to override to, (1-16)</param>
        /// <returns name="viewSection">The overridden view.</returns>
        /// <search>
        /// overridecrop
        /// </search>
        public static global::Revit.Elements.Element OverrideCrop(global::Revit.Elements.Element viewSection, int lineWeight)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            
            List<Autodesk.Revit.DB.View> internalView = new List<Autodesk.Revit.DB.View> { (Autodesk.Revit.DB.View)viewSection.InternalElement };

            Autodesk.Revit.DB.Element cropBoxElement = null;
            using (TransactionGroup tGroup = new TransactionGroup(doc))
            {
                TransactionManager.Instance.ForceCloseTransaction();
                tGroup.Start("Temp to find crop box element");
                using (Transaction t2 = new Transaction(doc, "Temp to find crop box element"))
                {
                    foreach (Autodesk.Revit.DB.View view in internalView)
                    {
                        // Deactivate crop box
                        t2.Start();
                        view.CropBoxVisible = false;
                        t2.Commit();
                        // Get all visible elements;
                        // this excludes hidden crop box
                        FilteredElementCollector collector = new FilteredElementCollector(doc, view.Id);
                        ICollection<ElementId> shownElems = collector.ToElementIds();
                        // Activate crop box
                        t2.Start();
                        view.CropBoxVisible = true;
                        t2.Commit();
                        // Get all visible elements excluding
                        // everything except the crop box
                        collector = new FilteredElementCollector(doc, view.Id);
                        collector.Excluding(shownElems);
                        cropBoxElement = collector.FirstElement();
                    }
                }
                tGroup.RollBack();
            }
            //create the override settings
            OverrideGraphicSettings gSettings = new OverrideGraphicSettings().SetProjectionLineWeight(lineWeight);
            foreach (Autodesk.Revit.DB.View view in internalView)
            {
                TransactionManager.Instance.EnsureInTransaction(doc);
                view.CropBoxVisible = true;
                view.SetElementOverrides(cropBoxElement.Id, gSettings);
                TransactionManager.Instance.TransactionTaskDone();
            }

            return viewSection;
        }


        /// <summary>
        /// This node will override the crop region of the given section view based on the pen weight provided. 
        /// This is the faster version that works with interior elevations.
        /// </summary>
        /// <param name="viewSection">The plan view to rotate</param>
        /// <param name="lineWeight">The lineweight to override to, (1-16)</param>
        /// <returns name="viewSection">The overidden view.</returns>
        /// <search>
        /// overridecrop
        /// </search>
        public static global::Revit.Elements.Element OverrideCropVersion2(global::Revit.Elements.Element viewSection, int lineWeight)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            Autodesk.Revit.DB.View internalView = (Autodesk.Revit.DB.View)viewSection.InternalElement;

            string stringId = internalView.Id.ToString();
            int idInt = Convert.ToInt32(stringId);

#if R20 || R21 || R22 || R23
            ElementId id = new ElementId(idInt - 1);
#endif

#if R24_OR_GREATER
            ElementId id = new ElementId(Convert.ToInt64(idInt-1));
#endif

            //create the override settings
            OverrideGraphicSettings gSettings = new OverrideGraphicSettings().SetProjectionLineWeight(lineWeight);

            TransactionManager.Instance.EnsureInTransaction(doc);
            internalView.CropBoxVisible = true;
            internalView.SetElementOverrides(id, gSettings);
            TransactionManager.Instance.TransactionTaskDone();

            return viewSection;
        }

        /// <summary>
        /// Retrieve the input view's origin, (if available).
        /// </summary>
        /// <param name="view">The view to get origin of.</param>
        /// <returns name="locationPoint">The origin of the view. Also, the origin of a plan view is not meaningful.</returns>
        /// <search>
        /// viewsection.locationpoint
        /// </search>
        public static List<Autodesk.DesignScript.Geometry.Point> LocationPoint(List<global::Revit.Elements.Element> view)
        {

            List<Autodesk.DesignScript.Geometry.Point> dynPoints = new List<Point>();

            foreach (global::Revit.Elements.Element v in view)
            {
                //the internal revit view
                Autodesk.Revit.DB.View internalView = (Autodesk.Revit.DB.View)v.InternalElement;

                // provide result based on conditions
                // Construct a point at the midpoint of the 
                // front bottom edge of the elev view cropbox
                double xmax = internalView.CropBox.Max.X;
                double xmin = internalView.CropBox.Min.X;
                double zmax = internalView.CropBox.Max.Z;
                XYZ pt = new XYZ(xmax - 0.5 * (xmax - xmin), 1.0, zmax);

                // Get pt's translation to 
                // project coordinate system
                pt = internalView.CropBox.Transform.OfPoint(pt);

                //global::Revit.Elements.Element rm= doc.GetRoomAtPoint(pt).ToDSType(true);
                dynPoints.Add(pt.ToPoint());
            }

            return dynPoints;
        }

        /// <summary>
        /// Creates a reference section.
        /// </summary>
        /// <param name="parentView"></param>
        /// <param name="viewToReference"></param>
        /// <param name="headPoint"></param>
        /// <param name="tailPoint"></param>
        public static void CreateReferenceSection(global::Revit.Elements.Element parentView,
            global::Revit.Elements.Element viewToReference, Point headPoint, Point tailPoint)
        {
            //the current document
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(doc);
            Autodesk.Revit.DB.ViewSection.CreateReferenceSection(doc,parentView.InternalElement.Id,viewToReference.InternalElement.Id,headPoint.ToRevitType(),tailPoint.ToRevitType());
            TransactionManager.Instance.TransactionTaskDone();
        }
    }
}


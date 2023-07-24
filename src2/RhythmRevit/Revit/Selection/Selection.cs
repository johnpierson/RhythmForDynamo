using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using Category = Revit.Elements.Category;
using Curve = Autodesk.Revit.DB.Curve;
using Element = Autodesk.Revit.DB.Element;
using Grid = Autodesk.Revit.DB.Grid;
using ModelCurve = Autodesk.Revit.DB.ModelCurve;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace Rhythm.Revit.Selection
{
    /// <summary>
    /// Wrapper class for selections.
    /// </summary>
    public class Selection
    {
        private Selection()
        {
        }

        /// <summary>
        /// This node will select grids along a model curve element ordered based on the start of the model curve.
        /// This works in the active view. So whatever plan representation your grids have, that is what is used.
        /// </summary>
        /// <param name="modelCurve">Revit model curve to select grids along.</param>
        /// <returns name="orderedGrids">The intersecting grids ordered from beginning to end of the line.</returns>
        public static List<global::Revit.Elements.Grid>IntersectingGridsByModelCurve(global::Revit.Elements.ModelCurve modelCurve)
        {
            ModelCurve mCurve = modelCurve.InternalElement as ModelCurve;

            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            List<global::Revit.Elements.Grid>intersectingGrids = new List<global::Revit.Elements.Grid>(); 

            IList<Autodesk.Revit.DB.Element> grids = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Grids).WhereElementIsNotElementType().ToElements();

            foreach (var grid in grids)
            {
                Grid g = grid as Grid;
                Curve c = g.GetCurvesInView(DatumExtentType.ViewSpecific,doc.ActiveView).First();
                Curve c2 = mCurve.GeometryCurve;

                Point pt1 = Point.ByCoordinates(0,0,c.GetEndPoint(0).Z);
                Point pt2 = Point.ByCoordinates(0, 0, c2.GetEndPoint(0).Z);
                XYZ vec = Vector.ByTwoPoints(pt2,pt1).ToRevitType();

                var transformed = c2.CreateTransformed(Transform.CreateTranslation(vec));

                SetComparisonResult test = c.Intersect(transformed);

                if (test == SetComparisonResult.Overlap ||
                    test == SetComparisonResult.Subset ||
                    test == SetComparisonResult.Superset ||
                    test == SetComparisonResult.Equal)
                {
                    intersectingGrids.Add(g.ToDSType(false) as global::Revit.Elements.Grid);
                }
                
            }

            return intersectingGrids.OrderBy(g => g.Curve.DistanceTo(modelCurve.Curve.StartPoint)).ToList();
        }
        /// <summary>
        /// Get Null
        /// </summary>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static object GetNull()
        {
            return null;
        }

        /// <summary>
        /// Sometimes a pick selection is nicer. 😁
        /// </summary>
        /// <param name="runIt">Allows you to tell the node to "run". Also allows you to refresh selection.</param>
        /// <param name="category">The category or categories to isolate to. (leave blank if you want to be able to pick anything)</param>
        /// <param name="singleSelection">Optional input for a single item selection. Default to multiple.</param>
        /// <param name="ordered">Force an ordered selection using esc to finish.</param>
        /// <returns name="pickedElements"></returns>
        public static object Pick(bool runIt, [DefaultArgument("Rhythm.Utilities.MiscUtils.GetNull()")] List<object> category, bool singleSelection = false, bool ordered = false)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var uiDoc = DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument;

            if (category is null)
            {
                if (singleSelection)
                {
                    var reference = uiDoc.Selection.PickObject(ObjectType.Element);
                    return doc.GetElement(reference.ElementId).ToDSType(false);
                }
                if (ordered)
                {
                    List<global::Revit.Elements.Element> orderedList = new List<global::Revit.Elements.Element>();
                    bool flag = true;
                    while (flag)
                    {
                        try
                        {
                            var reference = uiDoc.Selection.PickObject(ObjectType.Element,
                                "Pick elements in desired order and press ESC to finish selection");
                            orderedList.Add(doc.GetElement(reference.ElementId).ToDSType(false));
                        }
                        catch (Exception)
                        {
                            flag = false;
                        }
                    }

                    return orderedList;
                }
                var references = uiDoc.Selection.PickObjects(ObjectType.Element);
                return references.Select(r => doc.GetElement(r.ElementId).ToDSType(false)).ToList();
            }
            else
            {
                //clear the previous categories
                CategoryNames.Clear();
                foreach (var c in category)
                {
                    switch (c)
                    {
                        case Category cat:
                            CategoryNames.Add(cat.Name);
                            break;
                        case string catString:
                            CategoryNames.Add(catString);
                            break;
                    }
                }
                if (singleSelection)
                {
                    var reference = uiDoc.Selection.PickObject(ObjectType.Element, new CategorySelectionFilter());
                    return doc.GetElement(reference.ElementId).ToDSType(false);
                }

                if (ordered)
                {
                    List<global::Revit.Elements.Element> orderedList = new List<global::Revit.Elements.Element>();
                    bool flag = true;
                    while (flag)
                    {
                        try
                        {
                            var reference = uiDoc.Selection.PickObject(ObjectType.Element, new CategorySelectionFilter(),
                                "Pick elements in desired order and press ESC to finish selection");
                            orderedList.Add(doc.GetElement(reference.ElementId).ToDSType(false));
                        }
                        catch (Exception)
                        {
                            flag = false;
                        }
                    }

                    return orderedList;
                }
                var references = uiDoc.Selection.PickObjects(ObjectType.Element, new CategorySelectionFilter());
                return references.Select(r => doc.GetElement(r.ElementId).ToDSType(false)).ToList();
            }
        }

        internal static List<string> CategoryNames = new List<string>();
    }

    internal class CategorySelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return Selection.CategoryNames.Contains(elem.Category.Name);
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}

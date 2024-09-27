using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;
#if !R20
using static Autodesk.Revit.DB.SpecTypeId;
#endif
using Category = Revit.Elements.Category;
using Curve = Autodesk.Revit.DB.Curve;
using Element = Autodesk.Revit.DB.Element;
using FamilyType = Revit.Elements.FamilyType;
using GlobalParameter = Autodesk.Revit.DB.GlobalParameter;
using Grid = Autodesk.Revit.DB.Grid;
using ModelCurve = Autodesk.Revit.DB.ModelCurve;
using Point = Autodesk.DesignScript.Geometry.Point;
using Reference = Autodesk.Revit.DB.Reference;
using RevitLinkInstance = Autodesk.Revit.DB.RevitLinkInstance;
using Vector = Autodesk.DesignScript.Geometry.Vector;

namespace Rhythm.Revit.Selection
{
    /// <summary>
    /// Wrapper class for selections.
    /// </summary>
    public class Selection
    {
        private Selection() { }

        /// <summary>
        /// Select a room at the corresponding point in the corresponding phase.
        /// </summary>
        /// <param name="point">The point location</param>
        /// <param name="phase">Optional phase to search in. If empty, the last phase is used.</param>
        /// <returns name="room"></returns>
        public static global::Revit.Elements.Element RoomAtPoint(Point point, [DefaultArgument("Rhythm.Utilities.MiscUtils.GetNull()")] global::Revit.Elements.Element phase)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;

            var revitPoint = point.ToRevitType(true);

            Autodesk.Revit.DB.Architecture.Room room = null;

            if (phase is null)
            {
               room = doc.GetRoomAtPoint(revitPoint);
            }
            else
            {
                try
                {
                    var internalPhase = phase.InternalElement as Phase;
                    room = doc.GetRoomAtPoint(revitPoint, internalPhase);
                }
                catch (Exception)
                {
                    room = doc.GetRoomAtPoint(revitPoint);
                }
            }

            if (room is null)
            {
                throw new Exception(
                    "Could not find a room at the given point, in the given phase. Maybe try offsetting your point in the Z Axis a little?");
            }

            return room.ToDSType(true);
        }

        //public static void RoomsAtCurve(global::Revit.Elements.FamilyType famType)
        //{
        //    var doc = DocumentManager.Instance.CurrentDBDocument;
        //    var uiDoc = DocumentManager.Instance.CurrentUIDocument;

        //    var app = doc.Application;

        //    app.DocumentChanged
        //        += AppOnDocumentChanged;

        //    var internalfamType = famType.InternalElement as Autodesk.Revit.DB.FamilySymbol;


        //    PromptForFamilyInstancePlacementOptions opts = new PromptForFamilyInstancePlacementOptions();
        //    opts.SketchGalleryOptions = SketchGalleryOptions.SGO_Spline;

        //    uiDoc.PromptForFamilyInstancePlacement(internalfamType, opts);


        //}

        //private static void AppOnDocumentChanged(object sender, DocumentChangedEventArgs e)
        //{
        //    var newStuff = e.GetAddedElementIds();
        //}

        /// <summary>
        /// Select stuff from a link. Useful for Dynamo player.
        /// </summary>
        /// <param name="refreshSelection">Reset the selection and reselect new things</param>
        /// <param name="singleSelection">Enable single selection. False for multiple selection.</param>
        /// <returns name="selectedElements">The selected elements.</returns>
        /// <returns name="transform">If the link was moved this transform is needed to relocate the stuff.</returns>
        [MultiReturn(new[] { "selectedElements", "transform" })]
#pragma warning disable IDE0060 // Remove unused parameter
        public static Dictionary<string, object> FromLink(bool refreshSelection, bool singleSelection)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            UIDocument uiDoc = DocumentManager.Instance.CurrentUIDocument;

            Autodesk.Revit.UI.Selection.Selection sel = uiDoc.Selection;

            List<global::Revit.Elements.Element> selection = new List<global::Revit.Elements.Element>();


            if (singleSelection)
            {
                Reference reference = sel.PickObject(ObjectType.LinkedElement, "Please pick a model element from a link.");

                RevitLinkInstance link = DocumentManager.Instance.CurrentDBDocument.GetElement(reference) as RevitLinkInstance;
                var linkElement = link.GetLinkDocument().GetElement(reference.LinkedElementId);


                selection.Add(linkElement.ToDSType(false));
            }
            else
            {
                IList<Reference> references = sel.PickObjects(ObjectType.LinkedElement, "Please pick some model elements from a link.");
                foreach (var r in references)
                {
                    RevitLinkInstance link = DocumentManager.Instance.CurrentDBDocument.GetElement(r) as RevitLinkInstance;

                    var linkElement = link.GetLinkDocument().GetElement(r.LinkedElementId);

                    selection.Add(linkElement.ToDSType(false));
                }
            }

            var internalFirstElement = selection.First().InternalElement;

            var transform = ElementSelection.InLinkDoc(internalFirstElement.Document.Title, internalFirstElement.UniqueId, false) as CoordinateSystem;

            //returns the outputs
            var outInfo = new Dictionary<string, object>
            {
                { "selectedElements", selection},
                { "transform", transform}
            };
            return outInfo;
        }

        /// <summary>
        /// This node will select grids along a model curve element ordered based on the start of the model curve.
        /// This works in the active view. So whatever plan representation your grids have, that is what is used.
        /// </summary>
        /// <param name="modelCurve">Revit model curve to select grids along.</param>
        /// <returns name="orderedGrids">The intersecting grids ordered from beginning to end of the line.</returns>
        public static List<global::Revit.Elements.Grid> IntersectingGridsByModelCurve(global::Revit.Elements.ModelCurve modelCurve)
        {
            ModelCurve mCurve = modelCurve.InternalElement as ModelCurve;

            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            List<global::Revit.Elements.Grid> intersectingGrids = new List<global::Revit.Elements.Grid>();

            IList<Autodesk.Revit.DB.Element> grids = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Grids).WhereElementIsNotElementType().ToElements();

            foreach (var grid in grids)
            {
                Grid g = grid as Grid;
                Curve c = g.GetCurvesInView(DatumExtentType.ViewSpecific, doc.ActiveView).First();
                Curve c2 = mCurve.GeometryCurve;

                Point pt1 = Point.ByCoordinates(0, 0, c.GetEndPoint(0).Z);
                Point pt2 = Point.ByCoordinates(0, 0, c2.GetEndPoint(0).Z);
                XYZ vec = Vector.ByTwoPoints(pt2, pt1).ToRevitType();

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
#pragma warning disable IDE0060 // Remove unused parameter
        public static object Pick(bool runIt, [DefaultArgument("Rhythm.Utilities.MiscUtils.GetNull()")] List<object> category, bool singleSelection = false, bool ordered = false)
#pragma warning restore IDE0060 // Remove unused parameter
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

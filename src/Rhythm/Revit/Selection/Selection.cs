using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using Curve = Autodesk.Revit.DB.Curve;
using Grid = Autodesk.Revit.DB.Grid;
using ModelCurve = Autodesk.Revit.DB.ModelCurve;
using Point = Autodesk.DesignScript.Geometry.Point;
using ReferencePlane = Autodesk.Revit.DB.ReferencePlane;

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
                    intersectingGrids.Add(g.ToDSType(true) as global::Revit.Elements.Grid);
                }
                
            }

            return intersectingGrids.OrderBy(g => g.Curve.DistanceTo(modelCurve.Curve.StartPoint)).ToList();

        }
    }
}

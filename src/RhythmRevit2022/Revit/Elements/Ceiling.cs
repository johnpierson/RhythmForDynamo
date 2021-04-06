using Autodesk.Revit.DB;
using DynamoServices;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using System.Collections.Generic;
using System.Linq;
using DynamoUnits;
using Curve = Autodesk.DesignScript.Geometry.Curve;
using GlobalParameter = Autodesk.Revit.DB.GlobalParameter;

namespace Rhythm.Revit.Elements
{
    [RegisterForTrace]
    public class Ceiling
    {
        private static bool VerifyEdit(Autodesk.Revit.DB.Ceiling ceilingElem, List<List<Curve>> curves)
        {
            if (ceilingElem == null)
            {
                return false;
            }

            var ceilingParam = ceilingElem.get_Parameter(BuiltInParameter.HOST_PERIMETER_COMPUTED).AsDouble();
            double length = 0;
            foreach (var curveList in curves)
            {
                foreach (var curve in curveList)
                {
                    length = length +
                             curve.ToRevitType(true).Length;
                }
            }

            return length.AlmostEquals(ceilingParam, 0.01);
        }

        public static global::Revit.Elements.Element ByCurveLoops(List<List<Curve>> curves, global::Revit.Elements.Element ceilingType,
            global::Revit.Elements.Level level)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(doc);

            var ceilingElem =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.Ceiling>(doc);
            bool successfullyUsedExistingCeiling = false;

            //check if we need to update
            if (VerifyEdit(ceilingElem,curves))
            {
                return ceilingElem.ToDSType(true);
            }

            var ceilingTypeId = ceilingType.InternalElement.Id;
            var levelId = level.InternalElement.Id;

            //generate our curve loops
            var curveLoops = new List<CurveLoop>();
            foreach (var curveList in curves)
                curveLoops.Add(CurveLoop.Create(curveList.Select(c => c.ToRevitType(true)).ToList()));

            if (ceilingElem != null)
            {
                Sketch sketch = doc.GetElement(ceilingElem.SketchId) as Sketch;

                TransactionManager.Instance.ForceCloseTransaction();
                SketchEditScope sketchEditScope = new SketchEditScope(doc, "Editing existing sketch");
                sketchEditScope.Start(ceilingElem.SketchId);

                using (Transaction transaction = new Transaction(doc, "Modify sketch"))
                {
                    transaction.Start();
                    foreach (CurveArray curveArray in sketch.Profile)
                    {
                        foreach (Autodesk.Revit.DB.Curve curve in curveArray)
                        {
                            doc.Delete(curve.Reference.ElementId);
                        }
                    }

                    foreach (var curveList in curves)
                    {
                        foreach (var c in curveList)
                        {
                            doc.Create.NewModelCurve(c.ToRevitType(true), sketch.SketchPlane);
                        }
                    }
                    transaction.Commit();
                }
                sketchEditScope.Commit(new FailuresPreprocessor());

                successfullyUsedExistingCeiling = true;
            }

            var ceiling = successfullyUsedExistingCeiling ? ceilingElem : Autodesk.Revit.DB.Ceiling.Create(doc, curveLoops, ceilingTypeId, levelId);

            TransactionManager.Instance.TransactionTaskDone();

            // delete the element stored in trace and add this new one
            ElementBinder.CleanupAndSetElementForTrace(doc, ceiling);

            return ceiling.ToDSType(true);
        }

        public static global::Revit.Elements.Element DefaultCeilingType()
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;

            return new FilteredElementCollector(doc).OfClass(typeof(CeilingType)).WhereElementIsElementType()
                .FirstElement().ToDSType(true);
        }

        public class FailuresPreprocessor : IFailuresPreprocessor
        {
            public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
            {
                return FailureProcessingResult.Continue;
            }
        }
    }
}
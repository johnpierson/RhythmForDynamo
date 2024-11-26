using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using DynamoServices;
using DynamoUnits;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Curve = Autodesk.DesignScript.Geometry.Curve;
using Level = Revit.Elements.Level;
// ReSharper disable UnusedMember.Local

namespace Rhythm.Revit.Elements
{
#if R22_OR_GREATER
    /// <summary>
    /// Wrapper class for floors
    /// </summary>
    [RegisterForTrace]
    public class Ceiling
    {
        private Ceiling() { }

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
                    length += curve.ToRevitType(true).Length;
                }
            }

            return length.AlmostEquals(ceilingParam, 0.01);
        }
        /// <summary>
        /// Collect the first ceiling type available. Revit 2022+
        /// </summary>
        /// <returns name="ceilingType">The first (default) ceiling type.</returns>
        [NodeCategory("Query")]
        public static global::Revit.Elements.Element DefaultCeilingType()
        {

            var doc = DocumentManager.Instance.CurrentDBDocument;

            return new FilteredElementCollector(doc).OfClass(typeof(Autodesk.Revit.DB.CeilingType)).WhereElementIsElementType()
                .FirstElement().ToDSType(false) as global::Revit.Elements.Element;
        }


        /// <summary>
        /// Create a ceiling by multiple curve loops. Revit 2022+
        /// </summary>
        /// <param name="curves">The input curves as a list of lists.</param>
        /// <param name="ceilingType">Ceiling type to use.</param>
        /// <param name="level">The level to host on.</param>
        /// <returns name="ceiling">The newly created ceiling.</returns>
        [NodeCategory("Actions")]
        public static global::Revit.Elements.Element ByCurveLoops(List<List<Curve>> curves, global::Revit.Elements.Element ceilingType, Level level)
        {
            string versionNumber = DocumentManager.Instance.CurrentUIApplication.Application.VersionNumber;

            var doc = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(doc);

            var ceilingElem =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.Ceiling>(doc);
            bool successfullyUsedExistingCeiling = false;

            //check if we need to update
            if (VerifyEdit(ceilingElem, curves))
            {
                return ceilingElem.ToDSType(false);
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

            return ceiling.ToDSType(false);
        }
        /// <summary>
        /// Returns ceiling grid lines, with the option to return the boundary as well.
        /// </summary>
        /// <param name="ceiling">The ceiling to extract grids from</param>
        /// <param name="includeBoundary">Extract the boundary?</param>
        /// <returns name="ceilingGrids"></returns>
        public static List<Curve> GetGridLines(global::Revit.Elements.Element ceiling, bool includeBoundary = false)
        {
            Autodesk.Revit.DB.Ceiling internalCeiling = ceiling.InternalElement as Autodesk.Revit.DB.Ceiling;

            var curveList = internalCeiling.GetCeilingGridLines(includeBoundary);

            return curveList.Select(c => c.ToProtoType(true)).ToList();
        }

        internal class FailuresPreprocessor : IFailuresPreprocessor
        {
            public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
            {
                return FailureProcessingResult.Continue;
            }
        }
    }
#endif
}

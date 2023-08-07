using System;
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
using FloorType = Revit.Elements.FloorType;
using Level = Revit.Elements.Level;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for floors
    /// </summary>
    [RegisterForTrace]
    public class Floor
    {
        private Floor() { }

        private static bool VerifyEdit(Autodesk.Revit.DB.Floor floorElem, List<List<Curve>> curves)
        {
            if (floorElem == null)
            {
                return false;
            }

            var ceilingParam = floorElem.get_Parameter(BuiltInParameter.HOST_PERIMETER_COMPUTED).AsDouble();
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

        /// <summary>
        /// Collect the first floor type available. Revit 2022+
        /// </summary>
        /// <returns name="floorType">The first (default) floor type.</returns>
        [NodeCategory("Query")]
        public static global::Revit.Elements.FloorType DefaultFloorType()
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;

            var floorTypeInternal = doc.GetElement(Autodesk.Revit.DB.Floor.GetDefaultFloorType(doc, false));

            return (global::Revit.Elements.FloorType)floorTypeInternal.ToDSType(false) as FloorType;
        }

        /// <summary>
        /// Create a floor with multiple loops. Revit 2022+
        /// </summary>
        /// <param name="curves">The input curves as a list of lists.</param>
        /// <param name="floorType">Floor type to use.</param>
        /// <param name="level">The level to host on.</param>
        /// <returns name="floor">The new floor.</returns>
        /// <exception cref="Exception"></exception>
        [NodeCategory("Actions")]
        public static global::Revit.Elements.Floor ByCurveLoops(List<List<Curve>> curves, FloorType floorType, Level level)
        {

            //the current document and our data
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var floorTypeId = floorType.InternalElement.Id;
            var levelId = level.InternalElement.Id;

            var floorElem =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.Floor>(doc);
            var successfullyUsedExistingFloor = false;

            //check if we need to update
            if (VerifyEdit(floorElem, curves))
            {
                return floorElem.ToDSType(false) as global::Revit.Elements.Floor;
            }

            //generate our curve loops
            var curveLoops = new List<CurveLoop>();
            foreach (var curveList in curves)
                curveLoops.Add(CurveLoop.Create(curveList.Select(c => c.ToRevitType()).ToList()));

            if (floorElem != null)
            {
                var sketch = doc.GetElement(floorElem.SketchId) as Sketch;

                TransactionManager.Instance.ForceCloseTransaction();
                var sketchEditScope = new SketchEditScope(doc, "Editing existing sketch");
                sketchEditScope.Start(floorElem.SketchId);

                using (var transaction = new Transaction(doc, "Modify sketch"))
                {
                    transaction.Start();
                    foreach (CurveArray curveArray in sketch.Profile)
                        foreach (Autodesk.Revit.DB.Curve curve in curveArray)
                            doc.Delete(curve.Reference.ElementId);

                    foreach (var curveList in curves)
                        foreach (var c in curveList)
                            doc.Create.NewModelCurve(c.ToRevitType(), sketch.SketchPlane);
                    transaction.Commit();
                }

                sketchEditScope.Commit(new FailuresPreprocessor());
                successfullyUsedExistingFloor = true;
            }

            TransactionManager.Instance.EnsureInTransaction(doc);
            var floor = successfullyUsedExistingFloor
                ? floorElem
                : Autodesk.Revit.DB.Floor.Create(doc, curveLoops, floorTypeId, levelId);
            TransactionManager.Instance.TransactionTaskDone();

            // delete the element stored in trace and add this new one
            ElementBinder.CleanupAndSetElementForTrace(doc, floor);

            return (global::Revit.Elements.Floor)floor.ToDSType(false);
        }



        internal class FailuresPreprocessor : IFailuresPreprocessor
        {
            public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
            {
                return FailureProcessingResult.Continue;
            }
        }
    }
}

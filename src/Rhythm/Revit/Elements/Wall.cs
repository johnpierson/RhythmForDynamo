using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using RevitServices.Persistence;
using Revit.Elements;
using RevitServices.Transactions;
using Element = Autodesk.Revit.DB.Element;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for walls.
    /// </summary>
    public class Walls
    {
        private Walls()
        { }

        /// <summary>
        /// This node will try to check if the walls profile has been modified using the dependent elements method available in Revit 2018.1+
        /// </summary>
        /// <param name="wall">The walls to check.</param>
        /// <returns name="bool">The result.</returns>
        /// <search>
        /// profile, wall
        /// </search>
        public static bool HasEditedProfile(global::Revit.Elements.Element wall)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.Wall internalWall = (Autodesk.Revit.DB.Wall )wall.InternalElement;

            //dependent elements method (available in Revit 2018.1 +)
            Autodesk.Revit.DB.ElementFilter elemFilter = new ElementIsElementTypeFilter(true);
            IList<ElementId> elemIds = internalWall.GetDependentElements(elemFilter);

            //get the elements
            List<Autodesk.Revit.DB.Element> elems = new List<Element>(elemIds.Select(e => doc.GetElement(e)));

            //find out if any of the elements are of sketch type
            var sketchElems = elems.Where(e => e is Autodesk.Revit.DB.Sketch || e is Autodesk.Revit.DB.SketchPlane).ToList();

            return sketchElems.Count > 1;
        }
        /// <summary>
        /// This node will try to check if the walls profile has been modified using the dependent elements method available in Revit 2018.1+
        /// </summary>
        /// <param name="wall">The walls to check.</param>
        /// <returns name="modelCurves">The result.</returns>
        /// <search>
        /// profile, wall
        /// </search>
        public static global::Revit.Elements.Element[] EditedProfile(global::Revit.Elements.Element wall)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.Element internalWall = wall.InternalElement;

            //dependent elements method (available in Revit 2018.1 +)
            Autodesk.Revit.DB.ElementFilter elemFilter = new ElementIsElementTypeFilter(true);
            IList<ElementId> elemIds = internalWall.GetDependentElements(elemFilter);

            //get the elements
            List<Autodesk.Revit.DB.Element> elems = new List<Element>(elemIds.Select(e => doc.GetElement(e)));

            global::Autodesk.Revit.DB.Element[] internalCurves = elems.Where(e => e is Autodesk.Revit.DB.ModelCurve).ToArray();

            global::Revit.Elements.Element[] modelCurves = internalCurves.Select(e => e.ToDSType(true)).ToArray();

            return modelCurves;
        }
    }
}

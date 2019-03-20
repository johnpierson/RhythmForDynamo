using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using RevitServices.Persistence;
using Revit.Elements;
using RevitServices.Transactions;

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
        /// This node will try to check if the walls profile has been modified using logic outlined here,
        /// http://thebuildingcoder.typepad.com/blog/2010/11/access-to-sketch-and-sketch-plane.html
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

            Transaction trans = new Transaction(doc, "check wall");
            TransactionManager.Instance.ForceCloseTransaction();
            trans.Start();
            ICollection<ElementId> ids = doc.Delete(internalWall.Id);
            trans.RollBack();

            List<global::Autodesk.Revit.DB.Element> a = new List<global::Autodesk.Revit.DB.Element>(
                ids.Select(id => doc.GetElement(id)));

            a.Where(e => e is Autodesk.Revit.DB.Sketch || e is Autodesk.Revit.DB.SketchPlane).ToArray();

            bool result = a.Count > 1;

            return result;
        }
        /// <summary>
        /// This node will try to check if the walls profile has been modified using logic outlined here,
        /// http://thebuildingcoder.typepad.com/blog/2010/11/access-to-sketch-and-sketch-plane.html
        /// </summary>
        /// <param name="wall">The walls to check.</param>
        /// <returns name="modelCurves">The result.</returns>
        /// <search>
        /// profile, wall
        /// </search>
        public static global::Revit.Elements.Element[] EditedProfile(global::Revit.Elements.Element wall)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.Element internalElement = wall.InternalElement;

            Transaction trans = new Transaction(doc, "check element");
            TransactionManager.Instance.ForceCloseTransaction();
            trans.Start();
            ICollection<ElementId> ids = doc.Delete(internalElement.Id);
            trans.RollBack();

            List<global::Autodesk.Revit.DB.Element> internalWallElements = new List<global::Autodesk.Revit.DB.Element>(
                ids.Select(id => doc.GetElement(id)));

            global::Autodesk.Revit.DB.Element[] internalCurves = internalWallElements.Where(e => e is Autodesk.Revit.DB.ModelCurve).ToArray();

            global::Revit.Elements.Element[] modelCurves = internalCurves.Select(e => e.ToDSType(true)).ToArray();

            return modelCurves;
        }
    }
}

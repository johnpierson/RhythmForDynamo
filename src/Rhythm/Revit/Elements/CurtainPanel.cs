using System.Collections.Generic;
using Autodesk.Revit.DB;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for curtain panels.
    /// </summary>
    public class CurtainPanels
    {
        private CurtainPanels()
        { }
        /// <summary>
        /// This node will isolate the given curtain wall panels in the active view. 
        /// </summary>
        /// <param name="curtainPanels">The curtain panels to isolate.</param>
        /// <returns name="curtainPanels">The isolated curtain panels.</returns>
        /// <search>
        /// copy
        /// </search>
        public static List<global::Revit.Elements.Element> IsolateInView(List<global::Revit.Elements.Element> curtainPanels)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            List<Autodesk.Revit.DB.ElementId> curtainPanelIds = new List<ElementId>();
            foreach (global::Revit.Elements.Element panel in curtainPanels)
            {
                curtainPanelIds.Add(panel.InternalElement.Id);
            }
        
            Autodesk.Revit.DB.Category curtainPanelCat =
                Autodesk.Revit.DB.Category.GetCategory(doc,BuiltInCategory.OST_CurtainWallPanels);
            Autodesk.Revit.DB.View activeView = doc.ActiveView;
            TransactionManager.Instance.EnsureInTransaction(doc);
            activeView.IsolateCategoryTemporary(curtainPanelCat.Id);
            //activeView.IsolateElementsTemporary(curtainPanelIds);
            TransactionManager.Instance.TransactionTaskDone();

            TransactionManager.Instance.ForceCloseTransaction();
            TransactionManager.Instance.EnsureInTransaction(doc);
            activeView.IsolateElementsTemporary(curtainPanelIds);
            TransactionManager.Instance.TransactionTaskDone();

            return curtainPanels;
        }
    }
}

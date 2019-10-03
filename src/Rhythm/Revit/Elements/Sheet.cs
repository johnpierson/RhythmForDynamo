using Autodesk.DesignScript.Runtime;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using RevitServices.Persistence;
using Revit.Elements;
using RevitServices.Transactions;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for sheets.
    /// </summary>
    public class Sheet
    {
        private Sheet()
        {
        }

        /// <summary>
        /// This node will obtain viewports, views and schedules from a given sheet.
        /// </summary>
        /// <param name="sheet">The sheet to get viewports from.</param>
        /// <param name="doc">The document to use. If left blank the current document will be used.</param>
        /// <returns name="viewports">The viewports on the sheet.</returns>
        /// <returns name="views">The views on the sheet.</returns>
        /// <returns name="schedules">The schedules on the sheet.</returns>
        /// <search>
        /// viewport, schedules,rhythm
        /// </search>
        //this is the node sheet.viewports
        [MultiReturn(new[] { "viewports","views","schedules"})]
        public static Dictionary<string, object> GetViewportsAndViews(global::Revit.Elements.Views.Sheet sheet, [DefaultArgument("Rhythm.Revit.Elements.Sheet.CurrentDocument()")] Autodesk.Revit.DB.Document doc)
        {
            Autodesk.Revit.DB.ViewSheet viewSheet = (Autodesk.Revit.DB.ViewSheet) sheet.InternalElement;
            //obtains the viewports from the given sheet
            var viewportIds = viewSheet.GetAllViewports();
            List<global::Revit.Elements.Element> viewports = new List<global::Revit.Elements.Element>(viewportIds.Select(id => doc.GetElement(id).ToDSType(true)).ToArray());
            //obtains the views from the given sheet
            List<global::Revit.Elements.Element> views = new List<global::Revit.Elements.Element>(viewports.Select(v => doc.GetElement(((Autodesk.Revit.DB.Viewport)v.InternalElement).ViewId).ToDSType(true)).ToArray());
            //obtains the schedules from the sheet
            FilteredElementCollector scheduleCollector =
                new FilteredElementCollector(doc, viewSheet.Id).OfCategory(BuiltInCategory.OST_ScheduleGraphics);
            var schedulesInternal = scheduleCollector.ToElements();
            List<global::Revit.Elements.Element> schedules = new List<global::Revit.Elements.Element>(schedulesInternal.Select(s => s.ToDSType(true)).ToArray());

            //returns the outputs7
            var outInfo = new Dictionary<string, object>
                {
                    { "viewports",viewports },
                    { "views",views },
                    { "schedules",schedules }
                };
            return outInfo;
        }
        [IsVisibleInDynamoLibrary(false)]
        public static Autodesk.Revit.DB.Document CurrentDocument()
        {
            return DocumentManager.Instance.CurrentDBDocument;
        }

        /// <summary>
        /// Creates a new sheet.
        /// </summary>
        /// <param name="titleblock">The titleblock to use.</param>
        /// <returns name="Sheet">The newly created sheet.</returns>
        /// <search>
        /// sheet,rhythm
        /// </search>
        public static global::Revit.Elements.Element Create(global::Revit.Elements.FamilyType titleblock)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            TransactionManager.Instance.EnsureInTransaction(doc);
            ElementId titleblockId = new ElementId(titleblock.Id);
            ViewSheet createdSheet = ViewSheet.Create(doc, titleblockId);
            var result = createdSheet.ToDSType(true);
            TransactionManager.Instance.TransactionTaskDone();

            return result;
        }
        /// <summary>
        /// This node will grab the titleblock from the given sheet.
        /// </summary>
        /// <param name="viewSheet">The sheet to get titleblock from.</param>
        /// <returns name="titleblock">The sheet's titleblock.</returns>
        /// <search>
        /// sheet, sheets, titleblock
        /// </search>
        public static List<global::Revit.Elements.Element> Titleblock(global::Revit.Elements.Views.Sheet viewSheet)
        {
            var viewId = viewSheet.InternalElement.Id;
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            FilteredElementCollector elements = new FilteredElementCollector(doc, viewId).OfCategory(BuiltInCategory.OST_TitleBlocks);
            //grabs the elements from the collection
            var elementCollection = elements.ToElements();
            //generates a list to convert to a usable type in Dynamo
            List<global::Revit.Elements.Element> ttblList = new List<global::Revit.Elements.Element>(elementCollection.Select(e => e.ToDSType(true)).ToArray());
 
            return ttblList;
        }

    }
}
 
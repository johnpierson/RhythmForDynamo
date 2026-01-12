using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using RevitServices.Persistence;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for sheet collections.
    /// </summary>
    public class SheetCollection
    {
        private SheetCollection()
        {
        }

        /// <summary>
        /// Gets all sheets in the specified sheet collection.
        /// </summary>
        /// <param name="sheetCollection">The sheet collection to get sheets from.</param>
        /// <returns name="Sheets">The sheets in the collection.</returns>
        /// <search>
        /// sheet collection, sheets, rhythm
        /// </search>
        [NodeCategory("Query")]
        public static IEnumerable<global::Revit.Elements.Element> Sheets(global::Revit.Elements.Element sheetCollection)
        {
            if (sheetCollection == null)
            {
                return new List<global::Revit.Elements.Element>();
            }

#if !R25_OR_GREATER
            // SheetCollection class is only available in R25+
            return new List<global::Revit.Elements.Element>();
#else
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var internalCollection = sheetCollection.InternalElement as Autodesk.Revit.DB.SheetCollection;
            
            if (internalCollection == null)
            {
                return new List<global::Revit.Elements.Element>();
            }

            var collectionId = internalCollection.Id;

            // Get all sheets in the document
            var collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(ViewSheet));
            var allSheets = collector.ToElements().Cast<ViewSheet>();

            // Filter sheets that belong to this collection
            var sheetsInCollection = allSheets
                .Where(sheet => sheet.SheetCollectionId == collectionId)
                .OrderBy(sheet => sheet.get_Parameter(BuiltInParameter.SHEET_NUMBER)?.AsString() ?? "")
                .ThenBy(sheet => sheet.Name)
                .Select(sheet => sheet.ToDSType(false))
                .ToList();

            return sheetsInCollection;
#endif
        }
    }
}

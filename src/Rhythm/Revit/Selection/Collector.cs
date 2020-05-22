using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using Rhythm.Utilities;

namespace Rhythm.Revit.Selection
{
    /// <summary>
    /// Wrapper class for collectors.
    /// </summary>
    public class Collector
    {
        private Collector()
        { }
        /// <summary>
        /// This node will collect all elements of type from given document.
        /// </summary>
        /// <param name="document">The document to collect from. Preferably the document title obtained from Applications.OpenDocumentFile.</param>
        /// <param name="elementType">The element type to collect.</param>
        /// <returns name="element">The elements.</returns>
        /// <search>
        /// All Elements of Type in Document
        /// </search>
        [NodeCategory("Actions")]
        public static List<global::Revit.Elements.Element> ElementsOfTypeInDocument(object document, Type elementType )
        {
            Document doc = null;
            //this enables cross-compatibility with orchid documents by converting them to built in Autodesk.Revit.DB.Documents
            if (document.GetType().ToString().Contains("Orchid"))
            {
                doc = DocumentUtils.OrchidDocumentToDbDocument(document);
            }
            else
            {
                doc = document as Document;
            }

            FilteredElementCollector coll = new FilteredElementCollector(doc);
            List<global::Revit.Elements.Element> elems =
                new List<global::Revit.Elements.Element>(coll.OfClass(elementType).ToElements()
                    .Select(e => e.ToDSType(true)));

            return elems;
        }
        /// <summary>
        /// This node will collect all elements of the given category from given document.
        /// </summary>
        /// <param name="document">The document to collect from.</param>
        /// <param name="category">The category to collect.</param>
        /// <returns name="element">The elements.</returns>
        /// <search>
        /// All Elements of Category in Document
        /// </search>
        [NodeCategory("Actions")]
        public static List<global::Revit.Elements.Element> ElementsOfCategoryInDocument(object document, global::Revit.Elements.Category category)
        {
            Document doc = null;
            //this enables cross-compatibility with orchid documents by converting them to built in Autodesk.Revit.DB.Documents
            if (document.GetType().ToString().Contains("Orchid"))
            {
                doc = DocumentUtils.OrchidDocumentToDbDocument(document);
            }
            else
            {
                doc = document as Document;
            }

            //generate the category id from the input user viewable category
            Autodesk.Revit.DB.ElementId categoryId = new ElementId(category.Id);

            FilteredElementCollector coll = new FilteredElementCollector(doc);
            List<global::Revit.Elements.Element> elems =
                new List<global::Revit.Elements.Element>(coll.OfCategoryId(categoryId).ToElements()
                    .Select(e => e.ToDSType(true)));

            return elems;
        }
    }
}

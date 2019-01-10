using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Revit.Elements;

namespace Rhythm.Revit.Selection
{
    /// <summary>
    /// Wrapper class for textnotes.
    /// </summary>
    public class Collector
    {
        private Collector()
        { }
        /// <summary>
        /// This node will collect all elements of type from given document.
        /// </summary>
        /// <param name="document">The document to collect from.</param>
        /// <param name="elementType">The element type to collect.</param>
        /// <returns name="element">The elements.</returns>
        /// <search>
        /// All Elements of Type in Document
        /// </search>
        public static List<global::Revit.Elements.Element> ElementsOfTypeInDocument(Autodesk.Revit.DB.Document document, Type elementType )
        {
            FilteredElementCollector coll = new FilteredElementCollector(document);
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
        public static List<global::Revit.Elements.Element> ElementsOfCategoryInDocument(Autodesk.Revit.DB.Document document, global::Revit.Elements.Category category)
        {
            //generate the category id from the input user viewable category
            Autodesk.Revit.DB.ElementId categoryId = new ElementId(category.Id);

            FilteredElementCollector coll = new FilteredElementCollector(document);
            List<global::Revit.Elements.Element> elems =
                new List<global::Revit.Elements.Element>(coll.OfCategoryId(categoryId).ToElements()
                    .Select(e => e.ToDSType(true)));

            return elems;
        }
    }
}

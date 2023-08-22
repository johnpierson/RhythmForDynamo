using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using RevitServices.Persistence;
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
                throw new Exception(
                    "Orchid has recently been updated to return Revit.Application.Document, please update your Orchid to use this node.");
            }

            if (document is global::Revit.Application.Document dynamoDoc)
            {
                doc = dynamoDoc.ToRevitType();
            }
            else
            {
                doc = document as Document;
            }

            FilteredElementCollector coll = new FilteredElementCollector(doc);
            List<global::Revit.Elements.Element> elems =
                new List<global::Revit.Elements.Element>(coll.OfClass(elementType).ToElements()
                    .Select(e => e.ToDSType(false)));

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
                throw new Exception(
                    "Orchid has recently been updated to return Revit.Application.Document, please update your Orchid to use this node.");
            }
            if (document is global::Revit.Application.Document dynamoDoc)
            {
                doc = dynamoDoc.ToRevitType();
            }
            else
            {
                doc = document as Document;
            }

            var filter = new ElementCategoryFilter(category.ToRevitType().Id);

            FilteredElementCollector coll = new FilteredElementCollector(doc);
            List<global::Revit.Elements.Element> elems =
                new List<global::Revit.Elements.Element>(coll.WhereElementIsNotElementType().WherePasses(filter).ToElements()
                    .Select(e => e.ToDSType(false)));

            return elems;
        }

        /// <summary>
        /// Collect a detail or model group by a given name in the current model.
        /// </summary>
        /// <param name="name">The name of the group to find.</param>
        /// <returns name="groupInstance">The group instance(s) found in the current model.</returns>
        /// <search>
        /// Collector.ModelGroupByName
        /// </search>
        [NodeCategory("Actions")]
        public static List<global::Revit.Elements.Element> GroupByName(string name)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            List<BuiltInCategory> categories = new List<BuiltInCategory>
            {
                BuiltInCategory.OST_IOSModelGroups,
                BuiltInCategory.OST_IOSDetailGroups
            };

            ElementMulticategoryFilter filter = new ElementMulticategoryFilter(categories);

            var groups = new FilteredElementCollector(doc).WherePasses(filter).WhereElementIsNotElementType()
                .Where(g => g.Name.Equals(name)).ToList();

            return groups.Select(g => g.ToDSType(false)).ToList();
        }
    }
}

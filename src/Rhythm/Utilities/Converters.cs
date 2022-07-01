/*
 These converters were made possible thanks to @erfajo.
Link to the provided converters here: https://forum.dynamobim.com/t/the-fusion-post-for-coders/78033
 */
using System.Linq;
using System.Reflection;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Revit.Elements;
using RevitServices.Persistence;
using dynCategory = Revit.Elements.Category;
using dynDocument = Revit.Application.Document;
using dynElement = Revit.Elements.Element;
using dynElementSelector = Revit.Elements.ElementSelector;
using dynFamilyParameter = Revit.Elements.FamilyParameter;
using dynParameter = Revit.Elements.Parameter;
using rvtCategory = Autodesk.Revit.DB.Category;
using rvtDocument = Autodesk.Revit.DB.Document;
using rvtElement = Autodesk.Revit.DB.Element;
using rvtFamilyParameter = Autodesk.Revit.DB.FamilyParameter;
using rvtParameter = Autodesk.Revit.DB.Parameter;

namespace Rhythm.Utilities
{
    [IsVisibleInDynamoLibrary(false)]
    internal static class Convert
    {
        #region Document converters
        internal static dynDocument ToDynamoType(this rvtDocument item)
        {
            var constructor = typeof(dynDocument).GetConstructors(
                BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault();

            return constructor.Invoke(new object[] { item }) as dynDocument;
        }

        internal static rvtDocument ToRevitType(this dynDocument item)
        {
            var property = typeof(dynDocument).GetProperty("InternalDocument",
                BindingFlags.NonPublic | BindingFlags.Instance);

            return property.GetValue(item) as rvtDocument;
        }
        #endregion Document converters


        #region Category converters
        internal static dynCategory ToDynamoType(this rvtCategory item)
        {
            return dynCategory.ById(item.Id.IntegerValue);
        }

        internal static rvtCategory ToRevitType(this dynCategory item)
        {
            var document = DocumentManager.Instance.CurrentDBDocument;
            var id = new ElementId(item.Id);
            return rvtCategory.GetCategory(document, id);
        }

        internal static rvtCategory ToRevitType(
            this dynCategory item, rvtDocument document)
        {
            var id = new ElementId(item.Id);
            return rvtCategory.GetCategory(document, id);
        }
        #endregion Category converters


        #region Parameter converters
        internal static dynParameter ToDynamoType(this rvtParameter item)
        {
            var constructor = typeof(dynParameter).GetConstructors(
                BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault();

            return constructor.Invoke(new object[] { item }) as dynParameter;
        }

        internal static dynFamilyParameter ToDynamoType(this rvtFamilyParameter item)
        {
            var constructor = typeof(dynFamilyParameter).GetConstructors(
                BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault();

            return constructor.Invoke(new object[] { item }) as dynFamilyParameter;
        }

        internal static rvtParameter ToRevitType(this dynParameter item)
        {
            var property = typeof(dynParameter)
                .GetProperty("InternalParameter",
                BindingFlags.NonPublic | BindingFlags.Instance);

            return property.GetValue(item) as rvtParameter;
        }

        internal static rvtFamilyParameter ToRevitType(this dynFamilyParameter item)
        {
            var property = typeof(dynFamilyParameter)
                .GetProperty("InternalFamilyParameter",
                BindingFlags.NonPublic | BindingFlags.Instance);

            return property.GetValue(item) as rvtFamilyParameter;
        }
        #endregion Parameter converters


        #region Element converters
        internal static dynElement ToDynamoType(this ElementId item)
        {
            return dynElementSelector.ByElementId(item.IntegerValue);
        }

        internal static dynElement ToDynamoType(this int item)
        {
            return dynElementSelector.ByElementId(item);
        }

        internal static dynElement ToDynamoType(this rvtElement item)
        {
            return item.ToDSType(true);
        }

        internal static rvtElement ToRevitType(this dynElement item)
        {
            return item.InternalElement;
        }

        internal static rvtElement ToRevitType(this ElementId item)
        {
            var document = DocumentManager.Instance.CurrentDBDocument;
            return document.GetElement(item);
        }

        internal static rvtElement ToRevitType(this ElementId item, rvtDocument document)
        {
            return document.GetElement(item);
        }
        #endregion Element converters

    }
}
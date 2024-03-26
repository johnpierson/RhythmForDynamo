/*
 These converters were made possible thanks to @erfajo.
Link to the provided converters here: https://forum.dynamobim.com/t/the-fusion-post-for-coders/78033
 */
using System.Linq;
using System.Reflection;
using Autodesk.DesignScript.Runtime;
using dynCategory = Revit.Elements.Category;
using dynDocument = Revit.Application.Document;
using dynElement = Revit.Elements.Element;
using dynElementSelector = Revit.Elements.ElementSelector;

//using dynFamilyParameter = Revit.Elements.FamilyParameter;

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
#if R20 || R21 || R22 || R23
            return dynCategory.ById(item.Id.IntegerValue);
#endif

#if R24 || R25
            return dynCategory.ById(item.Id.Value);
#endif
        }

        internal static rvtCategory ToRevitType(this dynCategory item)
        {
            var internalCat = typeof(dynCategory).GetProperty("InternalCategory", BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance).GetValue(item) as rvtCategory;

            return internalCat;
        }
        #endregion Category converters
    }
}
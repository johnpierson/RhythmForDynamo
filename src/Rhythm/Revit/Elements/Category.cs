using Autodesk.DesignScript.Runtime;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using RevitServices.Persistence;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for categories.
    /// </summary>
    public class Categories
    {
        private Categories()
        {
        }
        /// <summary>
        /// Get the category projection lineweight.
        /// </summary>
        /// <returns name="categoryProjectionLineweight">The category projection lineweights.</returns>
        /// <search>
        /// Categories.ProjectionLineweight
        /// </search>
        public static List<int?> ProjectionLineweight(List<global::Revit.Elements.Category> category)
        {
            //obtains the current document for later use
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //category stuff - list to append the results to
            List<int?> categoryProjectionLineweight = new List<int?>();
            //iterates through each of the input items.
            foreach (global::Revit.Elements.Category c in category)
            {
                //generate the category id from the input user viewable category
                Autodesk.Revit.DB.ElementId categoryId = new ElementId(c.Id);
                //obtain the internal Revit category from the id
                Autodesk.Revit.DB.Category internalRevitCat = Autodesk.Revit.DB.Category.GetCategory(doc, categoryId);
                //get the projection lineweight
                categoryProjectionLineweight.Add(internalRevitCat.GetLineWeight(GraphicsStyleType.Projection));
            }
            return categoryProjectionLineweight;
        }

        /// <summary>
        /// Get the category cut lineweight.
        /// </summary>
        /// <returns name="categoryCutLineweight">The category projection lineweights.</returns>
        /// <search>
        /// Categories.CutLineweight
        /// </search>
        public static List<int?> CutLineweight(List<global::Revit.Elements.Category> category)
        {
            //obtains the current document for later use
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //category stuff - list to append the results to
            List<int?> categoryCutLineweight = new List<int?>();
            //iterates through each of the input items.
            foreach (global::Revit.Elements.Category c in category)
            {
                //generate the category id from the input user viewable category
                Autodesk.Revit.DB.ElementId categoryId = new ElementId(c.Id);
                //obtain the internal Revit category from the id
                Autodesk.Revit.DB.Category internalRevitCat = Autodesk.Revit.DB.Category.GetCategory(doc, categoryId);
                categoryCutLineweight.Add(internalRevitCat.GetLineWeight(GraphicsStyleType.Cut));
            }
            return categoryCutLineweight;
        }

        /// <summary>
        /// Get the category line color as RGB string..
        /// </summary>
        /// <returns name="lineColorString">The category line color as a string.</returns>
        /// <returns name="lineColor">The category line color as a usable Dynamo color.</returns>
        /// <search>
        /// Categories.LineColor
        /// </search>
        [MultiReturn(new[] { "lineColorString", "lineColor" })]
        public static Dictionary<string, object> LineColor(List<global::Revit.Elements.Category> category)
        {
            //obtains the current document for later use
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //category stuff - list to append the results to
            List<string> lineColorString = new List<string>();
            //category stuff - list to append the results to
            List<DSCore.Color> lineColor = new List<DSCore.Color>();
            //iterates through each of the input items.
            foreach (global::Revit.Elements.Category c in category)
            {
                //generate the category id from the input user viewable category
                Autodesk.Revit.DB.ElementId categoryId = new ElementId(c.Id);
                //obtain the internal Revit category from the id
                Autodesk.Revit.DB.Category internalRevitCat = Autodesk.Revit.DB.Category.GetCategory(doc, categoryId);
                //obtain the string representation of the color
                lineColorString.Add(internalRevitCat.LineColor.Red.ToString() + ", " + internalRevitCat.LineColor.Green.ToString() + ", " + internalRevitCat.LineColor.Blue.ToString());
                lineColor.Add(DSCore.Color.ByARGB(255, internalRevitCat.LineColor.Red, internalRevitCat.LineColor.Green, internalRevitCat.LineColor.Blue));
            }
            //returns the outputs
            var outInfo = new Dictionary<string, object>
            {
                {"lineColorString", lineColorString},
                {"lineColor", lineColor}
            };
            return outInfo;
        }

        /// <summary>
        /// Get the category line pattern.
        /// </summary>
        /// <returns name="linePattern">The category projection line pattern.</returns>
        /// <search>
        /// Categories.LinePattern
        /// </search>
        public static List<string> LinePattern(List<global::Revit.Elements.Category> category)
        {
            //obtains the current document for later use
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //category stuff - list to append the results to
            List<string> linePattern = new List<string>();
            //iterates through each of the input items.
            foreach (global::Revit.Elements.Category cat in category)
            {
                //generate the category id from the input user viewable category
                Autodesk.Revit.DB.ElementId categoryId = new ElementId(cat.Id);
                //obtain the internal Revit category from the id
                Autodesk.Revit.DB.Category c = Autodesk.Revit.DB.Category.GetCategory(doc, categoryId);
                //get the projection line pattern name
                if (c.GetLinePatternId(GraphicsStyleType.Projection).ToString() == "-3000010")
                {
                    linePattern.Add("Solid");
                }
                if (c.GetLinePatternId(GraphicsStyleType.Projection).ToString() == "-1")
                {
                    linePattern.Add("No Pattern");
                }
                if (!c.GetLinePatternId(GraphicsStyleType.Projection).ToString().Contains("-"))
                {
                    string linePatternName = doc.GetElement(c.GetLinePatternId(GraphicsStyleType.Projection)).Name;
                    linePattern.Add(linePatternName);
                }
            }
            return linePattern;
        }

        /// <summary>
        /// Get the category material and name.
        /// </summary>
        /// <returns name="materialName">The category material name.</returns>
        /// <search>
        /// Categories.MaterialName
        /// </search>
        public static List<string> Material(List<global::Revit.Elements.Category> category)
        {
            //obtains the current document for later use
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //category stuff - list to append the results to
            List<string> materialName = new List<string>();
            //iterates through each of the input items.
            foreach (global::Revit.Elements.Category cat in category)
            {
                //generate the category id from the input user viewable category
                Autodesk.Revit.DB.ElementId categoryId = new ElementId(cat.Id);
                //obtain the internal Revit category from the id
                Autodesk.Revit.DB.Category c = Autodesk.Revit.DB.Category.GetCategory(doc, categoryId);
                if (c.Material == null)
                {
                    materialName.Add("none");
                }
                else
                {
                    materialName.Add(c.Material.Name);
                }
            }
            return materialName;
        }
    }
}
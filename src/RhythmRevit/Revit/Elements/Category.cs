using Autodesk.DesignScript.Runtime;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Rhythm.Utilities;
using Category = Revit.Elements.Category;

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
        /// Collects all surface pattern related categories for override.
        /// </summary>
        /// <param name="toggle">Run It?</param>
        /// <returns></returns>
        [MultiReturn(new[] { "surfacePatternCategories", "cutPatternCategories" })]
        [NodeCategory("Action")]
        public static Dictionary<string, object> CollectHatchPatternCategories(bool toggle = true)
        {
            //obtains the current document for later use
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            //list to store dynamo categories
            List<global::Revit.Elements.Category> dynamoSurfacePatternCategories = new List<Category>();
            List<global::Revit.Elements.Category> dynamoCutPatternCategories = new List<Category>();

            
            foreach (Autodesk.Revit.DB.Category cat in doc.Settings.Categories)
            {
                foreach (Autodesk.Revit.DB.Category subcategory in cat.SubCategories)
                {
                    if (subcategory.Name.Contains("Surface Pattern"))
                    {
#if R20 || R21 || R22 || R23
                        var dynamoSurfacePatternCategory = global::Revit.Elements.Category.ById(subcategory.Id.IntegerValue);
#endif

#if R24 || R25
                        var dynamoSurfacePatternCategory = global::Revit.Elements.Category.ById(subcategory.Id.Value);
#endif

                        dynamoSurfacePatternCategories.Add(dynamoSurfacePatternCategory);

                    }

                    if (subcategory.Name.Contains("Cut Pattern"))
                    {
#if R20 || R21 || R22 || R23
                        var dynamoCutPatternCategory = global::Revit.Elements.Category.ById(subcategory.Id.IntegerValue);
#endif

#if R24 || R25
                        var dynamoCutPatternCategory = global::Revit.Elements.Category.ById(subcategory.Id.Value);
#endif

                        dynamoCutPatternCategories.Add(dynamoCutPatternCategory);

                    }
                }
            }

            //returns the outputs
            var outInfo = new Dictionary<string, object>
            {
                {"surfacePatternCategories", dynamoSurfacePatternCategories},
                {"cutPatternCategories", dynamoCutPatternCategories}
            };
            return outInfo;
        }


        /// <summary>
        /// Get the category projection lineweight.
        /// </summary>
        /// <returns name="categoryProjectionLineweight">The category projection lineweights.</returns>
        /// <search>
        /// Categories.ProjectionLineweight
        /// </search>
        [NodeCategory("Query")]
        public static List<int?> ProjectionLineweight(List<global::Revit.Elements.Category> category)
        {
            //category stuff - list to append the results to
            List<int?> categoryProjectionLineweight = new List<int?>();
            //iterates through each of the input items.
            foreach (global::Revit.Elements.Category c in category)
            {
                //obtain the internal Revit category from the id
                Autodesk.Revit.DB.Category internalRevitCat = c.ToRevitType();
                //get the projection lineweight
                categoryProjectionLineweight.Add(internalRevitCat.GetLineWeight(GraphicsStyleType.Projection));
            }
            return categoryProjectionLineweight;
        }

        /// <summary>
        /// Set the category projection lineweight.
        /// </summary>
        /// <returns name="category">The category</returns>
        /// <search>
        /// Categories.ProjectionLineweight
        /// </search>
        [NodeCategory("Action")]
        public static global::Revit.Elements.Category SetProjectionLineweight(global::Revit.Elements.Category category, int penWeight)
        {
            //obtains the current document for later use
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //obtain the internal Revit category
            Autodesk.Revit.DB.Category internalRevitCat = category.ToRevitType();

            TransactionManager.Instance.EnsureInTransaction(doc);
            internalRevitCat.SetLineWeight(penWeight, GraphicsStyleType.Projection);
            TransactionManager.Instance.TransactionTaskDone();

            doc.Regenerate();
            DocumentManager.Instance.CurrentUIDocument.RefreshActiveView();

            return category;
        }


        /// <summary>
        /// Get the category cut lineweight.
        /// </summary>
        /// <returns name="categoryCutLineweight">The category projection lineweights.</returns>
        /// <search>
        /// Categories.CutLineweight
        /// </search>
        [NodeCategory("Query")]
        public static List<int?> CutLineweight(List<global::Revit.Elements.Category> category)
        {
            //category stuff - list to append the results to
            List<int?> categoryCutLineweight = new List<int?>();
            //iterates through each of the input items.
            foreach (global::Revit.Elements.Category c in category)
            {

                //obtain the internal Revit category
                Autodesk.Revit.DB.Category internalRevitCat = c.ToRevitType();
                categoryCutLineweight.Add(internalRevitCat.GetLineWeight(GraphicsStyleType.Cut));
            }
            return categoryCutLineweight;
        }
        /// <summary>
        /// Set the category cut lineweight.
        /// </summary>
        /// <returns name="category">The category</returns>
        /// <search>
        /// Categories.CutLineweight
        /// </search>
        [NodeCategory("Action")]
        public static global::Revit.Elements.Category SetCutLineweight(global::Revit.Elements.Category category, int penWeight)
        {
            //obtains the current document for later use
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            
            //obtain the internal Revit category
            Autodesk.Revit.DB.Category internalRevitCat = category.ToRevitType();

            TransactionManager.Instance.EnsureInTransaction(doc);
            internalRevitCat.SetLineWeight(penWeight, GraphicsStyleType.Cut);
            TransactionManager.Instance.TransactionTaskDone();

            doc.Regenerate();
            DocumentManager.Instance.CurrentUIDocument.RefreshActiveView();


            return category;
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
        [NodeCategory("Query")]
        public static Dictionary<string, object> LineColor(List<global::Revit.Elements.Category> category)
        {
            //category stuff - list to append the results to
            List<string> lineColorString = new List<string>();
            //category stuff - list to append the results to
            List<DSCore.Color> lineColor = new List<DSCore.Color>();
            //iterates through each of the input items.
            foreach (global::Revit.Elements.Category c in category)
            {
                //obtain the internal Revit category
                Autodesk.Revit.DB.Category internalRevitCat = c.ToRevitType();
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
        [NodeCategory("Query")]
        public static List<string> LinePattern(List<global::Revit.Elements.Category> category)
        {
            //obtains the current document for later use
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //category stuff - list to append the results to
            List<string> linePattern = new List<string>();
            //iterates through each of the input items.
            foreach (global::Revit.Elements.Category cat in category)
            {
                //obtain the internal Revit category from the id
                Autodesk.Revit.DB.Category c = cat.ToRevitType();
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
        [NodeCategory("Query")]
        public static List<string> Material(List<global::Revit.Elements.Category> category)
        {
            //category stuff - list to append the results to
            List<string> materialName = new List<string>();
            //iterates through each of the input items.
            foreach (global::Revit.Elements.Category cat in category)
            {
                //obtain the internal Revit category
                Autodesk.Revit.DB.Category c = cat.ToRevitType();
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
#if R22_OR_GREATER
        /// <summary>
        /// Get the built in category name. Useful for troubleshooting.
        /// </summary>
        /// <param name="category">The category</param>
        /// <returns name="builtInCategoryName"></returns>
        public static string BuiltInCategoryName(global::Revit.Elements.Category category)
        {
            //obtain the internal Revit category
            Autodesk.Revit.DB.Category c = category.ToRevitType();

            return c.BuiltInCategory.ToString();
        }
#endif

    }
}
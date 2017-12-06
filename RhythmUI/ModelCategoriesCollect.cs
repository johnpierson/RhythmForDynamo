using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using CoreNodeModels;
using DSRevitNodesUI;
using Dynamo.Graph.Nodes;
using ProtoCore.AST.AssociativeAST;
using Revit.Elements;
using Revit.Elements.InternalUtilities;
using RevitServices.Persistence;
using Category = Revit.Elements.Category;
using Element = Revit.Elements.Element;


namespace RhythmUI
{

    [NodeName("Model Category Collect")]
    [NodeCategory("Rhythm.RevitElements.Selection")]
    [NodeDescription("Provides all model categories to select from.")]
    [IsDesignScriptCompatible]
    public class ModelCategoriesCollect : RevitDropDownBase
    {
        public ModelCategoriesCollect() : base("Model Category Collect")
        {
        }
        //this populates the categories to choose from
        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();
            var document = DocumentManager.Instance.CurrentDBDocument;

            foreach (BuiltInCategory categoryId in Enum.GetValues(typeof(BuiltInCategory)))
            {
                Autodesk.Revit.DB.Category category;
                try
                {
                    category = Autodesk.Revit.DB.Category.GetCategory(document, categoryId);
                }
                catch
                {
                    // We get here for internal/deprecated categories
                    continue;
                }

                if (category != null && category.CategoryType == CategoryType.Model)
                {
                    string name = getFullName(category);
                    Items.Add(new DynamoDropDownItem(name, categoryId));
                }
            }

            Items = Items.OrderBy(x => x.Name).ToObservableCollection();
            return SelectionState.Restore;
        }

        //this is where we nest the meat and potatoes
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            //Some of the legacy categories which were not working before will now be out of index.
            if (SelectedIndex < 0 || SelectedIndex >= Items.Count)
                return new[] {AstFactory.BuildNullNode()};

            BuiltInCategory categoryId = (BuiltInCategory) Items[SelectedIndex].Item;

            var args = new List<AssociativeNode>
            {
              AstFactory.BuildIntNode((int)categoryId)
            };

            var func = new Func<int, List<Revit.Elements.Element>>(Functions.GetCats);
            var functionCall = AstFactory.BuildFunctionCall(func, args);

            return new[] {AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall)};
        }
        //method to display the category name
        private static string getFullName(Autodesk.Revit.DB.Category category)
        {
            string name = string.Empty;
            if (category != null)
            {
                var parent = category.Parent;
                if (parent == null)
                {
                    // Top level category
                    // For example "Cable Trays"
                    name = category.Name.ToString();
                }
                else
                {
                    // Sub-category
                    // For example "Cable Tray - Center Lines"
                    name = parent.Name.ToString() + " - " + category.Name.ToString();
                }
            }
            return name;
        }
        //my custom function to return stuff
        [IsVisibleInDynamoLibrary(false)]
        public static class Functions
        {
            public static List<Revit.Elements.Element> GetCats(int catId)
            {
                Document doc = DocumentManager.Instance.CurrentDBDocument;
                ElementId goodId = new ElementId(catId);
                Autodesk.Revit.DB.Category category = Autodesk.Revit.DB.Category.GetCategory(doc, goodId);
                var catFilter = new ElementCategoryFilter(category.Id);
                var fec = new FilteredElementCollector(doc);
                var instances =
                    fec.WherePasses(catFilter)
                        .WhereElementIsNotElementType()
                        .ToElementIds()
                        .Select(id => ElementSelector.ByElementId(id.IntegerValue))
                        .ToList();
                return instances;
            }
        }



    }

    }


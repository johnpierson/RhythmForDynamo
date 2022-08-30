using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using CoreNodeModels;
using DSRevitNodesUI;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using RevitServices.Persistence;
using Category = Revit.Elements.Category;

namespace RhythmUI
{

    [NodeName("Model Categories")]
    [NodeCategory("Rhythm.Revit.Selection.Selection")]
    [NodeDescription("Provides all model categories to select from.")]
    [IsDesignScriptCompatible]
    public class ModelCategories : RevitDropDownBase
    {
        private const string outputName = "Model Category";
        public ModelCategories() : base(outputName) {}
        [JsonConstructor]
        public ModelCategories(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(outputName, inPorts, outPorts)
        {
        }

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

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            //Some of the legacy categories which were not working before will now be out of index.
            if (SelectedIndex < 0 || SelectedIndex >= Items.Count)
                return new[] {AstFactory.BuildNullNode()};

            BuiltInCategory categoryId = (BuiltInCategory) Items[SelectedIndex].Item;

            var args = new List<AssociativeNode>
            {
                AstFactory.BuildIntNode((int) categoryId)
            };
            var func = new Func<int, Category>(Revit.Elements.Category.ById);
            var functionCall = AstFactory.BuildFunctionCall(func, args);


            return new[] {AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall)};
        }

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
        }

    }


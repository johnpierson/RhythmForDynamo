using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using CoreNodeModels;
using DSRevitNodesUI;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using Revit.Elements;
using RevitServices.Persistence;


namespace RhythmUI
{

    [NodeName("All Elements of Model Category")]
    [NodeCategory("Rhythm.Revit.Selection.Selection")]
    [NodeDescription("This allows you to collect all elements of a selected category. This also removes the subcategories from the selection.")]
    [IsDesignScriptCompatible]
    public class ModelCategoriesCollect : RevitDropDownBase
    {
        private const string outputName = "element";
        public ModelCategoriesCollect() : base(outputName)
        {
        }
        [JsonConstructor]
        public ModelCategoriesCollect(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(outputName, inPorts, outPorts)
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
                if (category != null && category.CategoryType == CategoryType.Model && category.Parent == null)
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

            AssociativeNode node;

            Func<string, bool, Revit.Elements.Element> func = ElementSelector.ByUniqueId;
            var catColl = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);
            var elements = catColl.OfCategory(categoryId).WhereElementIsNotElementType().ToElements();
            var results = elements.ToList();

                var newInputs =
                    results.Select(
                        el =>
                            AstFactory.BuildFunctionCall(
                                func,
                                new List<AssociativeNode>
                                {
                                    AstFactory.BuildStringNode(el.UniqueId),
                                    AstFactory.BuildBooleanNode(true)
                                })).ToList();

                node = AstFactory.BuildExprList(newInputs);
            

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
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
        


    }

}
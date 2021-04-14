using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using CoreNodeModels;
using DSRevitNodesUI;
using Dynamo.Graph.Nodes;
using Dynamo.Utilities;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using RevitServices.Persistence;

namespace RhythmUI
{
    [NodeName("Titleblock Types")]
    [NodeCategory("Rhythm.Revit.Selection.Selection")]
    [NodeDescription("Allows you to select a titleblock type from your Revit file.")]
    [IsDesignScriptCompatible]
    public class TagTypes : RevitDropDownBase
    {
        private const string noTitleblockTypes = "No titleblock types available in project.";
        private const string outputName = "titleblockType";

        public TagTypes() : base(outputName) { }
        [JsonConstructor]
        public TagTypes(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(outputName, inPorts, outPorts)
        {
        }
        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();
            //find all sheets in the project
            var tagTypeCollector = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);
            var elements = tagTypeCollector.OfClass(typeof(Family)).ToElements();
            if (!elements.Any())
            {
                Items.Add(new DynamoDropDownItem(noTitleblockTypes, null));
                SelectedIndex = 0;
                return SelectionState.Done;
            }

            foreach (Family family in elements)
            {
                foreach (var id in family.GetFamilySymbolIds())
                {
                    var fs = family.Document.GetElement(id);
                    if (fs.Category.Id.IntegerValue.Equals(-2000280))
                    {
                        Items.Add(new DynamoDropDownItem(string.Format("{0}:{1}", family.Name, fs.Name), fs));
                    }
                }
            }


            Items = ExtensionMethods.ToObservableCollection(Items.OrderBy(x => x.Name));
            return SelectionState.Restore;
        }
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (Items.Count == 0 ||
                Items[0].Name == noTitleblockTypes ||
                SelectedIndex == -1)
            {
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }
            var node = AstFactory.BuildFunctionCall(
                "Revit.Elements.ElementSelector",
                "ByElementId",
                new List<AssociativeNode>
                {
                    AstFactory.BuildIntNode(((FamilySymbol)Items[SelectedIndex].Item).Id.IntegerValue)
                });
            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using CoreNodeModels;
using Dynamo.Graph.Nodes;
using ProtoCore.AST.AssociativeAST;
using RevitServices.Persistence;
using DSRevitNodesUI;
using Newtonsoft.Json;


namespace RhythmUI



{
    [NodeName("Roof Types")]
    [NodeCategory("Rhythm.Revit.Selection.Selection")]
    [NodeDescription("Allows you to select a roof type from the types in your project.")]
    [IsDesignScriptCompatible]
    public class RoofTypes : RevitDropDownBase
    {
        private const string noRoofs = "No roof types available in project.";
        private const string outputName = "Roof Type";
        public RoofTypes() : base(outputName) { }
        [JsonConstructor]
        public RoofTypes(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(outputName, inPorts, outPorts)
        {
        }
        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();
            //find all sheets in the project
            var roofTypeColl = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);
            roofTypeColl.OfClass(typeof(RoofType));
            var elements = roofTypeColl.ToElements();
            if (!elements.Any())
            {
                Items.Add(new DynamoDropDownItem(noRoofs, null));
                SelectedIndex = 0;
                return SelectionState.Done;
            }
            Items = elements.Select(x => new DynamoDropDownItem((x.Name), x)).OrderBy(x => x.Name).ToObservableCollection();
            return SelectionState.Restore;
        }
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (Items.Count == 0 ||
                Items[0].Name == noRoofs ||
                SelectedIndex == -1)
            {
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }
            var node = AstFactory.BuildFunctionCall(
                "Revit.Elements.ElementSelector",
                "ByElementId",
                new List<AssociativeNode>
                {
#if R26_OR_GREATER
                    AstFactory.BuildIntNode(((RoofType)Items[SelectedIndex].Item).Id.Value)
#endif
#if !R26_OR_GREATER
                    AstFactory.BuildIntNode(((RoofType)Items[SelectedIndex].Item).Id.IntegerValue)
#endif
                });
            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }
    }
}

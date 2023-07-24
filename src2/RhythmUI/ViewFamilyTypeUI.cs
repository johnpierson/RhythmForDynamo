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
    [NodeName("ViewFamilyTypes")]
    [NodeCategory("Rhythm.Revit.Selection.Selection")]
    [NodeDescription("Allows you to select a view family type from your file")]
    [IsDesignScriptCompatible]
    public class ViewFamilyTypes : RevitDropDownBase
    {
        private const string noFamilyTypes = "No View Family Types available in project.";
        private const string outputName = "ViewFamilyTypes";
        public ViewFamilyTypes() : base(outputName) { }
        [JsonConstructor]
        public ViewFamilyTypes(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(outputName, inPorts, outPorts)
        {
        }
        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();
            //find all sheets in the project
            var sheetColl = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);
            sheetColl.OfClass(typeof(ViewFamilyType));
            var elements = sheetColl.ToElements();
            if (!elements.Any())
            {
                Items.Add(new DynamoDropDownItem(noFamilyTypes, null));
                SelectedIndex = 0;
                return SelectionState.Done;
            }
            Items = elements.Select(x => new DynamoDropDownItem(x.Name, x)).OrderBy(x => x.Name).ToObservableCollection();
            return SelectionState.Restore;
        }
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (Items.Count == 0 ||
                Items[0].Name == noFamilyTypes ||
                SelectedIndex == -1)
            {
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }
            var node = AstFactory.BuildFunctionCall(
                "Revit.Elements.ElementSelector",
                "ByElementId",
                new List<AssociativeNode>
                {
                    AstFactory.BuildIntNode(((ViewFamilyType)Items[SelectedIndex].Item).Id.IntegerValue)
                });
            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }
    }
}

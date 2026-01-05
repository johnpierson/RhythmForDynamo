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
    [NodeName("Sheets")]
    [NodeCategory("Rhythm.Revit.Selection.Selection")]
    [NodeDescription("Allows you to select a sheet from all of the sheets in your project.")]
    [IsDesignScriptCompatible]
    public class Sheets : RevitDropDownBase
    {
        private const string noSheets = "No Sheets available in project.";
        private const string outputName = "Sheet";
        public Sheets() : base(outputName) { }
        [JsonConstructor]
        public Sheets(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(outputName, inPorts, outPorts)
        {
        }
        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();
            //find all sheets in the project
            var sheetColl = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);
            sheetColl.OfClass(typeof(ViewSheet));
            var elements = sheetColl.ToElements();
            if (!elements.Any())
            {
                Items.Add(new DynamoDropDownItem(noSheets, null));
                SelectedIndex = 0;
                return SelectionState.Done;
            }
            Items = elements.Select(x => new DynamoDropDownItem((x.get_Parameter(BuiltInParameter.SHEET_NUMBER)).AsString() + " : " +  x.Name, x)).OrderBy(x => x.Name).ToObservableCollection();
            return SelectionState.Restore;
        }
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (Items.Count == 0 ||
                Items[0].Name == noSheets ||
                SelectedIndex == -1)
            {
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }
            var node = AstFactory.BuildFunctionCall(
                "Revit.Elements.ElementSelector",
                "ByElementId",
                new List<AssociativeNode>
                {

#if !R25_OR_GREATER
                    AstFactory.BuildIntNode(((ViewSheet)Items[SelectedIndex].Item).Id.IntegerValue)
#endif
#if R25_OR_GREATER
                    AstFactory.BuildIntNode(((ViewSheet)Items[SelectedIndex].Item).Id.Value)

#endif
                });
            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }
    }
}

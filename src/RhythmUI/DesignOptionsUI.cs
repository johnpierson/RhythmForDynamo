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
    [NodeName("Design Options")]
    [NodeCategory("Rhythm.Revit.Selection.Selection")]
    [NodeDescription("Displays design options with option set for your use.")]
    [IsDesignScriptCompatible]
    public class DesignOptions : RevitDropDownBase 
    {
        private const string noDesignOptions = "No Design Options available in project.";
        private const string outputName = "DesignOption";

        public DesignOptions() : base(outputName) { }
        [JsonConstructor]
        public DesignOptions(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(outputName, inPorts, outPorts)
        {
        }
        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();
            //find all design options in the project
            var designOptColl = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);
            designOptColl.OfClass(typeof(DesignOption));
            var elements = designOptColl.ToElements();
            if (!elements.Any())
            {
                Items.Add(new DynamoDropDownItem(noDesignOptions, null));
                SelectedIndex = 0;
                return SelectionState.Done;
            }
            Items = elements.Select(x => new DynamoDropDownItem((x.Document.GetElement(x.get_Parameter(BuiltInParameter.OPTION_SET_ID).AsElementId())).Name + " : " + x.Name, x)).OrderBy(x => x.Name).ToObservableCollection();
            return SelectionState.Restore;
        }
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (Items.Count == 0 ||
                Items[0].Name == noDesignOptions ||
                SelectedIndex == -1)
            {
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }
            var node = AstFactory.BuildFunctionCall(
                "Revit.Elements.ElementSelector",
                "ByElementId",
                new List<AssociativeNode>
                {
                    AstFactory.BuildIntNode(((DesignOption)Items[SelectedIndex].Item).Id.IntegerValue)
                });
            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };

        }
    }
}

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
    [NodeName("Sheet Collections")]
    [NodeCategory("Rhythm.Revit.Selection.Selection")]
    [NodeDescription("Allows you to select a sheet collection from all sheet collections in your project.")]
    [IsDesignScriptCompatible]
    public class SheetCollection : RevitDropDownBase
    {
        private const string noCollections = "No Sheet Collections available in project.";
        private const string outputName = "SheetCollection";
        public SheetCollection() : base(outputName) { }
        [JsonConstructor]
        public SheetCollection(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(outputName, inPorts, outPorts)
        {
        }
        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();
            var doc = DocumentManager.Instance.CurrentDBDocument;
            
#if !R25_OR_GREATER
            // SheetCollection class is only available in R25+
            Items.Add(new DynamoDropDownItem("Sheet Collections require Revit 2025 or later.", null));
            SelectedIndex = 0;
            return SelectionState.Done;
#else
            // Find all SheetCollection elements in the project
            var collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(Autodesk.Revit.DB.SheetCollection));
            var elements = collector.ToElements().Cast<Autodesk.Revit.DB.SheetCollection>();
            
            if (!elements.Any())
            {
                Items.Add(new DynamoDropDownItem(noCollections, null));
                SelectedIndex = 0;
                return SelectionState.Done;
            }
            
            Items = elements.Select(x => new DynamoDropDownItem(x.Name, x)).OrderBy(x => x.Name).ToObservableCollection();
            return SelectionState.Restore;
#endif
        }
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (Items.Count == 0 ||
                Items[0].Name == noCollections ||
                Items[0].Name == "Sheet Collections require Revit 2025 or later." ||
                SelectedIndex == -1)
            {
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }

#if !R25_OR_GREATER
            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
#else
            var selectedCollection = Items[SelectedIndex].Item as Autodesk.Revit.DB.SheetCollection;
            if (selectedCollection == null)
            {
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }

            var node = AstFactory.BuildFunctionCall(
                "Revit.Elements.ElementSelector",
                "ByElementId",
                new List<AssociativeNode>
                {
                    AstFactory.BuildIntNode(selectedCollection.Id.Value)
                });
            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
#endif
        }
    }
}


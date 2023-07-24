using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using CoreNodeModels;
using Dynamo.Graph.Nodes;
using ProtoCore.AST.AssociativeAST;
using RevitServices.Persistence;
using DSRevitNodesUI;
using Newtonsoft.Json;
using Revit.Elements;


namespace RhythmUI



{
    [NodeName("Views++")]
    [NodeCategory("Rhythm.Revit.Selection.Selection")]
    [NodeDescription("Allows you to select a view from the instances in your project. With sorting and exclusion of View Templates.")]
    [IsDesignScriptCompatible]
    public class Views : RevitDropDownBase
    {
        private const string outputName = "View";
        public Views() : base(outputName) { }
        [JsonConstructor]
        public Views(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(outputName, inPorts, outPorts)
        {
        }
        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();
            //find all views in the project
            //exclude <RevisionSchedule> (revision tables on sheets) from list
            var views = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument)
                .OfClass(typeof(View)).Cast<View>()
                .Where(x => !x.Name.Contains('<'))
                .Where(v => !v.IsTemplate)
                .ToList();
            Items = views.Select(x => new DynamoDropDownItem((x.Name), x)).OrderBy(x => x.Name).ToObservableCollection();

            return SelectionState.Restore;
        }
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node;

            if (SelectedIndex == -1)
            {
                node = AstFactory.BuildNullNode();
            }
            else
            {
                var view = Items[SelectedIndex].Item as View;
                if (view == null)
                {
                    node = AstFactory.BuildNullNode();
                }
                else
                {
                    var idNode = AstFactory.BuildStringNode(view.UniqueId);
                    var falseNode = AstFactory.BuildBooleanNode(true);

                    node =
                        AstFactory.BuildFunctionCall(
                            new Func<string, bool, object>(ElementSelector.ByUniqueId),
                            new List<AssociativeNode>() { idNode, falseNode });
                }
            }

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }
    }
    [NodeName("Schedule Views")]
    [NodeCategory("Rhythm.Revit.Selection.Selection")]
    [NodeDescription("Allows you to select a schedule view from the instances in your project. With sorting and exclusion of View Templates.")]
    [IsDesignScriptCompatible]
    public class ScheduleViews : RevitDropDownBase
    {
        private const string outputName = "View";
        public ScheduleViews() : base(outputName) { }
        [JsonConstructor]
        public ScheduleViews(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(outputName, inPorts, outPorts)
        {
        }
        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();
            //find all views in the project
            //exclude <RevisionSchedule> (revision tables on sheets) from list
            var views = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument)
                .OfClass(typeof(ViewSchedule)).Cast<ViewSchedule>()
                .Where(x => !x.Name.Contains('<'))
                .Where(v => !v.IsTemplate)
                .ToList();
            Items = views.Select(x => new DynamoDropDownItem((x.Name), x)).OrderBy(x => x.Name).ToObservableCollection();

            return SelectionState.Restore;
        }
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node;

            if (SelectedIndex == -1)
            {
                node = AstFactory.BuildNullNode();
            }
            else
            {
                var view = Items[SelectedIndex].Item as View;
                if (view == null)
                {
                    node = AstFactory.BuildNullNode();
                }
                else
                {
                    var idNode = AstFactory.BuildStringNode(view.UniqueId);
                    var falseNode = AstFactory.BuildBooleanNode(true);

                    node =
                        AstFactory.BuildFunctionCall(
                            new Func<string, bool, object>(ElementSelector.ByUniqueId),
                            new List<AssociativeNode>() { idNode, falseNode });
                }
            }

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }
    }
}

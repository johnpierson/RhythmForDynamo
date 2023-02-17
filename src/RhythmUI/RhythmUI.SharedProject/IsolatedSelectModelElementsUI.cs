using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using Revit.Elements;
using RevitServices.Persistence;
using Element = Autodesk.Revit.DB.Element;

namespace RhythmUI
{
    
    [NodeName("Isolated Select Model Elements")]
    [NodeDescription("This allows you to select elements of a specific category. This is super-ultra BETA and may not always work..")]
    [NodeCategory("Rhythm.Revit.Selection.Selection")]
    [OutPortNames("Elements")]
    [OutPortTypes("element")]
    [OutPortDescriptions("Selected Elements")]
    [IsDesignScriptCompatible]
    public class IsolatedSelectModelElementsUI:NodeModel
    {
        
        

        [JsonConstructor]
        private IsolatedSelectModelElementsUI(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
        }

        public IsolatedSelectModelElementsUI()
        {
            RegisterAllPorts();
        }

        public static List<Element> results;


        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {

            AssociativeNode node;
            
            Func<string, bool, Revit.Elements.Element> func = ElementSelector.ByUniqueId;
            try
            {
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
            catch (Exception)
            {
                return new[] { AstFactory.BuildNullNode() };
            }

        }

        
    }
}

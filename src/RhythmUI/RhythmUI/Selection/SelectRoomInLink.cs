using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI.Selection;
using CoreNodeModels;
using Dynamo.Graph.Nodes;
using Dynamo.Logging;
using Dynamo.Nodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using Revit.Elements;
using RevitServices.Persistence;
using Rhythm.Revit.Selection;
using RhythmUI.Utilities;
using Element = Autodesk.Revit.DB.Element;

namespace RhythmUI
{
    [IsDesignScriptCompatible]
    [NodeCategory("Rhythm.Revit.Selection.Selection")]
    [NodeDescription("This allows you to select an element from a link. Useful for Dynamo player and Generative Design.")]
    [NodeName("Select Element from Link")]
    public class DSModelRoomInLinkSelection : ElementFilterSelection<Element>
    {
        private const string message = "Select Model Element";
        private const string prefix = "Element";


        public DSModelRoomInLinkSelection() : base(SelectionType.One, SelectionObjectType.None, DSModelRoomInLinkSelection.message, prefix)
        {
            //this.Selection.First();
            this.Modified += OnNodeExecutionEnd;
            string tttt = "";
        }

        private void OnNodeExecutionEnd(NodeModel obj)
        {
            string cats = obj.Category;
        }

        [JsonConstructor]
        public DSModelRoomInLinkSelection(IEnumerable<string> selectionIdentifier, IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : 
            base(SelectionType.One, SelectionObjectType.None, DSModelRoomInLinkSelection.message, prefix, selectionIdentifier, inPorts, outPorts)
        {

        }
        
    }
    public abstract class ElementFilterSelection<TSelection> : ElementSelection<TSelection>
            where TSelection : Element
    {
        public ISelectionFilter Filter { get; set; }
        protected ElementFilterSelection(SelectionType selectionType,
            SelectionObjectType selectionObjectType, string message, string prefix)
            : base(selectionType, selectionObjectType, message, prefix) { }

        [JsonConstructor]
        protected ElementFilterSelection(SelectionType selectionType,
            SelectionObjectType selectionObjectType, string message, string prefix,
            IEnumerable<string> selectionIdentifier, IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts)
            : base(selectionType, selectionObjectType, message, prefix, selectionIdentifier, inPorts, outPorts) { }

        public override IModelSelectionHelper<TSelection> SelectionHelper
        {
            get
            {
                var selectionHelper = RevitElementSelectionHelper<TSelection>.Instance;

                return selectionHelper;
            }
        }
        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node = null;
            Func<string, bool, Revit.Elements.Element> func = ElementSelection.InLinkDoc;

            var results = SelectionResults.ToList();

            if (SelectionResults == null || !results.Any())
            {
                node = AstFactory.BuildNullNode();
            }
            else if (results.Count == 1)
            {
                var el = results.First();
                //node = AstFactory.BuildFunctionCall(
                //    "Rhythm.Revit.Selection",
                //    "InLinkDoc",
                //    new List<AssociativeNode>
                //    {
                //                AstFactory.BuildStringNode(el.UniqueId),
                //                AstFactory.BuildBooleanNode(true)
                //    });
                // If there is only one object in the list,
                // return a single item.
                node = AstFactory.BuildFunctionCall(
                    func,
                    new List<AssociativeNode>
                    {
                        AstFactory.BuildStringNode(el.UniqueId),
                        AstFactory.BuildBooleanNode(true)
                    });
            }
            //else
            //{
            //    var newInputs =
            //        results.Select(
            //            el =>
            //                AstFactory.BuildFunctionCall(
            //                    func,
            //                    new List<AssociativeNode>
            //                    {
            //                        AstFactory.BuildStringNode(el.UniqueId),
            //                        AstFactory.BuildBooleanNode(true)
            //                    })).ToList();

            //    node = AstFactory.BuildExprList(newInputs);
            //}

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }

      
    }
    internal class RevitElementSelectionHelper<T> : LogSourceBase, IModelSelectionHelper<T>
        where T : Autodesk.Revit.DB.Element
    {
        public static RevitElementSelectionHelper<T> Instance { get; } = new RevitElementSelectionHelper<T>();

        /// <summary>
        /// Request an element in a selection.
        /// </summary>
        /// <typeparam name="T">The type of the Element.</typeparam>
        /// <param name="selectionMessage">The message to display.</param>
        /// <param name="selectionType">The selection type.</param>
        /// <param name="objectType">The selection object type.</param>
        /// <returns></returns>
        public IEnumerable<T> RequestSelectionOfType(
            string selectionMessage,
            SelectionType selectionType,
            SelectionObjectType objectType)
        {
            switch (selectionType)
            {
                case SelectionType.One:
                    return RequestElementSelection(selectionMessage, AsLogger());

                case SelectionType.Many:
                    return RequestElementSelection(selectionMessage, AsLogger());
            }

            return null;
        }

        private IEnumerable<T> RequestElementSelection(string selectionMessage, ILogger logger)
        {
            var doc = DocumentManager.Instance.CurrentUIDocument;

            //CategoryElementSelectionFilter<Element> filter = new CategoryElementSelectionFilter<Element>();
            //filter.Category = BuiltInCategory.OST_Rooms;
            



            Element e = null;

            //var choices = doc.Selection;
            //choices.SetElementIds(new Collection<ElementId>());

            var elementRef = doc.Selection.PickObject(ObjectType.LinkedElement, selectionMessage);
            
            RevitLinkInstance link = DocumentManager.Instance.CurrentDBDocument.GetElement(elementRef) as RevitLinkInstance;

            if (elementRef != null)
            {
                e = link.GetLinkDocument().GetElement(elementRef.LinkedElementId);
            }

            return new[] { e }.Cast<T>();
        }

       
    }
    [IsVisibleInDynamoLibrary(false)]
    internal class CategoryElementSelectionFilter<T> : ISelectionFilter
    {
        public BuiltInCategory Category { get; set; }
        public bool AllowElement(Element elem)
        {
            return (BuiltInCategory)System.Enum.Parse(typeof(BuiltInCategory), elem.Category.Id.ToString()) == Category;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}

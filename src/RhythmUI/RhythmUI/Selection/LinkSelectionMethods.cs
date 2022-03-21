using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using CoreNodeModels;
using DSCore;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Logging;
using Dynamo.Nodes;
using Dynamo.UI.Commands;
using Dynamo.Wpf;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using RevitServices.Persistence;
using Rhythm.Revit.Selection;
using Element = Autodesk.Revit.DB.Element;
using DSRevitNodesUI.Controls;

namespace RhythmUI
{
    #region Nodes
    [IsDesignScriptCompatible]
    [NodeCategory("Rhythm.Revit.Selection.Selection")]
    [NodeDescription("This allows you to select an element from a link. Useful for Dynamo player and Generative Design.")]
    [NodeName("Select Element from Link")]
    public class SelectElementInLink : ElementFilterSelection<Element>
    {
        private const string Message = "Select Model Element";
        private new const string Prefix = "Element";

        public SelectElementInLink() : base(SelectionType.One, SelectionObjectType.None, SelectElementInLink.Message, Prefix)
        {
        }

        [JsonConstructor]
        public SelectElementInLink(IEnumerable<string> selectionIdentifier, IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) :
            base(SelectionType.One, SelectionObjectType.None, SelectElementInLink.Message, Prefix, selectionIdentifier, inPorts, outPorts)
        {
        }
    }

    [IsDesignScriptCompatible]
    [NodeCategory("Rhythm.Revit.Selection.Selection")]
    [NodeDescription("This allows you to select an element from a link of a given cateogry. Useful for Dynamo player and Generative Design.")]
    [NodeName("Select Element from Link of Category")]
    public class SelectElementInLinkOfCategory : ElementFilterSelection<Element>
    {
        private const string message = "Select Model Element";
        private const string prefix = "Element";

        internal CategoryElementSelectionFilter<Element> SelectionFilter { get; set; }

        public DSRevitNodesUI.Categories DropDownNodeModel { get; set; }

        private int selectedIndex;

        [JsonProperty(PropertyName = "SelectedIndex")]
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                selectedIndex = value;
                DropDownNodeModel.SelectedIndex = value;
                if (value >= 0)
                    SelectionFilter.Category = (BuiltInCategory)DropDownNodeModel.Items[SelectedIndex].Item;

            }
        }

        public SelectElementInLinkOfCategory()
            : base(
                SelectionType.One,
                SelectionObjectType.None,
                message,
                prefix)
        {
            DropDownNodeModel = new DSRevitNodesUI.Categories();
            SelectedIndex = DropDownNodeModel.SelectedIndex;
            SelectionFilter = new CategoryElementSelectionFilter<Element>();
            base.Filter = SelectionFilter;
        }

        [JsonConstructor]
        public SelectElementInLinkOfCategory(IEnumerable<string> selectionIdentifier, IEnumerable<PortModel> inPorts,
            IEnumerable<PortModel> outPorts)
            : base(
                SelectionType.One,
                SelectionObjectType.None,
                message,
                prefix,
                selectionIdentifier,
                inPorts,
                outPorts)
        {
            DropDownNodeModel = new DSRevitNodesUI.Categories();
            SelectionFilter = new CategoryElementSelectionFilter<Element>();
            base.Filter = SelectionFilter;
        }

    }



    #endregion

    public class CategoryDropDown : DSRevitNodesUI.Categories
    {
        public CategoryDropDown() : base() { }
    }
    #region Node View Customization
    public class SelectElementInLinkOfCategoryNodeViewCustomization : INodeViewCustomization<SelectElementInLinkOfCategory>
    {
        public SelectElementInLinkOfCategory Model { get; set; }
        public DelegateCommand SelectCommand { get; set; }

        public void CustomizeView(SelectElementInLinkOfCategory model, NodeView nodeView)
        {
            Model = model;
            SelectCommand = new DelegateCommand(Model.Select);
            Model.PropertyChanged += (s, e) =>
            {
                nodeView.Dispatcher.Invoke(new Action(() =>
                {
                    if (e.PropertyName == "CanSelect")
                    {
                        SelectCommand.RaiseCanExecuteChanged();
                    }
                }));
            };
            var comboControl = new ComboControl { DataContext = this };
            nodeView.inputGrid.Children.Add(comboControl);
        }

        public void Dispose()
        {
        }
    }
    #endregion

    #region Selection Helpers
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
                selectionHelper.ElementFilter = Filter;
                return selectionHelper;
            }
        }
        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node = null;
            Func<string, string, bool, Revit.Elements.Element> func = ElementSelection.InLinkDoc;

            var results = SelectionResults.ToList();

            if (SelectionResults == null || !results.Any())
            {
                node = AstFactory.BuildNullNode();
            }
            else if (results.Count == 1)
            {
                var el = results.First();

                node = AstFactory.BuildFunctionCall(
                    func,
                    new List<AssociativeNode>
                    {
                        AstFactory.BuildStringNode(el.Document.Title),
                        AstFactory.BuildStringNode(el.UniqueId),
                        AstFactory.BuildBooleanNode(true)
                    });
            }
            else
            {
                var newInputs =
                    results.Select(
                        el =>
                            AstFactory.BuildFunctionCall(
                                func,
                                new List<AssociativeNode>
                                {
                                    AstFactory.BuildStringNode(el.Document.Title),
                                    AstFactory.BuildStringNode(el.UniqueId),
                                    AstFactory.BuildBooleanNode(true)
                                })).ToList();

                node = AstFactory.BuildExprList(newInputs);
            }

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }


    }
    internal class RevitElementSelectionHelper<T> : LogSourceBase, IModelSelectionHelper<T> where T : Autodesk.Revit.DB.Element
    {
        public static RevitElementSelectionHelper<T> Instance { get; } = new RevitElementSelectionHelper<T>();
        public ISelectionFilter ElementFilter { get; set; }

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
            Element e = null;
            Reference elementRef = null;

            if (ElementFilter != null)
            {

                //var choices = doc.Selection;
                //choices.SetElementIds(new Collection<ElementId>());

                elementRef = doc.Selection.PickObject(ObjectType.LinkedElement, ElementFilter, selectionMessage);

                RevitLinkInstance link = DocumentManager.Instance.CurrentDBDocument.GetElement(elementRef) as RevitLinkInstance;

                if (elementRef != null)
                {
                    e = link.GetLinkDocument().GetElement(elementRef.LinkedElementId);
                }
            }

            else
            {
                elementRef = doc.Selection.PickObject(ObjectType.LinkedElement, selectionMessage);

                RevitLinkInstance link = DocumentManager.Instance.CurrentDBDocument.GetElement(elementRef) as RevitLinkInstance;

                if (elementRef != null)
                {
                    e = link.GetLinkDocument().GetElement(elementRef.LinkedElementId);
                }
            }

            return new[] { e }.Cast<T>();
        }


    }
    #endregion

    [IsVisibleInDynamoLibrary(false)]
    internal class CategoryElementSelectionFilter<T> : ISelectionFilter
    {
        public BuiltInCategory Category { get; set; }
        public bool AllowElement(Element elem)
        {
            if (elem is RevitLinkInstance revitLinkInstance)
            {

                return true;

            }
            else
            {
                return false;
            }
            
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            var elem = DocumentManager.Instance.CurrentDBDocument.GetElement(reference);
            if (elem != null && elem is RevitLinkInstance link)
            {
                var linkElem = link.GetLinkDocument().GetElement(reference.LinkedElementId);
                return (BuiltInCategory)System.Enum.Parse(typeof(BuiltInCategory), linkElem.Category.Id.ToString()) == Category;
            }
            else
            {
                return (BuiltInCategory) System.Enum.Parse(typeof(BuiltInCategory), elem.Category.Id.ToString()) == Category;
            }
        }
    }
    [IsVisibleInDynamoLibrary(false)]
    internal class TypeElementSelectionFilter<T> : ISelectionFilter
    {
        public string ElementTypeName { get; set; }

        internal Type GetElementType()
        {
            return Types.FindTypeByNameInAssembly(ElementTypeName, "RevitAPI");
        }

        public bool AllowElement(Element elem)
        {
            return elem.GetType() == GetElementType();
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }

}

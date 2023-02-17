using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using CoreNodeModels;
using CoreNodeModels.HigherOrder;
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
#if !Revit2020
using DSRevitNodesUI.Controls;
#endif
using Dynamo.Graph.Connectors;

namespace RhythmUI
{
    #region Nodes
    [IsDesignScriptCompatible]
    [NodeCategory("Rhythm.Revit.Selection.RevitLinkSelection")]
    [NodeDescription("This allows you to select an element from a link. Useful for Dynamo player and Generative Design.")]
    [NodeName("Select Element from Link")]
    public class SelectElementInLink : ElementFilterSelection<Element>
    {
        private const string Message = "Select Model Element";
        private new const string Prefix = "Element";

        public SelectElementInLink() : base(SelectionType.One, SelectionObjectType.None, SelectElementInLink.Message, Prefix)
        {
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("transform", "the link instance transform")));
        }

        [JsonConstructor]
        public SelectElementInLink(IEnumerable<string> selectionIdentifier, IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) :
            base(SelectionType.One, SelectionObjectType.None, SelectElementInLink.Message, Prefix, selectionIdentifier, inPorts, outPorts)
        {
            
        }
    }

    [IsDesignScriptCompatible]
    [NodeCategory("Rhythm.Revit.Selection.RevitLinkSelection")]
    [NodeDescription("This allows you to select multiple elements from links. Useful for Dynamo player and Generative Design.")]
    [NodeName("Select Elements from Link")]
    public class SelectElementsInLink : ElementFilterSelection<Element>
    {
        private const string Message = "Select Model Element";
        private new const string Prefix = "Element";

        public SelectElementsInLink() : base(SelectionType.Many, SelectionObjectType.None, SelectElementsInLink.Message, Prefix)
        {
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("transform", "the link instance transforms")));
        }

        [JsonConstructor]
        public SelectElementsInLink(IEnumerable<string> selectionIdentifier, IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) :
            base(SelectionType.Many, SelectionObjectType.None, SelectElementsInLink.Message, Prefix, selectionIdentifier, inPorts, outPorts)
        {

           
        }
    }

    
    [IsDesignScriptCompatible]
    [NodeCategory("Rhythm.Revit.Selection.RevitLinkSelection")]
    [NodeDescription("This allows you to select an element from a link of a given category. Useful for Dynamo player and Generative Design.")]
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

            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("transform", "the link instance transform")));
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

    [IsDesignScriptCompatible]
    [NodeCategory("Rhythm.Revit.Selection.RevitLinkSelection")]
    [NodeDescription("This allows you to select multiple elements from a link of a given category. Useful for Dynamo player and Generative Design.")]
    [NodeName("Select Elements from Link of Category")]
    public class SelectElementsInLinkOfCategory : ElementFilterSelection<Element>
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

        public SelectElementsInLinkOfCategory()
            : base(
                SelectionType.Many,
                SelectionObjectType.None,
                message,
                prefix)
        {
            DropDownNodeModel = new DSRevitNodesUI.Categories();
            SelectedIndex = DropDownNodeModel.SelectedIndex;
            SelectionFilter = new CategoryElementSelectionFilter<Element>();
            base.Filter = SelectionFilter;

            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("transform", "the link instance transform")));
        }

        [JsonConstructor]
        public SelectElementsInLinkOfCategory(IEnumerable<string> selectionIdentifier, IEnumerable<PortModel> inPorts,
            IEnumerable<PortModel> outPorts)
            : base(
                SelectionType.Many,
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
#if Revit2020 || Revit2021|| Revit2022
            ComboBox comboControl = new ComboBox { DataContext = this };
#endif
#if Revit2023
 var comboControl = new ComboControl { DataContext = this };
#endif

            nodeView.inputGrid.Children.Add(comboControl);
        }

        public void Dispose()
        {
        }
    }
    public class SelectElementsInLinkOfCategoryNodeViewCustomization : INodeViewCustomization<SelectElementsInLinkOfCategory>
    {
        public SelectElementsInLinkOfCategory Model { get; set; }
        public DelegateCommand SelectCommand { get; set; }

        public void CustomizeView(SelectElementsInLinkOfCategory model, NodeView nodeView)
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
#if Revit2020 || Revit2020 || Revit2021|| Revit2022
            ComboBox comboControl = new ComboBox { DataContext = this };
#endif
#if Revit2023
 var comboControl = new ComboControl { DataContext = this };
#endif
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
            AssociativeNode elementOutput = null;
            AssociativeNode transformOutput = null;
            Func<string, string,bool, bool, object> func = ElementSelection.InLinkDoc;

            var results = SelectionResults.ToList();

            if (SelectionResults == null || !results.Any())
            {
                elementOutput = AstFactory.BuildNullNode();
            }
            else if (results.Count == 1)
            {
                var el = results.First();

                elementOutput = AstFactory.BuildFunctionCall(
                    func,
                    new List<AssociativeNode>
                    {
                        AstFactory.BuildStringNode(el.Document.Title),
                        AstFactory.BuildStringNode(el.UniqueId),
                        AstFactory.BuildBooleanNode(true),
                        AstFactory.BuildBooleanNode(true)
                    });
                transformOutput = AstFactory.BuildFunctionCall(
                    func,
                    new List<AssociativeNode>
                    {
                        AstFactory.BuildStringNode(el.Document.Title),
                        AstFactory.BuildStringNode(el.UniqueId),
                        AstFactory.BuildBooleanNode(false),
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
                                    AstFactory.BuildBooleanNode(true),
                                    AstFactory.BuildBooleanNode(true)
                                })).ToList();
                var newInputs2 =
                    results.Select(
                        el =>
                            AstFactory.BuildFunctionCall(
                                func,
                                new List<AssociativeNode>
                                {
                                    AstFactory.BuildStringNode(el.Document.Title),
                                    AstFactory.BuildStringNode(el.UniqueId),
                                    AstFactory.BuildBooleanNode(false),
                                    AstFactory.BuildBooleanNode(true)
                                })).ToList();

                elementOutput = AstFactory.BuildExprList(newInputs);
                transformOutput = AstFactory.BuildExprList(newInputs2);
            }

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), elementOutput), AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(1), transformOutput) };
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
                    return RequestMultipleElementsSelection(selectionMessage, AsLogger());
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
        private IEnumerable<T> RequestMultipleElementsSelection(string selectionMessage, ILogger logger)
        {
            var doc = DocumentManager.Instance.CurrentUIDocument;
            Element e = null;
            List<Reference> elementRefs = null;
            var elements = new List<T>();

            //var choices = doc.Selection;
            //choices.SetElementIds(new Collection<ElementId>());

            if (ElementFilter != null)
            {
                elementRefs = doc.Selection.PickObjects(ObjectType.LinkedElement, ElementFilter, selectionMessage).ToList();

            }
            else
            {
                elementRefs = doc.Selection.PickObjects(ObjectType.LinkedElement, selectionMessage).ToList();
            }


            if (elementRefs == null || !elementRefs.Any())
                return null;


            foreach (var elementRef in elementRefs)
            {
                RevitLinkInstance link = DocumentManager.Instance.CurrentDBDocument.GetElement(elementRef) as RevitLinkInstance;
                e = link.GetLinkDocument().GetElement(elementRef.LinkedElementId);
                elements.Add((T)e);
            }

            return elements;
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

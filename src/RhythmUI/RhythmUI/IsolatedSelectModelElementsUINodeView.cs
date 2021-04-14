using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Controls;
using Dynamo.ViewModels;
using Dynamo.Wpf;
using ProtoCore.AST.AssociativeAST;

namespace RhythmUI
{
    class IsolatedSelectModelElementsUINodeView : INodeViewCustomization<IsolatedSelectModelElementsUI>
    {
        private DynamoViewModel dynamoViewModel;
        private IsolatedSelectModelElementsUI helloUiNode;

        public void CustomizeView(IsolatedSelectModelElementsUI model, NodeView nodeView)
        {
            dynamoViewModel = nodeView.ViewModel.DynamoViewModel;
            helloUiNode = model;
            var ui = new IsolatedSelectModelElements();
            nodeView.inputGrid.Children.Add(ui);
            ui.DataContext = model;

            ui.SelectButton.Click += delegate
            {
               model.OnNodeModified();
            };
        }
        
        public void Dispose()
        {
        }
    }
}
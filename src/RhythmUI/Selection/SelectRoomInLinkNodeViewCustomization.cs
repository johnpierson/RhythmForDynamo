
using System;
using Autodesk.Revit.DB;
using CoreNodeModelsWpf.Nodes;
using Dynamo.Applications.Models;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.Wpf;

namespace RhythmUI.Selection
{
    public class DSModelRoomInLinkSelectionNodeViewCustomization : SelectionBaseNodeViewCustomization<RevitLinkInstance, Element>, INodeViewCustomization<ElementSelection<RevitLinkInstance>>, IDisposable
    {
        public DSModelRoomInLinkSelectionNodeViewCustomization()
        {
        }

        public void CustomizeView(ElementSelection<RevitLinkInstance> model, NodeView nodeView)
        {
            base.CustomizeView(model,nodeView);
            model.RevitDynamoModel = nodeView.ViewModel.DynamoViewModel.Model as RevitDynamoModel;
        }
    }
}
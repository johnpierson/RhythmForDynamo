using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
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

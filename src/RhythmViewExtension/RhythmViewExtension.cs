using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using Xceed.Wpf.AvalonDock.Controls;

namespace RhythmViewExtension
{
    public class RhythmViewExtension : IViewExtension
    {
        private readonly string _executingLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;

        private static Version _currentVersion;
        private static readonly Version MinVersion = new Version(2, 6, 0, 8481);

        public void Dispose()
        {
        }
        public static DynamoView view;

        public void Startup(ViewStartupParams p)
        {
            //resolve the dynamo version by checking which core is loaded
            var dsCore = AppDomain.CurrentDomain.GetAssemblies().First(a => a.FullName.Contains("DSCoreNodes"));
            _currentVersion = dsCore.GetName().Version;
        }

        private ViewLoadedParams loaded = null;
        public void Loaded(ViewLoadedParams p)
        {
            loaded = p;
            view = p.DynamoWindow as DynamoView;
            
            p.CurrentWorkspaceChanged += POnCurrentWorkspaceChanged;
            p.CurrentWorkspaceModel.NodeAdded += CurrentWorkspaceModelOnNodeAdded;

            //if it is the version with the yellow labels, deactivate them for Rhythm.
            //TODO: research further on how this impacts node placement and search
            if (_currentVersion.CompareTo(MinVersion) <= 0)
            {
                p.DynamoWindow.LayoutUpdated += DynamoWindowOnLayoutUpdated;
            }
        }

        private void DynamoWindowOnLayoutUpdated(object sender, EventArgs e)
        {
            try
            {
                var nodeViews = loaded.DynamoWindow.FindVisualChildren<NodeView>();
                foreach (var nv in nodeViews)
                {
                    if (!nv.ViewModel.NodeModel.CreationName.Contains("Rhythm")) continue;
                    var border = nv.FindVisualChildren<Border>().First(b => b.CornerRadius.Equals(new CornerRadius(3)));
                    border.Width = 0;
                    border.Height = 0;
                }
            }
            catch (Exception)
            {
                // do nothing
            }
           
        }

        private void POnCurrentWorkspaceChanged(IWorkspaceModel obj)
        {
            obj.NodeAdded -= CurrentWorkspaceModelOnNodeAdded;
            obj.NodeAdded += CurrentWorkspaceModelOnNodeAdded;
        }

        private void CurrentWorkspaceModelOnNodeAdded(NodeModel obj)
        {
            string creationName = obj.CreationName;

            if (creationName.Contains("Rhythm") && !obj.Name.Contains("ʳʰʸᵗʰᵐ|"))
            {
                obj.Name = "ʳʰʸᵗʰᵐ|" + obj.Name;
            }
            if (creationName.Contains("CloseDocument"))
            {
                dynView.HomeSpace.RunSettings.RunType = RunType.Manual;
            }
        }

       
        public void Shutdown()
        {
        }
       
        public string UniqueId => "5435824A-A3A1-4FC1-AF42-E5139041740F";//NOTE: If you are building your own view extension, you MUST change this.

        public string Name => "Rhythm View Extension";//NOTE: If you are building your own view extension, you MUST change this.

        // ReSharper disable once InconsistentNaming
        public static DynamoViewModel dynView => view.DataContext as DynamoViewModel;

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}

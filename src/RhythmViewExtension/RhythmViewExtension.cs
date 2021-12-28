using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;

namespace RhythmViewExtension
{
    public class RhythmViewExtension : IViewExtension
    {
        private readonly string _executingLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;

        public void Dispose()
        {
        }
        public static DynamoView view;

        public void Startup(ViewStartupParams p)
        {
            
        }

        private ViewLoadedParams loaded = null;
        public void Loaded(ViewLoadedParams p)
        {

            //AddFlushMenu(p);

            ClearNotifications(p);
            loaded = p;
            view = p.DynamoWindow as DynamoView;

            p.CurrentWorkspaceChanged += POnCurrentWorkspaceChanged;
            p.CurrentWorkspaceModel.NodeAdded += CurrentWorkspaceModelOnNodeAdded;
        }

        //private void AddFlushMenu(ViewLoadedParams p)
        //{
        //    var flushBindingsMenu = new MenuItem { Header = "Flush Bindings", ToolTip = "this will flush the bindings out of your current dynamo file and allow you to create new elements." };
        //    flushBindingsMenu.Click += (sender, args) =>
        //    {
        //        //dynView.HomeSpace.RunSettings.RunType = RunType.Manual;
        //        dynView.HomeSpace.EngineController.LiveRunnerRuntimeCore.RuntimeData.CallsiteCache.Clear();

        //        var doc = DocumentManager.Instance.CurrentDBDocument;

        //        var lifecycleManager = RevitServices.Persistence.ElementIDLifecycleManager<int>.GetInstance();

        //        var ids = lifecycleManager.ToString().Split(new string[] {"Element ID ", ":"}, StringSplitOptions.None);

        //        foreach (var stringId in ids)
        //        {
        //            try
        //            {
        //                int intId = Convert.ToInt32(stringId.Trim());
        //                var currentElement = ElementSelector.ByElementId(intId, false);

        //                lifecycleManager.UnRegisterAssociation(intId, currentElement);
        //            }
        //            catch (Exception e)
        //            {
        //                //
        //            }
        //        }

        //    };
        //    p.AddMenuItem(MenuBarType.Help,flushBindingsMenu);
        //}

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

        private void ClearNotifications(ViewLoadedParams p)
        {
            try
            {
                foreach (MenuItem m in p.dynamoMenu.Items)
                {
                    if (m.Items.Count == 2)
                    {
                        foreach (MenuItem i in m.Items)
                        {
                            if (i.Header.ToString().Contains("Dismiss"))
                            {
                                m.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                                i.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //do nothing
            }

        }
    }
}

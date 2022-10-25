using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CoreNodeModels.Input;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using Directory = System.IO.Directory;

namespace RhythmViewExtension
{
    public class RhythmViewExtension : IViewExtension
    {
        public string UniqueId => "5435824A-A3A1-4FC1-AF42-E5139041740F";//NOTE: If you are building your own view extension, you MUST change this.
        public string Name => "Rhythm View Extension";//NOTE: If you are building your own view extension, you MUST change this.


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
            loaded = p;
            view = p.DynamoWindow as DynamoView;

            p.CurrentWorkspaceChanged += POnCurrentWorkspaceChanged;
            p.CurrentWorkspaceModel.NodeAdded += CurrentWorkspaceModelOnNodeAdded;
#if DEBUG
    MenuItem attachMenu = new MenuItem { Header = "attach" };
            attachMenu.Click += (sender, args) =>
            {
                var revitServices = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName.Contains("RevitServices"));
                IEnumerable<Type> types = GetTypesSafely(revitServices);

                var rhythmRevit = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName.Contains("RhythmRevit,"));
                Type t = rhythmRevit.GetType("Rhythm.Utilities.CommandHelpers");
                var methodInfo = t.GetMethod("GetView");
                var o = Activator.CreateInstance(t);

                foreach (Type objType in types)
                {
                    if (objType.IsClass)
                    {
                        if (objType.Name.Equals("DocumentManager"))
                        {
                            var constructor = objType.GetConstructors(
                                BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault();

                            object baseObject = constructor.Invoke(new object[] { });

                           
                            var view = methodInfo.Invoke(o, null);

                            object[] stuff = new[] { view };

                            objType.InvokeMember("HandleDocumentActivation",
                                BindingFlags.Default | BindingFlags.InvokeMethod, null, baseObject, stuff);
                        }
                    }
                }
            };

            p.AddExtensionMenuItem(attachMenu);         
#endif
        }
        private static IEnumerable<Type> GetTypesSafely(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(x => x != null);
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


        // ReSharper disable once InconsistentNaming
        public static DynamoViewModel dynView => view.DataContext as DynamoViewModel;

    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using CoreNodeModels.Input;
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
        public string UniqueId => "5435824A-A3A1-4FC1-AF42-E5139041740F";//NOTE: If you are building your own view extension, you MUST change this.
        public string Name => "Rhythm View Extension";//NOTE: If you are building your own view extension, you MUST change this.

        public void Dispose()
        {
        }
        public static DynamoView view;

        public void Startup(ViewStartupParams p)
        {
            var stuff = Global.PackageBinFolder;
            var otherStuff = Global.PackageExtraFolder;
        }

        private ViewLoadedParams loaded = null;
        public void Loaded(ViewLoadedParams p)
        {
            loaded = p;
            view = p.DynamoWindow as DynamoView;

            //Subscribe to node placed events for renaming stuff
            p.CurrentWorkspaceChanged += POnCurrentWorkspaceChanged;
            p.CurrentWorkspaceModel.NodeAdded += CurrentWorkspaceModelOnNodeAdded;


            //get revit version
            //find the DynamoRevit dll
            var revitApi = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name.Equals("RevitAPI"));

            if (revitApi != null)
            {
                var version = revitApi.GetName().Version.Major;

                FirstRunSetup(p, $"{version}");
            }

        }

        private void FirstRunSetup(ViewLoadedParams p, string version)
        {
            //first run setup. If this is the first install of Rhythm, load the correct Revit DLLs.
            if (!File.Exists(Global.RhythmRevitDll))
            {
                // Get resource name for our DLLs
                var revitResourceName = Global.EmbeddedRevitLibraries.FirstOrDefault(x => x.Contains(version));
                if (revitResourceName == null)
                {
                    return;
                }

                var revitUiResourceName = Global.EmbeddedRevitUiLibraries.FirstOrDefault(x => x.Contains(version));

                var vm = new RhythmMessageBoxViewModel
                {
                    IsCollapsed = false,
                    UserMessage = $"Loading correct Rhythm version for Revit 20{version}. Please wait...",
                    WrongVersionLoaded = false
                };

                RhythmMessageBox messageBox =
                    new RhythmMessageBox()
                    {
                        //Set the data context for the main grid in the window.
                        MainGrid = { DataContext = vm },
                        //Set the owner of the window to the Dynamo window.
                        Owner = p.DynamoWindow,
                    };

                messageBox.Show();

                //install and load the revit nodes
                using (var stream = Global.ExecutingAssembly.GetManifestResourceStream(revitResourceName))
                {
                    var bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);

                    File.WriteAllBytes(Global.RhythmRevitDll, bytes);
                }
                //install and load the revit ui nodes
                if (!string.IsNullOrWhiteSpace(revitUiResourceName))
                {
                    using var stream = Global.ExecutingAssembly.GetManifestResourceStream(revitResourceName);
                    var bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);

                    File.WriteAllBytes(Global.RhythmRevitUiDll, bytes);
                }

                //load the regular revit nodes
                var assembly = Assembly.Load(Global.RhythmRevitDll);
                p.ViewStartupParams.LibraryLoader.LoadNodeLibrary(assembly);

                //try to load the ui nodes
                var uiAssembly = Assembly.Load(Global.RhythmRevitUiDll);
                p.ViewStartupParams.LibraryLoader.LoadNodeLibrary(uiAssembly);

                //rewrite the json
                File.WriteAllText(Global.PackageJson, Global.PackageJsonText);

                messageBox.Close();
            }
            //we get here for compatibility check
            else
            {
                FileVersionInfo fileInfo = FileVersionInfo.GetVersionInfo(Global.RhythmRevitDll);

                if (!fileInfo.FileDescription.EndsWith(version))
                {

                    var vm = new RhythmMessageBoxViewModel()
                    {
                        IsCollapsed = false,
                        UserMessage = $"Incompatible Rhythm version loaded. You have {fileInfo.FileDescription} loaded. " +
                                      $"You should reinstall from the package manager. Also, you made the dog sad. You monster. " +
                                      $"For more info, click the question mark button.",
                        WrongVersionLoaded = true
                    };
                    RhythmMessageBox messageBox =
                        new RhythmMessageBox()
                        {
                            // Set the data context for the main grid in the window.
                            MainGrid = { DataContext = vm },
                            // Set the owner of the window to the Dynamo window.
                            Owner = p.DynamoWindow,
                        };

                    messageBox.Show();

                    //rewrite the json without the DynamoRevit libraries being loaded TODO: Decide if it is too cruel to unload the DLL like this.
                    //File.WriteAllText(Global.PackageJson, Global.PackageJsonTextWithoutRevitNodes);
                }
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
            if (creationName.Contains("CloseDocument") || creationName.Contains("UpgradeFamilies"))
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

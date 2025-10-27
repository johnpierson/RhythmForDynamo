using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using ProtoCore.AST.ImperativeAST;
using RhythmViewExtension.Utilities;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

namespace RhythmViewExtension
{
    public class RhythmViewExtension : IViewExtension
    {
        public string UniqueId => "5435824A-A3A1-4FC1-AF42-E5139041740F";//NOTE: If you are building your own view extension, you MUST change this.
        public string Name => "Rhythm View Extension";//NOTE: If you are building your own view extension, you MUST change this.

        internal string GitHubUrl => "https://raw.githubusercontent.com/johnpierson/RhythmForDynamo/master/deploy/20";

        public void Dispose()
        {
        }
        public static DynamoView view;

        public void Startup(ViewStartupParams p)
        {
            //var stuff = Global.PackageBinFolder;
            //var otherStuff = Global.PackageExtraFolder;
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
            //just load the core nodes, the user isn't in Revit
            else
            {
                var dynamoCore = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName.Contains("DynamoCore"));

                if (dynamoCore != null)
                {
                    var dynamoCoreVersion = dynamoCore.GetName().Version;
                    string dynamoMajorMinor = $"{dynamoCoreVersion.Major}.{dynamoCoreVersion.Minor}";

                    var revitVersion = VersionUtils.GetRevitYearsForDynamo(dynamoMajorMinor).First().ToString();
                    //core nodes
                    LoadCoreNodes(p, revitVersion);
                }
                
            }
        }

        private void FirstRunSetup(ViewLoadedParams p, string version)
        {
            //first run setup. If this is the first install of Rhythm, load the correct DLLs.
            if (!File.Exists(Global.RhythmRevitDll))
            {
                var vm = new RhythmMessageBoxViewModel
                {
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

                //core nodes
                LoadCoreNodes(p,version);

                //download the latest dlls related to that Revit version
                DownloadFile(version, Global.RhythmRevitDll);
                DownloadFile(version, Global.RhythmRevitXml);
                DownloadFile(version, Global.RhythmRevitCustomizationXml);
                DownloadFile(version, Global.RhythmRevitCustomizationDll);

                //next the ui revit nodes
                DownloadFile(version, Global.RhythmRevitUiDll);
                DownloadFile(version, Global.RhythmRevitUiXml);

                //download supplemental DLLs and correct pkg.json
                DownloadFile(version,Global.HumanizerDll);
                DownloadFile(version, Global.MarkovDll);
                DownloadFile(string.Empty, Global.PackageJson);



                //load the regular revit nodes
                try
                {
                    var assembly = Assembly.LoadFrom(Global.RhythmRevitDll);
                    p.ViewStartupParams.LibraryLoader.LoadNodeLibrary(assembly);
                }
                catch (Exception e)
                {
                    //
                }

                //rewrite the json
                string jsonDLLUrl =
                    $"https://raw.githubusercontent.com/johnpierson/RhythmForDynamo/master/deploy/pkg.json";
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("a", "a");
                    try
                    {
                        wc.DownloadFile(jsonDLLUrl, Global.PackageJson);
                    }
                    catch (Exception ex)
                    {
                        //
                    }
                }

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
                }
            }
        }

        internal void LoadCoreNodes(ViewLoadedParams p, string version)
        {
            //download the latest core nodes
            DownloadFile(version, Global.RhythmCoreDll);

            //now the appropriate XMLs
            DownloadFile(version, Global.RhythmCoreXml);
            DownloadFile(version,Global.RhythmCoreCustomizationXml);
            DownloadFile(version, Global.RhythmCoreCustomizationDll);

            //download supplemental DLLs and correct pkg.json
            DownloadFile(version, Global.HumanizerDll);
            DownloadFile(version, Global.MarkovDll);
            DownloadFile(string.Empty, Global.PackageJson);


            //load the core nodes
            try
            {
                var assembly = Assembly.LoadFrom(Global.RhythmCoreDll);
                p.ViewStartupParams.LibraryLoader.LoadNodeLibrary(assembly);
            }
            catch (Exception e)
            {
                //
            }

        }

        internal void DownloadFile(string version, string fileLocation)
        {
            FileInfo fileInfo = new FileInfo(fileLocation);

            string fileName = fileInfo.Name;

            var url = string.IsNullOrWhiteSpace(version) ? $"{GitHubUrl}/{fileName}" : $"{GitHubUrl}{version}/{fileName}";

            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add("a", "a");
                try
                {
                    wc.DownloadFile(url, fileLocation);
                }
                catch (Exception ex)
                {
                    //
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

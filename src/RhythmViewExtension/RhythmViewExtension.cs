using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Newtonsoft.Json;
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

        /// <summary>
        /// WebClient's default is 100 seconds per request. Startup makes a dozen or more requests
        /// in a row, so a proxy that black-holes traffic used to be able to hold the Dynamo window
        /// for over twenty minutes. Fifteen seconds is generous for files of this size.
        /// </summary>
        private const int DownloadTimeoutMilliseconds = 15000;

        /// <summary>
        /// WebClient exposes no timeout, so the only way to set one is to override the request.
        /// </summary>
        private sealed class TimeoutWebClient : WebClient
        {
            private readonly int _timeoutMilliseconds;

            internal TimeoutWebClient(int timeoutMilliseconds)
            {
                _timeoutMilliseconds = timeoutMilliseconds;
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                var request = base.GetWebRequest(address);
                if (request != null)
                {
                    request.Timeout = _timeoutMilliseconds;
                }
                return request;
            }
        }

        /// <summary>
        /// Every download failure during this startup, so the user gets one honest report instead
        /// of a package that silently has no nodes in it.
        /// </summary>
        private readonly List<string> _failures = new List<string>();

        private void RecordFailure(string fileName, string url, Exception ex)
        {
            _failures.Add($"{fileName} ({url}): {ex.Message}");
            LogMessage($"Rhythm: failed to download {fileName} from {url} - {ex.Message}");
        }

        private void LogMessage(string message)
        {
            try
            {
                dynView?.Model?.Logger?.Log(message);
            }
            catch (Exception)
            {
                // Logging must never be the reason startup fails.
            }
        }

        /// <summary>
        /// Shows one dialog listing everything that could not be downloaded. Without this, a
        /// blocked or offline first run produced an installed package with no Revit nodes and no
        /// indication of why.
        /// </summary>
        private void ReportFailuresIfAny(ViewLoadedParams p)
        {
            if (_failures.Count == 0)
            {
                return;
            }

            var vm = new RhythmMessageBoxViewModel
            {
                UserMessage =
                    "Rhythm could not download part of itself, so some nodes will be missing. This is usually a firewall or proxy blocking raw.githubusercontent.com. " +
                    "Restart Dynamo once you have a connection and Rhythm will finish installing.\n\n" +
                    string.Join("\n", _failures),
                WrongVersionLoaded = true
            };

            new RhythmMessageBox
            {
                MainGrid = { DataContext = vm },
                Owner = p.DynamoWindow,
            }.Show();

            _failures.Clear();
        }

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
            RemoveCustomizationDllsIfNeeded(version);
            //first run setup. If this is the first install of Rhythm, load the correct DLLs.
            //A zero-byte or unreadable file counts as "not installed": a download that failed
            //partway used to leave one behind, and because File.Exists then returned true the
            //download never retried, leaving the install permanently and silently broken.
            if (!IsUsableAssembly(Global.RhythmRevitDll))
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

                //download the latest dlls related to that Revit version
                DownloadFile(version, Global.RhythmRevitDll);
                DownloadFile(version, Global.RhythmRevitXml);
                DownloadFile(version, Global.RhythmRevitCustomizationXml);
                DownloadCustomizationDllIfSupported(version, Global.RhythmRevitCustomizationDll);

                //next the ui revit nodes
                DownloadFile(version, Global.RhythmRevitUiDll);
                DownloadFile(version, Global.RhythmRevitUiXml);

                //core nodes
                LoadCoreNodes(p, version);

                //load the regular revit nodes
                try
                {
                    var assembly = Assembly.LoadFrom(Global.RhythmRevitDll);
                    p.ViewStartupParams.LibraryLoader.LoadNodeLibrary(assembly);
                }
                catch (Exception e)
                {
                    RecordFailure("RhythmRevit.dll", Global.RhythmRevitDll, e);
                }

                messageBox.Close();
                ReportFailuresIfAny(p);
            }
            //we get here for compatibility check
            else
            {
                FileVersionInfo fileInfo = FileVersionInfo.GetVersionInfo(Global.RhythmRevitDll);

                //FileDescription is null for a file with no version resource. Calling EndsWith on it
                //threw NullReferenceException out of Loaded(), before the advisory dialog below
                //could ever be shown.
                if (fileInfo.FileDescription == null || !fileInfo.FileDescription.EndsWith(version))
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
            RemoveCustomizationDllsIfNeeded(version);
            //download the latest core nodes
            DownloadFile(version, Global.RhythmCoreDll);

            //now the appropriate XMLs
            DownloadFile(version, Global.RhythmCoreXml);
            DownloadFile(version,Global.RhythmCoreCustomizationXml);
            DownloadCustomizationDllIfSupported(version, Global.RhythmCoreCustomizationDll);

            //download supplemental DLLs
            DownloadFile(version, Global.HumanizerDll);
            DownloadFile(version, Global.MarkovDll);

            //The manifest lives at deploy/pkg.json, not under a per-year folder. Passing an empty
            //version here built the URL ".../deploy/20/pkg.json", a path that has never existed,
            //so the request 404'd on every run - and because the download wrote straight to the
            //destination, it destroyed the installed manifest each time before the real fetch
            //below restored it. Offline, nothing restored it and the package stopped loading.
            DownloadPackageJson();

            //load the core nodes
            try
            {
                var assembly = Assembly.LoadFrom(Global.RhythmCoreDll);
                p.ViewStartupParams.LibraryLoader.LoadNodeLibrary(assembly);
            }
            catch (Exception e)
            {
                RecordFailure("RhythmCore.dll", Global.RhythmCoreDll, e);
            }

            ReportFailuresIfAny(p);
        }

        /// <summary>
        /// Refreshes pkg.json from the repository, writing it only if the download succeeds and
        /// parses as JSON. This file is what Dynamo reads to load the package at all, so a
        /// half-written or empty one takes Rhythm out of service until the user reinstalls.
        /// </summary>
        private void DownloadPackageJson()
        {
            const string url = "https://raw.githubusercontent.com/johnpierson/RhythmForDynamo/master/deploy/pkg.json";
            string tempFile = Global.PackageJson + ".download";

            try
            {
                using (WebClient wc = new TimeoutWebClient(DownloadTimeoutMilliseconds))
                {
                    wc.DownloadFile(url, tempFile);
                }

                //Parse before replacing: a captive portal or proxy error page returns HTTP 200 with
                //an HTML body, which would otherwise be written over a perfectly good manifest.
                JsonConvert.DeserializeObject(File.ReadAllText(tempFile));

                File.Copy(tempFile, Global.PackageJson, true);
            }
            catch (Exception ex)
            {
                RecordFailure("pkg.json", url, ex);
            }
            finally
            {
                try
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
                catch (IOException)
                {
                    // Harmless leftover.
                }
            }
        }

        /// <summary>
        /// True when the file exists and is large enough to be a real assembly. Guards against the
        /// zero-byte and truncated files that a failed download used to leave behind.
        /// </summary>
        private static bool IsUsableAssembly(string path)
        {
            try
            {
                var info = new FileInfo(path);
                return info.Exists && info.Length > 0;
            }
            catch (IOException)
            {
                return false;
            }
        }

        /// <summary>
        /// Downloads one file, replacing the destination only once the transfer has fully
        /// succeeded. Returns false and records the reason on failure.
        /// </summary>
        /// <remarks>
        /// WebClient.DownloadFile opens the destination with FileMode.Create *before* it issues the
        /// request, and deletes it if the request fails. Writing straight to the destination
        /// therefore destroyed a good file whenever the network was unavailable - which is how an
        /// offline start could leave the package with no pkg.json and no way to repair itself.
        /// Downloading to a temporary file and moving it into place makes a failed download a
        /// no-op instead.
        /// </remarks>
        internal bool DownloadFile(string version, string fileLocation)
        {
            FileInfo fileInfo = new FileInfo(fileLocation);

            string fileName = fileInfo.Name;

            var url = string.IsNullOrWhiteSpace(version) ? $"{GitHubUrl}/{fileName}" : $"{GitHubUrl}{version}/{fileName}";

            string tempFile = fileLocation + ".download";

            try
            {
                using (WebClient wc = new TimeoutWebClient(DownloadTimeoutMilliseconds))
                {
                    wc.DownloadFile(url, tempFile);
                }

                if (new FileInfo(tempFile).Length == 0)
                {
                    throw new IOException($"{url} returned an empty file.");
                }

                // File.Copy over the destination rather than File.Move, because the destination may
                // be an assembly Dynamo has already loaded and locked. A failure here leaves the
                // existing file untouched, which is the safe outcome.
                File.Copy(tempFile, fileLocation, true);
                return true;
            }
            catch (Exception ex)
            {
                RecordFailure(fileName, url, ex);
                return false;
            }
            finally
            {
                try
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
                catch (IOException)
                {
                    // A leftover .download file is harmless; it is overwritten next time.
                }
            }
        }

        private void DownloadCustomizationDllIfSupported(string version, string fileLocation)
        {
            if (ShouldSkipCustomizationDll(version))
            {
                return;
            }

            DownloadFile(version, fileLocation);
        }

        private static void RemoveCustomizationDllsIfNeeded(string version)
        {
            if (!ShouldSkipCustomizationDll(version))
            {
                return;
            }

            DeleteIfExists(Global.RhythmCoreCustomizationDll);
            DeleteIfExists(Global.RhythmRevitCustomizationDll);
        }

        private static bool ShouldSkipCustomizationDll(string version)
        {
            if (!int.TryParse(version, out var revitVersion))
            {
                return false;
            }

            // Revit sessions pass the assembly major version (27), while the core-only fallback path passes the release year (2027).
            return revitVersion >= 2027 || (revitVersion >= 27 && revitVersion < 100);
        }

        private static void DeleteIfExists(string fileLocation)
        {
            if (File.Exists(fileLocation))
            {
                File.Delete(fileLocation);
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
            string nodeDescription = obj.Description;

            if (creationName.Contains("Rhythm") && !obj.Name.Contains("ʳʰʸᵗʰᵐ|"))
            {
                obj.Name = "ʳʰʸᵗʰᵐ|" + obj.Name;
            }
            if (creationName.Contains("CloseDocument") || creationName.Contains("UpgradeFamilies") || nodeDescription.Contains("Manual Run Mode"))
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

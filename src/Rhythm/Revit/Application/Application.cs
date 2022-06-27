using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using Rhythm.Utilities;

namespace Rhythm.Revit.Application
{
    /// <summary>
    /// Wrapper class for application level nodes.
    /// </summary>
    public class Applications
    {
        private Applications()
        { }

        /// <summary>
        /// This node will open the given file in the background.
        /// </summary>
        /// <param name="filePath">The file to obtain document from.</param>
        /// <param name="audit">Choose whether or not to audit the file upon opening. (Will run slower with this)</param>
        /// <param name="detachFromCentral">Choose whether or not to detach from central upon opening. Only for RVT files. </param>
        /// <param name="preserveWorksets">Choose whether or not to preserve worksets upon opening. Only for RVT files. </param>
        /// <param name="closeAllWorksets">Choose if you want to close all worksets upon opening. Defaulted to false.</param>
        /// <returns name="document">The document object. If the file path is blank this returns the current document.</returns>
        /// <search>
        /// Application.OpenDocumentFile, rhythm
        /// </search>
        [NodeCategory("Create")]
        public static global::Revit.Application.Document OpenDocumentFile(string filePath, bool audit = false, bool detachFromCentral = false, bool preserveWorksets = true, bool closeAllWorksets = false)
        {
            var uiapp = DocumentManager.Instance.CurrentUIApplication;
            var app = uiapp.Application;
            //instantiate open options for user to pick to audit or not
            OpenOptions openOpts = new OpenOptions
            {
                Audit = audit,
                DetachFromCentralOption = detachFromCentral == false ? DetachFromCentralOption.DoNotDetach :
                    preserveWorksets == true ? DetachFromCentralOption.DetachAndPreserveWorksets :
                    DetachFromCentralOption.DetachAndDiscardWorksets
            };
            //TransmittedModelOptions tOpt = TransmittedModelOptions.SaveAsNewCentral;
            //option to close all worksets
            WorksetConfiguration worksetConfiguration = new WorksetConfiguration(WorksetConfigurationOption.OpenAllWorksets);
            if (closeAllWorksets)
            {
                worksetConfiguration = new WorksetConfiguration(WorksetConfigurationOption.CloseAllWorksets);
            }
            openOpts.SetOpenWorksetsConfiguration(worksetConfiguration);

            //convert string to model path for open
            ModelPath modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath);

            var document = app.OpenDocumentFile(modelPath, openOpts);

            return document.ToDynamoType();
        }

        /// <summary>
        /// This node will close the given document with the option to save.
        /// </summary>
        /// <param name="document">The background opened document object, (preferably this is the title as obtained with Applications.OpenDocumentFile from Rhythm).</param>
        /// <param name="save">Do you want to save?</param>
        /// <returns name="result">Did it work?</returns>
        /// <search>
        /// Application.CloseDocument, rhythm
        /// </search>
        [NodeCategory("Actions")]
        //[Obsolete("This node will be completely removed in future versions of Rhythm")]
        public static string CloseDocument(object document, bool save)
        {
            Document dbDoc = null;

            if (document is global::Revit.Application.Document dynamoDoc)
            {
                dbDoc = dynamoDoc.ToRevitType();
            }
            else
            {
                dbDoc = document as Document;
            }

            try
            {
                dbDoc.Close(save);
                dbDoc.Dispose();
                return "closed";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        /// <summary>
        /// This will try to open a file in the current version with various options.
        /// </summary>
        /// <param name="filePath">The file to obtain document from.</param>
        /// <param name="audit">Choose whether or not to audit the file upon opening. (Will run slower with this)</param>
        /// <param name="detachFromCentral">Choose whether or not to detach from central upon opening. Only for RVT files. </param>
        /// <param name="preserveWorksets">Choose whether or not to preserve worksets upon opening. Only for RVT files. </param>
        /// <param name="closeAllWorksets">Choose if you want to close all worksets upon opening. Defaulted to false.</param>
        /// <param name="unloadAllLinks">Choose if you want unload all links?</param>
        /// <returns name="result">Did it work?</returns>
        public static string UpgradeFile(string filePath, bool audit = false, bool detachFromCentral = false, bool preserveWorksets = true, bool closeAllWorksets = false, bool unloadAllLinks = false)
        {
            var uiapp = DocumentManager.Instance.CurrentUIApplication;
            var app = uiapp.Application;

            app.FailuresProcessing += AppOnFailuresProcessing;

            //instantiate open options for user to pick to audit or not
            OpenOptions openOpts = new OpenOptions
            {
                Audit = audit,
                DetachFromCentralOption = detachFromCentral == false ? DetachFromCentralOption.DoNotDetach :
                    preserveWorksets == true ? DetachFromCentralOption.DetachAndPreserveWorksets :
                    DetachFromCentralOption.DetachAndDiscardWorksets
            };
            //TransmittedModelOptions tOpt = TransmittedModelOptions.SaveAsNewCentral;
            //option to close all worksets
            WorksetConfiguration worksetConfiguration = new WorksetConfiguration(WorksetConfigurationOption.OpenAllWorksets);
            if (closeAllWorksets)
            {
                worksetConfiguration = new WorksetConfiguration(WorksetConfigurationOption.CloseAllWorksets);
            }
            openOpts.SetOpenWorksetsConfiguration(worksetConfiguration);

            //unload all links if that is desired
            if (unloadAllLinks)
            {
                UnloadRevitLinks(filePath);
            }


            //convert string to model path for open
            ModelPath modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath);

            var document = app.OpenDocumentFile(modelPath, openOpts);
            var workshared = document.IsWorkshared;
            try
            {
                app.FailuresProcessing -= AppOnFailuresProcessing;
                //do a save as
                SaveAsOptions opts = new SaveAsOptions();
                if (workshared)
                {
                    var worksharingOptions = new WorksharingSaveAsOptions { SaveAsCentral = true };
                    opts.SetWorksharingOptions(worksharingOptions);
                }
                
                FileInfo originalInfo = new FileInfo(filePath);
                string newPath = Path.Combine(originalInfo.DirectoryName,
                    DocumentManager.Instance.CurrentUIApplication.Application.VersionNumber);

                Directory.CreateDirectory(newPath);

                ModelPath newModelPath = new FilePath(Path.Combine(newPath,originalInfo.Name));
                document.SaveAs(newModelPath, opts);

                document.Dispose();

                return "Upgraded, saved and closed";
            }
            catch (Exception e)
            {
                app.FailuresProcessing -= AppOnFailuresProcessing;
                return e.Message;
            }
        }

        private static void AppOnFailuresProcessing(object sender, FailuresProcessingEventArgs e)
        {
          e.GetFailuresAccessor().DeleteAllWarnings();
        }

        /// <summary>
        /// This node provides access to all of the open documents in revit.
        /// </summary>
        /// <param name="runIt">Do you want to save?</param>
        /// <returns name="documents">The documents that are currently open.</returns>
        [NodeCategory("Query")]
        //[Obsolete("This node will be completely removed in future versions of Rhythm")]
        public static List<global::Revit.Application.Document> GetOpenDocuments(bool runIt)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            List<Autodesk.Revit.DB.Document> documents = new List<Autodesk.Revit.DB.Document>();

            var uiApp = DocumentManager.Instance.CurrentUIApplication;
            var app = uiApp.Application;

            foreach (Document d in app.Documents)
            {
                if (d.Title == doc.Title) continue;
                try
                {
                    documents.Add(d);
                }
                catch (Exception)
                {
                    //nothing
                }
            }
            return documents.Select(d => d.ToDynamoType()).ToList();
        }

        /// <summary>
        /// Unload revit links for given file path.
        /// </summary>
        /// <param name="modelPath">The path to the Revit file</param>
        /// <returns name="success">Was it successful?></returns>
        public static bool UnloadRevitLinks(string modelPath)
        {
            try
            {
                ModelPath mPath = new FilePath(modelPath);
                TransmissionData tData = TransmissionData.ReadTransmissionData(mPath);

                // collect all (immediate) external references in the model
                ICollection<ElementId> externalReferences = tData.GetAllExternalFileReferenceIds();

                // find every reference that is a link
                foreach (ElementId refId in externalReferences)
                {
                    ExternalFileReference extRef = tData.GetLastSavedReferenceData(refId);

                    if (extRef.ExternalFileReferenceType == ExternalFileReferenceType.RevitLink)
                    {
                        // we do not want to change neither the path nor the path-type
                        // we only want the links to be unloaded (shouldLoad = false)
                        tData.SetDesiredReferenceData(refId, extRef.GetPath(), extRef.PathType, false);
                    }
                }

                // make sure the IsTransmitted property is set 
                tData.IsTransmitted = true;

                // modified transmission data must be saved back to the model
                TransmissionData.WriteTransmissionData(mPath, tData);

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
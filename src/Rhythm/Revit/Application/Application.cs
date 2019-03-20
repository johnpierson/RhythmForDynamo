using System;
using System.Collections.Generic;
using System.Reflection;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Dynamo.Applications;
using Dynamo.Applications.ViewModel;
using Dynamo.Controls;
using Dynamo.Graph;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.ViewModels;
using RevitServices.Persistence;
using Rhythm.Utilities;
using Object = DSCore.Object;

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
        /// <returns name="document">The document object. Primarily for use with other Rhythm nodes.</returns>
        /// <search>
        /// Application.OpenDocumentFile, rhythm
        /// </search>
        public static string OpenDocumentFile(string filePath, bool audit = false, bool detachFromCentral = false)
        {
            var uiapp = DocumentManager.Instance.CurrentUIApplication;
            var app = uiapp.Application;
            Document doc;
            string docTitle = string.Empty;
            //instantiate open options for user to pick to audit or not
            OpenOptions openOpts = new OpenOptions();
            openOpts.Audit = audit;
            TransmittedModelOptions tOpt = TransmittedModelOptions.SaveAsNewCentral;
            if (detachFromCentral == false)
            {
                openOpts.DetachFromCentralOption = DetachFromCentralOption.DoNotDetach;
            }
            else
            {
                openOpts.DetachFromCentralOption = DetachFromCentralOption.DetachAndPreserveWorksets;
            }
            
            //convert string to model path for open
            ModelPath modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath);

            try
            {
                docTitle = DocumentUtils.OpenDocument(modelPath, openOpts);
            }
            catch (Exception)
            {
                //nothing
            }

            return docTitle;
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
        public static string CloseDocument(object document, bool save)
        {
            Document doc = DocumentUtils.RetrieveDocument(document);

            string result;
            try
            {
                doc.Close(save);
                result = "closed";
            }
            catch (Exception)
            {
                result = "Error, it appears this file cannot be found in the background opened document list.";
            }

            return result;
        }
        /// <summary>
        /// This node provides access to all of the open documents in revit.
        /// </summary>
        /// <param name="runIt">Do you want to save?</param>
        /// <returns name="documents">The documents that are currently open.</returns>
        public static List<string> GetOpenDocuments(bool runIt)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            List<string> docNames = new List<string>();

            var uiApp = DocumentManager.Instance.CurrentUIApplication;
            var app = uiApp.Application;

            foreach (Document d in app.Documents)
            {
                if (d.Title != doc.Title)
                {
                    try
                    {
                        docNames.Add(DocumentUtils.ConvertDocument(doc));
                    }
                    catch (Exception)
                    {
                        //nothing
                    }
                }
            }

            return docNames;
        }
    }
}

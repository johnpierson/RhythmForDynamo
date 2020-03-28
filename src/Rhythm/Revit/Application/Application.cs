using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Dynamo.Applications;
using Dynamo.Applications.ViewModel;
using RevitServices.Persistence;

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
        /// <returns name="document">The document object. If the file path is blank this returns the current document.</returns>
        /// <search>
        /// Application.OpenDocumentFile, rhythm
        /// </search>
        public static object OpenDocumentFile(string filePath, bool audit = false, bool detachFromCentral = false, bool preserveWorksets = true)
        {
            var uiapp = DocumentManager.Instance.CurrentUIApplication;
            var app = uiapp.Application;
            //instantiate open options for user to pick to audit or not
            OpenOptions openOpts = new OpenOptions();
            openOpts.Audit = audit;
            //TransmittedModelOptions tOpt = TransmittedModelOptions.SaveAsNewCentral;
            openOpts.DetachFromCentralOption = detachFromCentral == false ? DetachFromCentralOption.DoNotDetach : preserveWorksets == true ? DetachFromCentralOption.DetachAndPreserveWorksets : DetachFromCentralOption.DetachAndDiscardWorksets;

            //convert string to model path for open
            ModelPath modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath);

            var document = app.OpenDocumentFile(modelPath, openOpts);

            return document;
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
        public static string CloseDocument(Autodesk.Revit.DB.Document document, bool save)
        {

            string result;
            try
            {

                document.Close(save);
                result = "closed";
            }
            catch (Exception)
            {
                result = "can't close the file. It has either been closed already or another weird error.";
            }

            return result;
        }
        /// <summary>
        /// This node provides access to all of the open documents in revit.
        /// </summary>
        /// <param name="runIt">Do you want to save?</param>
        /// <returns name="documents">The documents that are currently open.</returns>
        public static List<Autodesk.Revit.DB.Document> GetOpenDocuments(bool runIt)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            List<Autodesk.Revit.DB.Document> documents = new List<Autodesk.Revit.DB.Document>();

            var uiApp = DocumentManager.Instance.CurrentUIApplication;
            var app = uiApp.Application;

            foreach (Document d in app.Documents)
            {
                if (d.Title != doc.Title)
                {
                    try
                    {
                        documents.Add(d);
                    }
                    catch (Exception)
                    {
                        //nothing
                    }
                }
            }

            return documents;
        }


    }
}

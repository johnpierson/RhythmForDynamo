using Autodesk.Revit.DB;
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
        /// <returns name="document">The document.</returns>
        /// <search>
        /// Application.OpenDocumentFile, rhythm
        /// </search>
        public static Autodesk.Revit.DB.Document OpenDocumentFile(string filePath, bool audit = false, bool detachFromCentral = false)
        {
            var uiapp = DocumentManager.Instance.CurrentUIApplication;
            var app = uiapp.Application;
            //declare open options for user to pick to audit or not
            OpenOptions openOpts = new OpenOptions();
            openOpts.Audit = audit;
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

            return app.OpenDocumentFile(modelPath, openOpts);
        }     

        /// <summary>
        /// This node will close the given document with the option to save.
        /// </summary>
        /// <param name="document">The document to close.</param>
        /// <param name="save">Do you want to save?</param>
        /// <returns name="result">Did it work?</returns>
        /// <search>
        /// Application.CloseDocument, rhythm
        /// </search>
        public static bool CloseDocument(Autodesk.Revit.DB.Document document,bool save)
        {
            var result = document.Close(save);

            return result;
        }
    }
}

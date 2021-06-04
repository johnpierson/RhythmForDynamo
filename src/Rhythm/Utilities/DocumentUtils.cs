using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;
using RevitServices.Persistence;

namespace Rhythm.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public class DocumentUtils
    {
        private DocumentUtils()
        {
        }

        /// <summary>
        /// This converts orchid documents to Revit DB Documents
        /// </summary>
        /// <param name="orchidDocument">The Orchid document to convert to a Autodesk.Revit.DB.Document.</param>
        /// <returns name="dbDocument">The Autodesk.Revit.DB.Document</returns>
        public static Autodesk.Revit.DB.Document OrchidDocumentToDbDocument(object orchidDocument)
        {
            //find the orchid assembly
            Assembly sourceAssembly = Assembly.GetAssembly(orchidDocument.GetType());
            Type type = sourceAssembly.GetType("Orchid.RevitProject.Common.Document");
            //find the path of the document
            string path =  type.InvokeMember("Path", BindingFlags.Default | BindingFlags.InvokeMethod, null, null,
                new object[] { orchidDocument, true }).ToString();

            //revit db document
            Autodesk.Revit.DB.Document doc = null;

            foreach (Autodesk.Revit.DB.Document d in DocumentManager.Instance.CurrentUIApplication.Application.Documents)
            {
                if (d.PathName.Equals(path))
                {
                    doc = d;
                }
            }
            return doc;
        }
        /// <summary>
        /// This converts Revit DB Documents to orchid documents.
        /// </summary>
        /// <param name="dbDocument">The Autodesk.Revit.DB.Document to convert to Orchid Document</param>
        /// <returns name="orchidDocument">The Orchid document</returns>
        public static object DbDocumentToOrchidDocument(Autodesk.Revit.DB.Document dbDocument)
        {

            //find the orchid assembly
            var assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.FullName.Contains("OrchidRB") && !a.FullName.Contains("customization"));
            Type type = assembly.GetType("Orchid.RevitProject.Common.Document");
            //find the path of the document
            return type.InvokeMember("BackgroundOpen", BindingFlags.Default | BindingFlags.InvokeMethod, null, null,
                new object[] { dbDocument.PathName});
        }
    }
}

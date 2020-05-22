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
        /// <param name="docObject"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static Autodesk.Revit.DB.Document OrchidDocumentToDbDocument(object docObject)
        {
            //find the orchid assembly
            Assembly sourceAssembly = Assembly.GetAssembly(docObject.GetType());
            Type type = sourceAssembly.GetType("Orchid.RevitProject.Common.Document");
            //find the path of the document
            string path =  type.InvokeMember("Path", BindingFlags.Default | BindingFlags.InvokeMethod, null, null,
                new object[] { docObject, true }).ToString();

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
        /// <param name="document"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static object DbDocumentToOrchidDocument(Autodesk.Revit.DB.Document document)
        {

            //find the orchid assembly
            var assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.FullName.Contains("OrchidRB") && !a.FullName.Contains("customization"));
            Type type = assembly.GetType("Orchid.RevitProject.Common.Document");
            //find the path of the document
            return type.InvokeMember("BackgroundOpen", BindingFlags.Default | BindingFlags.InvokeMethod, null, null,
                new object[] { document.PathName});

           
        }
    }
}

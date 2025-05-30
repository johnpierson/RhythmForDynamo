using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Fabrication;
using RevitServices.Persistence;

namespace Rhythm.Revit.Fabrication
{
    public class Fabrication
    {
        private Fabrication() { }

        /// <summary>
        /// <summary>  
        /// This node exports a list of fabrication elements to a PCF (Piping Component File) format.  
        /// It filters the provided elements to include only FabricationPart instances and then uses Revit's FabricationUtils  
        /// to perform the export operation.  
        /// </summary>  
        /// <param name="fileName">The full path of the PCF file to export to.</param>  
        /// <param name="elements">A list of Revit elements to be exported. Only FabricationPart elements are considered.</param>  
        /// <returns name="resultMessage">A message indicating the success or failure of the export operation.</returns>
        public static string ExportToPCF(string fileName, List<global::Revit.Elements.Element>elements)
        {
            var currentDocument = DocumentManager.Instance.CurrentDBDocument;

            List<ElementId> elementIds = new List<ElementId>();

            foreach (var element in elements)
            {
                if (element.InternalElement is FabricationPart fabricationPart)
                {
                    elementIds.Add(fabricationPart.Id);
                }
            }

            try
            {
                FabricationUtils.ExportToPCF(currentDocument, elementIds, fileName);

                return $"Exported {elementIds.Count} elements {fileName} successfully.";
            }
            catch (Exception e)
            {
                return $"Failed to export elements to PCF: {e.Message}";
                throw;
            }
        }

    }
}

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

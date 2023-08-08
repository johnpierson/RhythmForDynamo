using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;

namespace Rhythm.Revit.Selection
{
    [IsVisibleInDynamoLibrary(false)]
    public class ElementSelection
    {
        private ElementSelection(){}
        public static object InLinkDoc(string docTitle, string uniqueId, bool returnElement, bool isRevitOwned = false )
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;

            var links = new FilteredElementCollector(doc).OfClass(typeof(RevitLinkInstance)).Cast<RevitLinkInstance>().ToList();

            foreach (var l in links)
            {
                var linkDoc = l.GetLinkDocument();
                if (linkDoc.Title.Equals(docTitle))
                {
                    if (returnElement)
                    {
                        
                        return l.GetLinkDocument().GetElement(uniqueId).ToDSType(false);
                    }
                    else
                    {
                        return l.GetTotalTransform().ToCoordinateSystem();
                    }
                    
                }
            }

            return null;
        }
    }
}

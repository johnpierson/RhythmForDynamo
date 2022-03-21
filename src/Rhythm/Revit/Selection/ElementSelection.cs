using System.Linq;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Revit.Elements;
using RevitServices.Persistence;

namespace Rhythm.Revit.Selection
{
    [IsVisibleInDynamoLibrary(false)]
    public class ElementSelection
    {
        private ElementSelection(){}
        public static global::Revit.Elements.Element InLinkDoc(string docTitle, string uniqueId, bool isRevitOwned = true)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;

            var links = new FilteredElementCollector(doc).OfClass(typeof(RevitLinkInstance)).Cast<RevitLinkInstance>().ToList();

            foreach (var l in links)
            {
                var linkDoc = l.GetLinkDocument();
                if (linkDoc.Title.Equals(docTitle))
                {
                    return l.GetLinkDocument().GetElement(uniqueId).ToDSType(true);
                }
            }

            return null;
        }
    }
}

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
        public static global::Revit.Elements.Element InLinkDoc(string arg1, bool arg2 = true)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;

            var links = new FilteredElementCollector(doc).OfClass(typeof(RevitLinkInstance)).Cast<RevitLinkInstance>().ToList();

            foreach (var l in links)
            {
                if (l.GetLinkDocument().GetElement(arg1) != null)
                {
                    return l.GetLinkDocument().GetElement(arg1).ToDSType(true);
                }
            }

            return null;
        }
    }
}

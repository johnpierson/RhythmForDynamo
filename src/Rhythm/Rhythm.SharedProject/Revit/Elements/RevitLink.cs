using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for revit links.
    /// </summary>
    public class RevitLink
    {
        private RevitLink()
        {
        }
        /// <summary>
        /// This node will obtain the selected link's document.
        /// </summary>
        /// <param name="linkInstance">The link to get document from.</param>
        /// <returns name="Document">The document.</returns>
        /// <search>
        ///  rhythm
        /// </search>
        [NodeCategory("Actions")]
        public static Autodesk.Revit.DB.Document GetDocument(global::Revit.Elements.Element linkInstance)
        {
            RevitLinkInstance internalLink = (RevitLinkInstance) linkInstance.InternalElement;
            Autodesk.Revit.DB.Document linkDoc = internalLink.GetLinkDocument();

            return linkDoc;
        }
    }
}

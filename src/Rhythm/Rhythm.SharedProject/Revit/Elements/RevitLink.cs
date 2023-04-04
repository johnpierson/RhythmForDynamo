using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using RevitServices.Transactions;

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


#if Revit2024
        public static Rhythm.Revit.Elements.RevitLinkGraphicsSettings GetLinkOverrides(global::Revit.Elements.Views.View view, global::Revit.Elements.Element linkInstance)
        {
            Autodesk.Revit.DB.View internalView = view.InternalElement as Autodesk.Revit.DB.View;
     
            var linkOverrides = internalView.GetLinkOverrides(linkInstance.InternalElement.Id);

            return new RevitLinkGraphicsSettings(linkOverrides);
        }

        public static void SetLinkOverrides(global::Revit.Elements.Views.View view, global::Revit.Elements.Element linkInstance, Rhythm.Revit.Elements.RevitLinkGraphicsSettings revitLinkGraphicSettings)
        {
            Autodesk.Revit.DB.View internalView = view.InternalElement as Autodesk.Revit.DB.View;
            var doc = internalView.Document;

            TransactionManager.Instance.EnsureInTransaction(doc);
            internalView.SetLinkOverrides(linkInstance.InternalElement.Id, revitLinkGraphicSettings.InternalRevitLinkGraphicsSettings);
            TransactionManager.Instance.TransactionTaskDone();
        }
#endif
    }
}

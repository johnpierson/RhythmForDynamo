using System;
using Autodesk.Revit.DB;
using RevitServices.Persistence;

namespace Rhythm.Revit.Worksharing
{
    /// <summary>
    /// Wrapper class for elements in workshared files.
    /// </summary>
    public class Element
    {
        private Element()
        {
        }
        /// <summary>
        /// This node will output the username of the creator of the element if it is available. Keep in mind this only works with workshared documents!
        /// </summary>
        /// <param name="element">The element to check.</param>
        /// <returns name="creatorName">The username of the creator.</returns>
        public static string Creator(global::Revit.Elements.Element element)
        {
            Autodesk.Revit.DB.Document doc;
            try
            {
                doc = element.InternalElement.Document;
            }
            catch (Exception)
            {
                doc = DocumentManager.Instance.CurrentDBDocument;
            }
            

            WorksharingTooltipInfo tooltipInfo = WorksharingUtils.GetWorksharingTooltipInfo(doc, element.InternalElement.Id);
          
            return tooltipInfo.Creator;
        }
        /// <summary>
        /// This node will output the username of the person who last changed the element if it is available. Keep in mind this only works with workshared documents!
        /// </summary>
        /// <param name="element">The element to check.</param>
        /// <returns name="lastChangedBy">The username of the person who last changed the element.</returns>
        public static string LastChangedBy(global::Revit.Elements.Element element)
        {
            Autodesk.Revit.DB.Document doc;
            try
            {
                doc = element.InternalElement.Document;
            }
            catch (Exception)
            {
                doc = DocumentManager.Instance.CurrentDBDocument;
            }

            WorksharingTooltipInfo tooltipInfo = WorksharingUtils.GetWorksharingTooltipInfo(doc, element.InternalElement.Id);

            return tooltipInfo.LastChangedBy;
        }
    }
}

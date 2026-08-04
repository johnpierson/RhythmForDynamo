using System;
using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using RevitServices.Transactions;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrappers for elements
    /// </summary>
    public class RevitLinkType
    {
        private RevitLinkType()
        {
        }
        /// <summary>
        /// Reload link from another path.
        /// </summary>
        /// <param name="revitLinkType"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        [NodeCategory("Actions")]
        public static string ReloadFrom(global::Revit.Elements.Element revitLinkType, string path)
        {
            
            Autodesk.Revit.DB.RevitLinkType internalLinkType =
                revitLinkType.InternalElement as Autodesk.Revit.DB.RevitLinkType;

            if (internalLinkType == null)
            {
                throw new ArgumentException("The provided element is not a Revit link type.", nameof(revitLinkType));
            }

            // Char.Parse requires a string of length one, so the previous "//" argument threw
            // FormatException on every call, before the link was ever touched. The intent was to
            // collapse doubled separators, which is the two-argument string overload.
            ModelPath mPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(path.Replace("//", "/"));
            TransactionManager.Instance.ForceCloseTransaction();
          
            LinkLoadResult  loadResult = internalLinkType.LoadFrom(mPath, new WorksetConfiguration());
       
            return string.Format(
                "Result = {0}", loadResult.LoadResult);
        }
    }
}

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

            // The path is passed through untouched. The original called Char.Parse("//"), which
            // requires a string of length one and so threw FormatException on every call, before
            // the link was ever reached - the node has never worked, so there is no separator
            // normalisation to preserve here. Nor should there be: ConvertUserVisiblePathToModelPath
            // accepts user-visible server and cloud paths such as RSN://server/folder/model.rvt,
            // and collapsing "//" would rewrite that scheme delimiter to RSN:/ and break the very
            // paths the node exists to reload from.
            ModelPath mPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(path);
            TransactionManager.Instance.ForceCloseTransaction();
          
            LinkLoadResult  loadResult = internalLinkType.LoadFrom(mPath, new WorksetConfiguration());
       
            return string.Format(
                "Result = {0}", loadResult.LoadResult);
        }
    }
}

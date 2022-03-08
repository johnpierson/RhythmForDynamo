using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Rhythm.Revit.Elements
{
    public class DimensionSegment
    {
        private DimensionSegment(){}

        /// <summary>
        /// Get the data from the dimension segment
        /// </summary>
        /// <param name="segment">The dimension segment to check.</param>
        [NodeCategory("Query")]
        [MultiReturn(new[] {  "valueString", "above", "below", "prefix", "suffix", })]
        public static Dictionary<string, object> QueryData(Autodesk.Revit.DB.DimensionSegment segment)
        {
            //returns the outputs
            var outInfo = new Dictionary<string, object>
            {
               
                { "valueString",  segment.ValueString},
                { "above",  segment.Above},
                { "below",  segment.Below},
                { "prefix",  segment.Prefix},
                { "suffix",  segment.Suffix},
            };
            return outInfo;
        }
        /// <summary>
        /// Set the data given the inputs.
        /// </summary>
        /// <param name="segment">The dimension segment</param>
        /// <param name="above">Above value (optional)</param>
        /// <param name="below">Below value (optional)</param>
        /// <param name="prefix">Prefix value (optional)</param>
        /// <param name="suffix">Suffix value (optional)</param>
        [NodeCategory("Actions")]
        public static void SetData(Autodesk.Revit.DB.DimensionSegment segment, [DefaultArgument("Rhythm.Utilities.MiscUtils.GetNull()")] string above, [DefaultArgument("Rhythm.Utilities.MiscUtils.GetNull()")] string below, [DefaultArgument("Rhythm.Utilities.MiscUtils.GetNull()")] string prefix, [DefaultArgument("Rhythm.Utilities.MiscUtils.GetNull()")] string suffix)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(doc);
            if (!string.IsNullOrEmpty(above))
            {
                segment.Above = above;
            }
            if (!string.IsNullOrEmpty(below))
            {
                segment.Below = below;
            }
            if (!string.IsNullOrEmpty(prefix))
            {
                segment.Prefix = prefix;
            }
            if (!string.IsNullOrEmpty(suffix))
            {
                segment.Suffix = suffix;
            }
            TransactionManager.Instance.TransactionTaskDone();
        }


        public static string ValueString(Autodesk.Revit.DB.DimensionSegment segment)
        {
            return segment.ValueString;
        }
        public static double? Value(Autodesk.Revit.DB.DimensionSegment segment)
        {
            return segment.Value;
        }
        public static string Above(Autodesk.Revit.DB.DimensionSegment segment)
        {
            return segment.Above;
        }
        public static string Below(Autodesk.Revit.DB.DimensionSegment segment)
        {
            return segment.Below;
        }
        public static string Prefix(Autodesk.Revit.DB.DimensionSegment segment)
        {
            return segment.Prefix;
        }
        public static string Suffix(Autodesk.Revit.DB.DimensionSegment segment)
        {
            return segment.Suffix;
        }
    }
}

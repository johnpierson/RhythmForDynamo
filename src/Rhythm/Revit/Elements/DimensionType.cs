using System.Collections.Generic;
using Dynamo.Graph.Nodes;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for dimension type.
    /// </summary>
    public class DimensionTypes
    {
        private DimensionTypes()
        { }
        /// <summary>
        /// Determine if the given dimension type uses project (default) settings.
        /// </summary>
        /// <param name="dimensionType">The dimension type the unit format options from.</param>
        /// <returns name="bool">A boolean mask to filter with.</returns>
        /// <search>
        /// dimensiontype
        /// </search>
        [NodeCategory("Query")]
        public static List<bool> UsesProjectSettings(List<global::Revit.Elements.DimensionType> dimensionType)
        {
            //declare new list to append results to
            List<bool> mask = new List<bool>();
            //cycle through all dimension types supplied.
            foreach (var d in dimensionType)
            {
                Autodesk.Revit.DB.DimensionType internalDimType =
    (Autodesk.Revit.DB.DimensionType)d.InternalElement;
                try
                {
                    mask.Add(internalDimType.GetUnitsFormatOptions().UseDefault);
                }
                catch
                {
                    mask.Add(true);
                }
            }

            return mask;
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace Rhythm.Revit.Elements
{
    public class Dimensions
    {
        public static string DisplayUnits(global::Revit.Elements.Dimension dimension)
        {
            Dimension dim = dimension.InternalElement as Dimension;
            try
            {
                return dim.DimensionType.GetUnitsFormatOptions().GetUnitTypeId().TypeId;
            }
            catch (Exception)
            {
                return "UseDefault";
            }
        }

        public static void SetFormat(global::Revit.Elements.Dimension dimension,Autodesk.Revit.DB.Units units)
        {
            Dimension internalDimension = dimension.InternalElement as Dimension;
            ForgeTypeId typeId = new ForgeTypeId("autodesk.spec.aec:length-2.0.0");

            units.SetFormatOptions(typeId,
                internalDimension.DimensionType.GetUnitsFormatOptions());
        }
    }
}

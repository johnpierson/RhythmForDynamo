using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace Rhythm.Revit.Elements
{
    public class Parameters
    {

        public static string GetUnitType(Autodesk.Revit.DB.Parameter parameter)
        {
            return parameter.GetUnitTypeId().TypeId;
        }
    }
}

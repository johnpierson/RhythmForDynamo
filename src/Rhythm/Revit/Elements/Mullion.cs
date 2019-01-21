using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using RevitServices.Persistence;
using Mullion = Revit.Elements.Mullion;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for mullions.
    /// </summary>
    public class Mullions
    {
        private Mullions()
        {
        }

        /// <summary>
        /// This node will retrieve the mullions from the curtain wall grouped by direction.
        /// </summary>
        /// <param name="hostingElement">The wall that contains the mullions.</param>
        /// <returns name="horizontal">The horizontal mullions.</returns>
        /// <returns name="vertical">The vertical mullions.</returns>
        [MultiReturn(new[] { "horizontal", "vertical" })]
        public static Dictionary<string,object>ByDirection(global::Revit.Elements.Wall hostingElement)
        {
            //obtains the current document for later use
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            List<global::Revit.Elements.Mullion> horizontal = new List<Mullion>();
            List<global::Revit.Elements.Mullion> vertical = new List<Mullion>();

            var mullions = Mullion.ByElement(hostingElement);
            foreach (var mullion in mullions)
            {
                if (mullion.LocationCurve.StartPoint.Z == mullion.LocationCurve.EndPoint.Z)
                {
                    horizontal.Add(mullion);
                }
                else
                {
                    vertical.Add(mullion);
                }            
            }

            var orderedHorizontal = horizontal.OrderBy(m => m.LocationCurve.StartPoint.Z).ThenBy(m => m.LocationCurve.StartPoint.X);
            var orderedVertical = vertical.OrderBy(m => m.LocationCurve.StartPoint.X).ThenBy(m => m.LocationCurve.StartPoint.Z);


            var outInfo = new Dictionary<string, object>
            {
                { "horizontal", orderedHorizontal},
                { "vertical", orderedVertical}
            };
            return outInfo;
        }
    }
}

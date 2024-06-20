using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhythm.Geometry
{
    public class Point
    {
        private Point(){}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="point">The point to return values for.</param>
        /// <returns></returns>
        [MultiReturn(new[] { "x", "y","z" })]
        public static Dictionary<string, object> Deconstruct(Autodesk.DesignScript.Geometry.Point point)
        {
            return new Dictionary<string, object>
            {
                { "x", point.X },
                { "y", point.Y},
                { "z",  point.Z}
            };
        }
    }
}

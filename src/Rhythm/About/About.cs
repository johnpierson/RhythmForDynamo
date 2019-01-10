using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhythm.About
{
    /// <summary>
    /// Wrapper for about class.
    /// </summary>
    public class About
    {
        private About()
        {
        }

        /// <summary>
        /// This node is primarily to show the icon in Dynamo 2.0
        /// </summary>
        /// <returns></returns>
        public static string AboutRhythm()
        {
            return "Rhythm does stuff";
        }
    }
}

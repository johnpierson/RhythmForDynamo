using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dynamo.Graph.Nodes;
using Humanizer;

namespace Rhythm.Numbers
{

    /// <summary>
    /// Wrapper class for numbers
    /// </summary>
    public class Numbers
    {
        private Numbers()
        {
        }

        /// <summary>
        /// Convert the input numbers into words. Only considers whole numbers (integers).
        /// Made possible with Humanizer (https://github.com/Humanizr/Humanizer)
        /// </summary>
        /// <param name="number"></param>
        /// <param name="locale">This offers the option to override the locale of the words.
        /// By default it uses your computer's locale, you can override this with the list here. https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c?redirectedfrom=MSDN</param>
        /// <returns></returns>
        [NodeCategory("Actions")]
        public static string ToWords(int number, string locale = "")
        {
            if (locale == "")
            {
                return number.ToWords(Thread.CurrentThread.CurrentCulture);
            }
            else
            {
                return number.ToWords(CultureInfo.GetCultureInfo(locale));
            }
        }
        /// <summary>
        /// Convert the input numbers into ordinal words. Only considers whole numbers (integers) (Eg. 1 becomes 1st).
        /// Made possible with Humanizer (https://github.com/Humanizr/Humanizer)
        /// </summary>
        /// <param name="number"></param>
        /// <param name="locale">This offers the option to override the locale of the words.
        /// By default it uses your computer's locale, you can override this with the list here. https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c?redirectedfrom=MSDN</param>
        /// <returns></returns>
        [NodeCategory("Actions")]
        public static string ToOrdinalWords(int number, string locale = "")
        {
            if (locale == "")
            {
                return number.ToOrdinalWords(Thread.CurrentThread.CurrentCulture);
            }
            else
            {
                return number.ToOrdinalWords(CultureInfo.GetCultureInfo(locale));
            }
        }
        /// <summary>
        /// Convert the input numbers into roman numerals. Only considers whole numbers (integers) (Eg. 1 becomes I).
        /// Made possible with Humanizer (https://github.com/Humanizr/Humanizer)
        /// </summary>
        /// <param name="number"></param>
        /// By default it uses your computer's locale, you can override this with the list here. https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c?redirectedfrom=MSDN
        /// <returns></returns>
        [NodeCategory("Actions")]
        public static string ToRoman(int number)
        {       
                return number.ToRoman();
        }
        #region Headings

        /// <summary>
        /// Convert the input numbers into headings: N,S,E,W or north, east, south or west.
        /// Made possible with Humanizer (https://github.com/Humanizr/Humanizer)
        /// </summary>
        /// <param name="number"></param>
        /// <param name="fullHeading"></param>
        /// <returns></returns>
        [NodeCategory("Actions")]
        public static string ToHeading(double number, bool fullHeading = false)
        {
            return fullHeading ? number.ToHeading(HeadingStyle.Full) : number.ToHeading();
        }
        /// <summary>
        /// Convert the input numbers into headings: ↑, →, ↓, ←.
        /// Made possible with Humanizer (https://github.com/Humanizr/Humanizer)
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        [NodeCategory("Actions")]
        public static string ToHeadingArrow(double number)
        {
            return number.ToHeadingArrow().ToString();
        }

        #endregion



    }
}

using System;
using System.Globalization;
using System.Threading;

namespace Rhythm.String
{
    /// <summary>
    /// Wrapper class for string modifiers
    /// </summary>
    public class Modify
    {
        private Modify()
        {
        }

        /// <summary>
        /// Converts the input string to a title case.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToTitle(string str)
        {
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            TextInfo textInfo = cultureInfo.TextInfo;
            
            return textInfo.ToTitleCase(str);
        }

        //this code was inspired by this python code - https://github.com/nkrim/spongemock
        /// <summary>
        /// This generates a "mocking text" case. Just for fun. 😀
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string MOcKtExt(string str)
        {
            string newString = string.Empty;
            Random ran = new Random(0);
            bool lastUpper = true;
            double diversity = 0.5;
            double swapChance = 0.5;

            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];

                if (char.IsLetter(c))
                {
                    if (ran.NextDouble() < swapChance)
                    {
                        lastUpper = !lastUpper;
                        swapChance = 0.5;
                    }

                    if (lastUpper)
                    {
                        c = char.ToUpper(c);
                    }
                    else
                    {
                        c = char.ToLower(c);
                    }

                    swapChance += (1 - swapChance) * diversity;
                }

                newString += c;
            }

            return newString;
        }

    }
}

using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Dynamo.Graph.Nodes;
using Humanizer;

namespace Rhythm.String
{
    /// <summary>
    /// Wrapper class for string modifiers
    /// </summary>
    public class Inspect
    {
        private Inspect(){}

        /// <summary>
        /// Find the longest common substring between two strings.
        /// </summary>
        /// <param name="string1">First one to compare.</param>
        /// <param name="string2">Second one to compare.</param>
        /// <returns name="longestCommonSubstring">The longest common substring.</returns>
        public static string LongestCommonSubstring(string string1, string string2)
        {
            var subStr = string.Empty;

            if (string.IsNullOrEmpty(string1) || string.IsNullOrEmpty(string2))
                return string.Empty;

            int[,] num = new int[string1.Length, string2.Length];
            int maxlen = 0;
            int lastSubsBegin = 0;
            StringBuilder subStrBuilder = new StringBuilder();

            for (int i = 0; i < string1.Length; i++)
            {
                for (int j = 0; j < string2.Length; j++)
                {
                    if (string1[i] != string2[j])
                    {
                        num[i, j] = 0;
                    }
                    else
                    {
                        if ((i == 0) || (j == 0))
                            num[i, j] = 1;
                        else
                            num[i, j] = 1 + num[i - 1, j - 1];

                        if (num[i, j] > maxlen)
                        {
                            maxlen = num[i, j];

                            int thisSubsBegin = i - num[i, j] + 1;

                            if (lastSubsBegin == thisSubsBegin)
                            {
                                subStrBuilder.Append(string1[i]);
                            }
                            else
                            {
                                lastSubsBegin = thisSubsBegin;
                                subStrBuilder.Length = 0;
                                subStrBuilder.Append(string1.Substring(lastSubsBegin, (i + 1) - lastSubsBegin));
                            }
                        }
                    }
                }
            }

            subStr = subStrBuilder.ToString();

            return subStr;
        }
    }
    public class Modify
    {
        private Modify()
        {
        }

        /// <summary>
        /// This will run a regular expression on a a string. By default this removes all whitespace and special characters from a string
        /// </summary>
        /// <param name="stringToReplace">Your target string.</param>
        /// <param name="regexString">The regular expression to use.</param>
        /// <param name="replacement">What to replace with.</param>
        /// <returns name="modifiedString">The finished product</returns>
        [NodeCategory("Actions")]
        public static string ParseRegularExpression(string stringToReplace, string regexString = @"[^a-zA-Z0-9]", string replacement = "")
        {
            string modifiedString = Regex.Replace(stringToReplace, @"[^a-zA-Z0-9]", "");

            return modifiedString;
        }
        /// <summary>
        /// Converts the input string to a title case.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        [NodeCategory("Actions")]
        public static string ToTitle(string str)
        {
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            TextInfo textInfo = cultureInfo.TextInfo;
            
            return textInfo.ToTitleCase(str);
        }
        /// <summary>
        /// Converts the input string to a title case.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        [NodeCategory("Actions")]
        public static string ToSentence(string str)
        {
            if (str.Contains('.'))
            {
                var split = str.ToLower().Split('.');

                return string.Join(".", split.Select(s => s.Transform(To.SentenceCase)).ToArray());
            }

            if (str.Contains('\r'))
            {
                var split = str.ToLower().Split('\r');

                return string.Join("\r", split.Select(s => s.Transform(To.SentenceCase)).ToArray());
            }


            return str.Transform(To.SentenceCase);
        }
        /// <summary>
        /// This will attempt to return a quantity, given a string and count.
        /// Made possible with Humanizer (https://github.com/Humanizr/Humanizer)
        /// </summary>
        /// <param name="str">The string to convert to quantity.</param>
        /// <param name="quantity">The amount of things.</param>
        /// <returns name="quantityString">The formatted quantity.</returns>
        [NodeCategory("Actions")]
        public static string ToQuantity(string str, int quantity)
        {
            return str.ToQuantity(quantity);
        }

        /// <summary>
        /// This will truncate the given string, byt the given length. (Eg."Long text to truncate", with 10, becomes Long text…") 
        /// Made possible with Humanizer (https://github.com/Humanizr/Humanizer)
        /// </summary>
        /// <param name="str">The string to truncate.</param>
        /// <param name="length">The target length of the string.</param>
        /// <param name="truncationString">The characters to fill in the string with.</param>
        /// <returns name="truncatedString">The truncated string.</returns>
        [NodeCategory("Actions")]
        public static string Truncate(string str, int length, string truncationString = "…")
        {
            return str.Truncate(length, truncationString);
        }

        /// <summary>
        /// This will attempt to return a plural version of a word.
        /// Made possible with Humanizer (https://github.com/Humanizr/Humanizer)
        /// </summary>
        /// <param name="str">The string to pluralize.</param>
        /// <returns name="pluralString">The string in it's plural form.</returns>
        [NodeCategory("Actions")]
        public static string Pluralize(string str)
        {
            return str.Pluralize();
        }
        /// <summary>
        /// This will attempt to return a singular version of a word.
        /// Made possible with Humanizer (https://github.com/Humanizr/Humanizer)
        /// </summary>
        /// <param name="str">The string to singularize.</param>
        /// <returns name="singularString">The string in it's singular form.</returns>
        [NodeCategory("Actions")]
        public static string Singularize(string str)
        {
            return str.Singularize();
        }
        /// <summary>
        /// Titleize converts the input words to Title casing
        /// Made possible with Humanizer (https://github.com/Humanizr/Humanizer)
        /// </summary>
        /// <param name="str">The string to titleize.</param>
        /// <returns name="str">The string in it's title form.</returns>
        [NodeCategory("Actions")]
        public static string Titleize(string str)
        {
            return str.Titleize();
        }
        /// <summary>
        /// Pascalize converts the input words to UpperCamelCase, also removing underscores and spaces. (Eg. SomeTitleForSomething)
        /// Made possible with Humanizer (https://github.com/Humanizr/Humanizer)
        /// </summary>
        /// <param name="str">The string to Pascalize.</param>
        /// <returns name="str">The string in it's pascal case form.</returns>
        [NodeCategory("Actions")]
        public static string Pascalize(string str)
        {
            return str.Pascalize();
        }
        /// <summary>
        /// Camelize behaves identically to Pascalize, except that the first character is lower case. (Eg. someTitleForSomething)
        /// Made possible with Humanizer (https://github.com/Humanizr/Humanizer)
        /// </summary>
        /// <param name="str">The string to Camelize.</param>
        /// <returns name="str">The string in it's camel case form.</returns>
        [NodeCategory("Actions")]
        public static string Camelize(string str)
        {
            return str.Camelize();
        }
        /// <summary>
        /// Underscore separates the input words with underscore. (Eg. some_title)
        /// Made possible with Humanizer (https://github.com/Humanizr/Humanizer)
        /// </summary>
        /// <param name="str">The string to convert to underscore separated.</param>
        /// <returns name="str">The resulting words separated with underscores.</returns>
        [NodeCategory("Actions")]
        public static string Underscore(string str)
        {
            return str.Underscore();
        }
        /// <summary>
        /// Underscore separates the input words with a dash. (Eg. Some-Title)
        /// Made possible with Humanizer (https://github.com/Humanizr/Humanizer)
        /// </summary>
        /// <param name="str">The string to convert to dash separated.</param>
        /// <returns name="str">The resulting words separated with dashes.</returns>
        [NodeCategory("Actions")]
        public static string Dasherize(string str)
        {
            return str.Underscore().Dasherize();
        }
        /// <summary>
        /// Humanize string extensions allow you turn an otherwise computerized string into a more readable human-friendly one.
        /// "Underscored_input_string_is_turned_into_sentence." becomes "Underscored input string is turned into sentence"
        /// Made possible with Humanizer (https://github.com/Humanizr/Humanizer)
        /// </summary>
        /// <param name="obj">The string to human understandable form.</param>
        /// <returns name="str">The humanized string.</returns>
        [NodeCategory("Actions")]
        public static string Humanize(object obj)
        {
            if (obj is DateTime date)
            {
                return date.Humanize(false);
            }
            if (obj is TimeSpan span)
            {
                return span.Humanize();
            }

            string str = obj as string;
            return str.Humanize();

        }

        /// <summary>
        /// Format input string with arguments.
        /// Made possible with Humanizer (https://github.com/Humanizr/Humanizer)
        /// </summary>
        /// <param name="str">The string to format.</param>
        /// <param name="args">The params (objects)</param>
        /// <returns name="str">The formatted string.</returns>
        [NodeCategory("Actions")]
        public static string FormatWith(string str, object[] args)
        {
            return str.FormatWith(args);
        }

        //this code was inspired by this python code - https://github.com/nkrim/spongemock
        /// <summary>
        /// This generates a "mocking text" case. Just for fun. 😀
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        [NodeCategory("Actions")]
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

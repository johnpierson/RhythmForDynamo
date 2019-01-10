using System;
using System.Collections.Generic;
using System.Linq;
using Rhythm.Utilities;
using Element = Revit.Elements.Element;

namespace Rhythm.Revit.ElementFilter
{
    /// <summary>
    /// Wrapper class for ElementFilter.
    /// </summary>
    public class ElementFilter
    {
        private ElementFilter()
        {
        }
        /// <summary>
        /// Provides element filtering options by name. For the filter method, we are using something called "LevenshteinDistance". 
        /// This was introduced to me by Eric Rudisaile + Kyle Martin here, http://dynamobim.org/fuzzy-string-matching/.
        /// </summary>
        /// <param name="elements">The elements to filter.</param>
        /// <param name="value">The value to filter by.</param>
        /// <param name="filterMethod">The method to filter by. This includes Contains, DoesNotContain, StartsWith, DoesNotStartWith, EndsWith, DoesNotEndWith, Equals, DoesNotEqual</param>
        /// <returns name="elements">The filtered elements.</returns>
        /// <search>
        /// ElementFilter,Filter.ByName
        /// </search>
        public static List<global::Revit.Elements.Element> ByName(List<global::Revit.Elements.Element> elements, string value, string filterMethod)
        {
            List<string> potentialFilterMethod = new List<string>(new string[] { "Contains", "DoesNotContain", "StartsWith", "DoesNotStartWith", "EndsWith", "DoesNotEndWith", "Equals", "DoesNotEqual","==","!=" });
            List<global::Revit.Elements.Element> filteredElements = null;
            string filterMethodToUse = null;
            List<int> values = new List<int>();

            foreach (string i in potentialFilterMethod)
            {
                values.Add(StringComparisonUtilities.Compute(filterMethod, i));
            }
            int minIndex = values.IndexOf(values.Min());
            filterMethodToUse = potentialFilterMethod[minIndex];
            //scenarios there is probably a better way to do this.
            switch (filterMethodToUse)
            {
                case "Contains":
                    filteredElements = new List<Element>(elements.Where(e => e.Name.Contains(value)).ToArray());
                    break;
                case "DoesNotContain":
                    filteredElements = new List<Element>(elements.Where(e => !e.Name.Contains(value)).ToArray());
                    break;
                case "StartsWith":
                    filteredElements = new List<Element>(elements.Where(e => e.Name.StartsWith(value)).ToArray());
                    break;
                case "DoesNotStartWith":
                    filteredElements = new List<Element>(elements.Where(e => !e.Name.StartsWith(value)).ToArray());
                    break;
                case "EndsWith":
                    filteredElements = new List<Element>(elements.Where(e => e.Name.EndsWith(value)).ToArray());
                    break;
                case "DoesNotEndWith":
                    filteredElements = new List<Element>(elements.Where(e => !e.Name.EndsWith(value)).ToArray());
                    break;
                case "Equals":
                    filteredElements = new List<Element>(elements.Where(e => e.Name.Equals(value)).ToArray());
                    break;
                case "==":
                    filteredElements = new List<Element>(elements.Where(e => e.Name.Equals(value)).ToArray());
                    break;
                case "DoesNotEqual":
                    filteredElements = new List<Element>(elements.Where(e => !e.Name.Equals(value)).ToArray());
                    break;
                case "!=":
                    filteredElements = new List<Element>(elements.Where(e => !e.Name.Equals(value)).ToArray());
                    break;
                default:
                    filteredElements = elements;
                    break;
            }

            return filteredElements;
        }
        /// <summary>
        /// Provides element filtering options by parameter string value. For the filter method, we are using something called "LevenshteinDistance". 
        /// This was introduced to me by Eric Rudisaile + Kyle Martin here, http://dynamobim.org/fuzzy-string-matching/.
        /// </summary>
        /// <param name="elements">The elements to filter.</param>
        /// <param name="parameterName">The parameter name to filter against..</param>
        /// <param name="value">The value to filter by.</param>
        /// <param name="filterMethod">The method to filter by. This includes Contains, DoesNotContain, StartsWith, DoesNotStartWith, EndsWith, DoesNotEndWith, Equals, DoesNotEqual</param>
        /// <returns name="elements">The filtered elements.</returns>
        /// <search>
        /// ElementFilter,Filter.ByName
        /// </search>
        public static List<global::Revit.Elements.Element> ByParameterStringValue(List<global::Revit.Elements.Element> elements, string parameterName, string value, string filterMethod)
        {
            List<string> potentialFilterMethod = new List<string>(new string[] { "Contains", "DoesNotContain", "StartsWith", "DoesNotStartWith", "EndsWith", "DoesNotEndWith", "Equals", "DoesNotEqual","==","!=" });
            List<global::Revit.Elements.Element> filteredElements = null;
            string filterMethodToUse = null;
            List<int> values = new List<int>();

            foreach (string i in potentialFilterMethod)
            {
                values.Add(StringComparisonUtilities.Compute(filterMethod, i));
            }
            int minIndex = values.IndexOf(values.Min());
            filterMethodToUse = potentialFilterMethod[minIndex];
            //scenarios there is probably a better way to do this.
            switch (filterMethodToUse)
            {
                case "Contains":
                    filteredElements = new List<Element>(elements.Where(e => e.GetParameterValueByName(parameterName).ToString().Contains(value)).ToArray());
                    break;
                case "DoesNotContain":
                    filteredElements = new List<Element>(elements.Where(e => !e.GetParameterValueByName(parameterName).ToString().Contains(value)).ToArray());
                    break;
                case "StartsWith":
                    filteredElements = new List<Element>(elements.Where(e => e.GetParameterValueByName(parameterName).ToString().StartsWith(value)).ToArray());
                    break;
                case "DoesNotStartWith":
                    filteredElements = new List<Element>(elements.Where(e => !e.GetParameterValueByName(parameterName).ToString().StartsWith(value)).ToArray());
                    break;
                case "EndsWith":
                    filteredElements = new List<Element>(elements.Where(e => e.GetParameterValueByName(parameterName).ToString().EndsWith(value)).ToArray());
                    break;
                case "DoesNotEndWith":
                    filteredElements = new List<Element>(elements.Where(e => !e.GetParameterValueByName(parameterName).ToString().EndsWith(value)).ToArray());
                    break;
                case "Equals":
                    filteredElements = new List<Element>(elements.Where(e => e.GetParameterValueByName(parameterName).ToString().Equals(value)).ToArray());
                    break;
                case "==":
                    filteredElements = new List<Element>(elements.Where(e => e.GetParameterValueByName(parameterName).ToString().Equals(value)).ToArray());
                    break;
                case "DoesNotEqual":
                    filteredElements = new List<Element>(elements.Where(e => !e.GetParameterValueByName(parameterName).ToString().Equals(value)).ToArray());
                    break;
                case "!=":
                    filteredElements = new List<Element>(elements.Where(e => !e.GetParameterValueByName(parameterName).ToString().Equals(value)).ToArray());
                    break;
                default:
                    filteredElements = elements;
                    break;
            }


            return filteredElements;
        }
        /// <summary>
        /// Provides element filtering options by parameter numeric value. For the filter method, we are using something called "LevenshteinDistance". 
        /// This was introduced to me by Eric Rudisaile + Kyle Martin here, http://dynamobim.org/fuzzy-string-matching/.
        /// </summary>
        /// <param name="elements">The elements to filter.</param>
        /// <param name="parameterName">The parameter name to filter against..</param>
        /// <param name="value">The value to filter by.</param>
        /// <param name="filterMethod">The method to filter by. This includes GreaterThan, GreaterThanOrEqualTo, LessThan, LessThanOrEqualTo, EqualTo, NotEqualTo</param>
        /// <returns name="elements">The filtered elements.</returns>
        /// <search>
        /// ElementFilter,Filter.ByName
        /// </search>
        public static List<global::Revit.Elements.Element> ByParameterNumericValue(List<global::Revit.Elements.Element> elements, string parameterName, double value, string filterMethod)
        {
            List<string> potentialFilterMethod = new List<string>(new string[] { "GreaterThan", "GreaterThanOrEqualTo", "LessThan", "LessThanOrEqualTo", ">", ">=", "<", "<=","EqualTo","==","NotEqualTo","!=" });
            List<global::Revit.Elements.Element> filteredElements = null;
            string filterMethodToUse = null;
            List<int> values = new List<int>();

            foreach (string i in potentialFilterMethod)
            {
                values.Add(StringComparisonUtilities.Compute(filterMethod, i));
            }
            int minIndex = values.IndexOf(values.Min());
            filterMethodToUse = potentialFilterMethod[minIndex];
            //scenarios
            if (filterMethodToUse == "GreaterThan" || filterMethodToUse == ">")
            {
                filteredElements = new List<Element>(elements.Where(e => Convert.ToDouble(e.GetParameterValueByName(parameterName)) > value).ToArray());
            }
            if (filterMethodToUse == "GreaterThanOrEqualTo" || filterMethodToUse == ">=")
            {
                filteredElements = new List<Element>(elements.Where(e => Convert.ToDouble(e.GetParameterValueByName(parameterName)) >= value).ToArray());
            }
            if (filterMethodToUse == "LessThan" || filterMethodToUse == "<")
            {
                filteredElements = new List<Element>(elements.Where(e => Convert.ToDouble(e.GetParameterValueByName(parameterName)) < value).ToArray());
            }
            if (filterMethodToUse == "LessThanOrEqualTo" || filterMethodToUse == "<=")
            {
                filteredElements = new List<Element>(elements.Where(e => Convert.ToDouble(e.GetParameterValueByName(parameterName)) <= value).ToArray());
            }
            if (filterMethodToUse == "EqualTo" || filterMethodToUse == "==")
            {
                filteredElements = new List<Element>(elements.Where(e => Convert.ToDouble(e.GetParameterValueByName(parameterName)) == value).ToArray());
            }
            if (filterMethodToUse == "NotEqualTo" || filterMethodToUse == "!=")
            {
                filteredElements = new List<Element>(elements.Where(e => Convert.ToDouble(e.GetParameterValueByName(parameterName)) != value).ToArray());
            }

            return filteredElements;
        }

        /// <summary>
        /// Provides element filtering options by category. For the filter method, we are using something called "LevenshteinDistance". 
        /// This was introduced to me by Eric Rudisaile + Kyle Martin here, http://dynamobim.org/fuzzy-string-matching/.
        /// </summary>
        /// <param name="elements">The elements to filter.</param>
        /// <param name="category">The value to filter by.</param>
        /// <param name="filterMethod">The method to filter by. This includes Contains, DoesNotContain, StartsWith, DoesNotStartWith, EndsWith, DoesNotEndWith, Equals, DoesNotEqual</param>
        /// <returns name="elements">The filtered elements.</returns>
        /// <search>
        /// ElementFilter,Filter.ByName
        /// </search>
        public static List<global::Revit.Elements.Element> ByCategory(List<global::Revit.Elements.Element> elements, object category, string filterMethod)
        {
            List<string> potentialFilterMethod = new List<string>(new string[] { "Contains", "DoesNotContain", "StartsWith", "DoesNotStartWith", "EndsWith", "DoesNotEndWith", "Equals", "DoesNotEqual","==","!=" });
            List<global::Revit.Elements.Element> filteredElements = null;
            string filterMethodToUse = null;
            List<int> values = new List<int>();
            string value = null;

            foreach (string i in potentialFilterMethod)
            {
                values.Add(StringComparisonUtilities.Compute(filterMethod, i));
            }
            int minIndex = values.IndexOf(values.Min());
            filterMethodToUse = potentialFilterMethod[minIndex];
            //assign the value based on if the input is a string or category
            if (category is string)
            {
                value = category as string;
            }
            else
            {
                global::Revit.Elements.Category cat = (global::Revit.Elements.Category)category;
                value = cat.Name;
            }
            //scenarios there is probably a better way to do this.
            switch (filterMethodToUse)
            {
                case "Contains":
                    filteredElements = new List<Element>(elements.Where(e => e.GetCategory.Name.Contains(value)).ToArray());
                    break;
                case "DoesNotContain":
                    filteredElements = new List<Element>(elements.Where(e => !e.GetCategory.Name.Contains(value)).ToArray());
                    break;
                case "StartsWith":
                    filteredElements = new List<Element>(elements.Where(e => e.GetCategory.Name.StartsWith(value)).ToArray());
                    break;
                case "DoesNotStartWith":
                    filteredElements = new List<Element>(elements.Where(e => !e.GetCategory.Name.StartsWith(value)).ToArray());
                    break;
                case "EndsWith":
                    filteredElements = new List<Element>(elements.Where(e => e.GetCategory.Name.EndsWith(value)).ToArray());
                    break;
                case "DoesNotEndWith":
                    filteredElements = new List<Element>(elements.Where(e => !e.GetCategory.Name.EndsWith(value)).ToArray());
                    break;
                case "Equals":
                    filteredElements = new List<Element>(elements.Where(e => e.GetCategory.Name.Equals(value)).ToArray());
                    break;
                case "==":
                    filteredElements = new List<Element>(elements.Where(e => e.GetCategory.Name.Equals(value)).ToArray());
                    break;
                case "DoesNotEqual":
                    filteredElements = new List<Element>(elements.Where(e => !e.GetCategory.Name.Equals(value)).ToArray());
                    break;
                case "!=":
                    filteredElements = new List<Element>(elements.Where(e => !e.GetCategory.Name.Equals(value)).ToArray());
                    break;
                default:
                    filteredElements = elements;
                    break;
            }    

            return filteredElements;
        }

    }
}

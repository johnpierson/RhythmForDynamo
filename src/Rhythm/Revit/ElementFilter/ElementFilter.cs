using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Graph.Nodes;
using Dynamo.Utilities;
using Rhythm.Utilities;
using Convert = System.Convert;
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
        /// This was introduced to me here, http://dynamobim.org/fuzzy-string-matching/.
        /// </summary>
        /// <param name="elements">The elements to filter.</param>
        /// <param name="value">The value to filter by.</param>
        /// <param name="filterMethod">The method to filter by. This includes Contains, DoesNotContain, StartsWith, DoesNotStartWith, EndsWith, DoesNotEndWith, Equals, DoesNotEqual</param>
        /// <param name="ignoreCase">Ignore the case?</param>
        /// <returns name="elements">The filtered elements.</returns>
        /// <search>
        /// ElementFilter,Filter.ByName
        /// </search>
        [NodeCategory("Actions")]
        public static List<global::Revit.Elements.Element> ByName(List<global::Revit.Elements.Element> elements, string value, string filterMethod, bool ignoreCase = false)
        {
            var potentialFilterMethod = new[] { "Contains", "DoesNotContain", "StartsWith", "DoesNotStartWith", "EndsWith", "DoesNotEndWith", "Equals", "DoesNotEqual", "==", "!=" };

            var values = potentialFilterMethod.Select(i => StringComparisonUtilities.Compute(filterMethod, i)).ToArray();
            int minIndex = values.IndexOf(values.Min());
            string filterMethodToUse = potentialFilterMethod[minIndex];

            //scenarios
            switch (filterMethodToUse)
            {
                case "Contains":
                    return ignoreCase ? elements.Where(e => e.InternalElement.Name.ToLower().Contains(value.ToLower())).ToList() : elements.Where(e => e.InternalElement.Name.Contains(value)).ToList();
                case "DoesNotContain":
                    return ignoreCase ? elements.Where(e => !e.InternalElement.Name.ToLower().Contains(value.ToLower())).ToList() : elements.Where(e => !e.InternalElement.Name.Contains(value)).ToList();
                case "StartsWith":
                    return ignoreCase ? elements.Where(e => e.InternalElement.Name.ToLower().StartsWith(value.ToLower())).ToList() : elements.Where(e => e.InternalElement.Name.StartsWith(value)).ToList();
                case "DoesNotStartWith":
                    return ignoreCase ? elements.Where(e => !e.InternalElement.Name.ToLower().StartsWith(value.ToLower())).ToList() : elements.Where(e => !e.InternalElement.Name.StartsWith(value)).ToList();
                case "EndsWith":
                    return ignoreCase ? elements.Where(e => e.InternalElement.Name.ToLower().EndsWith(value.ToLower())).ToList() : elements.Where(e => e.InternalElement.Name.EndsWith(value)).ToList();
                case "DoesNotEndWith":
                    return ignoreCase ? elements.Where(e => !e.InternalElement.Name.ToLower().EndsWith(value.ToLower())).ToList() : elements.Where(e => !e.InternalElement.Name.EndsWith(value)).ToList();
                case "Equals":
                case "==":
                    return ignoreCase ? elements.Where(e => e.InternalElement.Name.ToLower().Equals(value.ToLower())).ToList() : elements.Where(e => e.InternalElement.Name.Equals(value)).ToList();
                case "DoesNotEqual":
                case "!=":
                    return ignoreCase ? elements.Where(e => !e.InternalElement.Name.ToLower().Equals(value.ToLower())).ToList() : elements.Where(e => !e.InternalElement.Name.Equals(value)).ToList();
                default:
                    return elements;
            }
        }
        /// <summary>
        /// Provides element filtering options by parameter string value. For the filter method, we are using something called "LevenshteinDistance". 
        /// This was introduced to me here, http://dynamobim.org/fuzzy-string-matching/.
        /// </summary>
        /// <param name="elements">The elements to filter.</param>
        /// <param name="parameterName">The parameter name to filter against..</param>
        /// <param name="value">The value to filter by.</param>
        /// <param name="filterMethod">The method to filter by. This includes Contains, DoesNotContain, StartsWith, DoesNotStartWith, EndsWith, DoesNotEndWith, Equals, DoesNotEqual</param>
        /// <returns name="elements">The filtered elements.</returns>
        /// <search>
        /// ElementFilter,Filter.ByName
        /// </search>
        [NodeCategory("Actions")]
        public static List<global::Revit.Elements.Element> ByParameterStringValue(List<global::Revit.Elements.Element> elements, string parameterName, string value, string filterMethod)
        {
            var potentialFilterMethod = new[] { "Contains", "DoesNotContain", "StartsWith", "DoesNotStartWith", "EndsWith", "DoesNotEndWith", "Equals", "DoesNotEqual", "==", "!=" };

            var values = potentialFilterMethod.Select(i => StringComparisonUtilities.Compute(filterMethod, i)).ToArray();
            int minIndex = values.IndexOf(values.Min());
            string filterMethodToUse = potentialFilterMethod[minIndex];

            //scenarios
            switch (filterMethodToUse)
            {
                case "Contains":
                    return elements.Where(e => e.GetParameterValueByName(parameterName).ToString().Contains(value)).ToList();
                case "DoesNotContain":
                    return elements.Where(e => !e.GetParameterValueByName(parameterName).ToString().Contains(value)).ToList();
                case "StartsWith":
                    return elements.Where(e => e.GetParameterValueByName(parameterName).ToString().StartsWith(value)).ToList();
                case "DoesNotStartWith":
                    return elements.Where(e => !e.GetParameterValueByName(parameterName).ToString().StartsWith(value)).ToList();
                case "EndsWith":
                    return elements.Where(e => e.GetParameterValueByName(parameterName).ToString().EndsWith(value)).ToList();
                case "DoesNotEndWith":
                    return elements.Where(e => !e.GetParameterValueByName(parameterName).ToString().EndsWith(value)).ToList();
                case "Equals":
                case "==":
                    return elements.Where(e => e.GetParameterValueByName(parameterName).ToString().Equals(value)).ToList();
                case "DoesNotEqual":
                case "!=":
                    return elements.Where(e => !e.GetParameterValueByName(parameterName).ToString().Equals(value)).ToList();
                default:
                    return elements;
            }
        }
        /// <summary>
        /// Provides element filtering options by parameter numeric value. For the filter method, we are using something called "LevenshteinDistance". 
        /// This was introduced to me here, http://dynamobim.org/fuzzy-string-matching/.
        /// </summary>
        /// <param name="elements">The elements to filter.</param>
        /// <param name="parameterName">The parameter name to filter against..</param>
        /// <param name="value">The value to filter by.</param>
        /// <param name="filterMethod">The method to filter by. This includes GreaterThan, GreaterThanOrEqualTo, LessThan, LessThanOrEqualTo, EqualTo, NotEqualTo</param>
        /// <returns name="elements">The filtered elements.</returns>
        /// <search>
        /// ElementFilter,Filter.ByName
        /// </search>
        [NodeCategory("Actions")]
        public static List<global::Revit.Elements.Element> ByParameterNumericValue(List<global::Revit.Elements.Element> elements, string parameterName, double value, string filterMethod)
        {
            var potentialFilterMethod = new[] { "GreaterThan", "GreaterThanOrEqualTo", "LessThan", "LessThanOrEqualTo", ">", ">=", "<", "<=", "EqualTo", "==", "NotEqualTo", "!=" };

            var values = potentialFilterMethod.Select(i => StringComparisonUtilities.Compute(filterMethod, i)).ToArray();
            int minIndex = values.IndexOf(values.Min());
            string filterMethodToUse = potentialFilterMethod[minIndex];

            switch (filterMethodToUse)
            {
                //scenarios
                case "GreaterThan":
                case ">":
                    return elements.Where(e => Convert.ToDouble(e.GetParameterValueByName(parameterName)) > value).ToList();
                case "GreaterThanOrEqualTo":
                case ">=":
                    return elements.Where(e => Convert.ToDouble(e.GetParameterValueByName(parameterName)) >= value).ToList();
                case "LessThan":
                case "<":
                    return elements.Where(e => Convert.ToDouble(e.GetParameterValueByName(parameterName)) < value).ToList();
                case "LessThanOrEqualTo":
                case "<=":
                    return elements.Where(e => Convert.ToDouble(e.GetParameterValueByName(parameterName)) <= value).ToList();
                case "EqualTo":
                case "==":
                    return elements.Where(e => Convert.ToDouble(e.GetParameterValueByName(parameterName)) == value).ToList();
                case "NotEqualTo":
                case "!=":
                    return elements.Where(e => Convert.ToDouble(e.GetParameterValueByName(parameterName)) != value).ToList();
                default:
                    return elements;
            }
        }

        /// <summary>
        /// Provides element filtering options by category. For the filter method, we are using something called "LevenshteinDistance". 
        /// This was introduced to me here, http://dynamobim.org/fuzzy-string-matching/.
        /// </summary>
        /// <param name="elements">The elements to filter.</param>
        /// <param name="category">The value to filter by.</param>
        /// <param name="filterMethod">The method to filter by. This includes Contains, DoesNotContain, StartsWith, DoesNotStartWith, EndsWith, DoesNotEndWith, Equals, DoesNotEqual</param>
        /// <returns name="elements">The filtered elements.</returns>
        /// <search>
        /// ElementFilter,Filter.ByName
        /// </search>
        [NodeCategory("Actions")]
        public static List<global::Revit.Elements.Element> ByCategory(List<global::Revit.Elements.Element> elements, object category, string filterMethod)
        {
            var potentialFilterMethod = new[] { "Contains", "DoesNotContain", "StartsWith", "DoesNotStartWith", "EndsWith", "DoesNotEndWith", "Equals", "DoesNotEqual", "==", "!=" };

            string value;

            var values = potentialFilterMethod.Select(i => StringComparisonUtilities.Compute(filterMethod, i)).ToArray();
            int minIndex = values.IndexOf(values.Min());
            string filterMethodToUse = potentialFilterMethod[minIndex];

            //assign the value based on if the input is a string or category
            if (category is string s)
            {
                value = s;
            }
            else
            {
                global::Revit.Elements.Category cat = (global::Revit.Elements.Category)category;
                value = cat.Name;
            }

            switch (filterMethodToUse)
            {
                //scenarios
                case "Contains":
                    return elements.Where(e => e.GetCategory.Name.Contains(value)).ToList();
                case "DoesNotContain":
                    return elements.Where(e => !e.GetCategory.Name.Contains(value)).ToList();
                case "StartsWith":
                    return elements.Where(e => e.GetCategory.Name.StartsWith(value)).ToList();
                case "DoesNotStartWith":
                    return elements.Where(e => !e.GetCategory.Name.StartsWith(value)).ToList();
                case "EndsWith":
                    return elements.Where(e => e.GetCategory.Name.EndsWith(value)).ToList();
                case "DoesNotEndWith":
                    return elements.Where(e => !e.GetCategory.Name.EndsWith(value)).ToList();
                case "Equals":
                case "==":
                    return elements.Where(e => e.GetCategory.Name.Equals(value)).ToList();
                case "DoesNotEqual":
                case "!=":
                    return elements.Where(e => !e.GetCategory.Name.Equals(value)).ToList();
                default:
                    return elements;
            }
        }

    }
}

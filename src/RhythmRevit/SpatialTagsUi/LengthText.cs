// The 3d spatial tags dialog is Revit 2025 and up, with the node it belongs to.
#if R25_OR_GREATER
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Rhythm.SpatialTagsUi
{
    /// <summary>Reads the feet-and-inches the text height box accepts.</summary>
    internal static class LengthText
    {
        private const string Pattern =
            "^\\s*(?<minus>-)?\\s*(((?<feet>\\d+)(?<inch>\\d{2})(?<sixt>\\d{2}))|((?<feet>[\\d.]+)')?[\\s-]*((?<inch>\\d+)?[\\s-]*((?<numer>\\d+)/(?<denom>\\d+))?\")?)\\s*$";

        /// <summary>
        /// The length in inches, or zero when the text is not a length this can read.
        ///
        /// Zero doubles as the failure indicator and as "nothing entered", which suits the one
        /// caller: both mean leave the tag family's own text height alone.
        /// </summary>
        public static double ParseFeetAndInches(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return 0;

            var match = new Regex(Pattern).Match(input);

            if (!match.Success) return 0;

            var sign = match.Groups["minus"].Success ? -1 : 1;

            // Parsed invariantly rather than with the ambient culture, which permits group
            // separators: on a culture that groups with '.', "1.5" came back as fifteen, and a
            // user asking for one and a half feet of text silently got fifteen. The pattern above
            // only ever admits '.' as a decimal point, so invariant parsing is the correct reading
            // of what it matched.
            //
            // TryParse rather than Parse: "1.2.3" matches the pattern but is not a number, and
            // returning zero puts it in front of the user as an unreadable height rather than
            // throwing.
            double feet = 0;

            if (match.Groups["feet"].Success &&
                !double.TryParse(match.Groups["feet"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out feet))
            {
                return 0;
            }

            var inch = match.Groups["inch"].Success ? Convert.ToInt32(match.Groups["inch"].Value) : 0;
            var sixteenths = match.Groups["sixt"].Success ? Convert.ToInt32(match.Groups["sixt"].Value) : 0;
            var numerator = match.Groups["numer"].Success ? Convert.ToInt32(match.Groups["numer"].Value) : 0;
            var denominator = match.Groups["denom"].Success ? Convert.ToInt32(match.Groups["denom"].Value) : 1;

            return sign * (feet * 12 + inch + sixteenths / 16.0 + numerator / Convert.ToDouble(denominator));
        }
    }
}
#endif

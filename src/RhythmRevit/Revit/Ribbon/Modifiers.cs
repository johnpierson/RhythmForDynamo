using System.Windows.Media;
using adWin = Autodesk.Windows;
using Color = DSCore.Color;
using AW = Autodesk.Windows;

namespace Rhythm.Revit.Ribbon
{
    /// <summary>
    /// Wrapper class for ribbon stuff.
    /// </summary>
    public class Modifiers
    {
        private Modifiers()
        {
        }
        /// <summary>
        /// This will rotate your ribbon. Seriously.
        /// </summary>
        /// <param name="rotation">The amount to rotate the ribbon.</param>
        /// <returns name="rotation">The rotated amount.</returns>
        public static int Rotate(int rotation)
        {

            adWin.ComponentManager.Ribbon.RenderTransform
                = new global::System.Windows.Media.RotateTransform(
                    rotation);

            return rotation;
        }
        /// <summary>
        /// This will set the font on your ribbon.
        /// </summary>
        /// <param name="fontName">The font to use.</param>
        /// <returns name="fontName">Thefont used.</returns>
        public static string SetFont(string fontName)
        {
            adWin.RibbonControl ribbon = adWin.ComponentManager.Ribbon;
            ribbon.FontFamily = new global::System.Windows.Media
                .FontFamily(fontName);

            return fontName;
        }
        /// <summary>
        /// This will set the color of your ribbon.
        /// </summary>
        /// <param name="color">The color to use</param>
        /// <returns name="color">The rotated amount.</returns>
        public static Color SetColor(Color color)
        {
            adWin.RibbonControl ribbon = adWin.ComponentManager.Ribbon;
            LinearGradientBrush gradientBrush = new LinearGradientBrush();
            gradientBrush.GradientStops.Add(
                new GradientStop(global::System.Windows.Media.Color.FromRgb(color.Red, color.Green, color.Blue), 0.0));

            gradientBrush.GradientStops.Add(
                new GradientStop(global::System.Windows.Media.Color.FromRgb(color.Red, color.Green, color.Blue), 1));

            foreach (adWin.RibbonTab tab in ribbon.Tabs)
            {
                foreach (adWin.RibbonPanel panel in tab.Panels)
                {
                    panel.CustomPanelBackground = gradientBrush;
                }
            }
            return color;
        }
    }
}

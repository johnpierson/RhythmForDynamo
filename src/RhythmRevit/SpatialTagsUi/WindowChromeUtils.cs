// The 3d spatial tags dialog is Revit 2025 and up, with the node it belongs to.
#if R25_OR_GREATER
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Color = System.Windows.Media.Color;

namespace Rhythm.SpatialTagsUi
{
    /// <summary>
    /// Colours a window's native title bar to match the dialog below it.
    ///
    /// Windows' own caption, tinted, rather than a caption drawn from scratch. A drawn one has to
    /// re-implement minimise and close, the system menu, double-click to maximise, Aero snap and
    /// the Windows 11 snap-layouts flyout, and it has to do all of that again for high contrast,
    /// for every DPI, and for whatever the next Windows does. This asks the desktop window manager
    /// to paint its own caption in three colours instead, and everything else keeps working
    /// because it is still the real caption.
    ///
    /// Windows 11 build 22000 introduced these attributes. On anything older the call returns a
    /// failure result, which is ignored: the window keeps the default caption, which is what it
    /// would have had anyway.
    /// </summary>
    internal static class WindowChromeUtils
    {
        private const int DwmwaBorderColor = 34;
        private const int DwmwaCaptionColor = 35;
        private const int DwmwaTextColor = 36;

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

        /// <summary>
        /// Paints the window's caption in the given colours once it has a handle.
        ///
        /// Safe to call before the window is shown: with no handle yet the work is deferred to
        /// SourceInitialized, because the attributes are set on an HWND and the HWND does not
        /// exist until then.
        /// </summary>
        public static void ApplyCaptionColors(Window window, Color caption, Color text, Color border)
        {
            if (window == null) return;

            var handle = new WindowInteropHelper(window).Handle;

            if (handle == IntPtr.Zero)
            {
                EventHandler onSourceInitialized = null;

                onSourceInitialized = (sender, e) =>
                {
                    window.SourceInitialized -= onSourceInitialized;
                    Apply(new WindowInteropHelper(window).Handle, caption, text, border);
                };

                window.SourceInitialized += onSourceInitialized;
                return;
            }

            Apply(handle, caption, text, border);
        }

        private static void Apply(IntPtr handle, Color caption, Color text, Color border)
        {
            if (handle == IntPtr.Zero) return;

            try
            {
                // Each attribute is an independent request. An older Windows fails all three and
                // the caption stays default; there is no half-tinted state to worry about, because
                // a failed call changes nothing.
                Set(handle, DwmwaCaptionColor, caption);
                Set(handle, DwmwaTextColor, text);
                Set(handle, DwmwaBorderColor, border);
            }
            catch (Exception)
            {
                // A Windows without dwmapi. The dialog is entirely usable with a default title
                // bar, so this is never worth failing over.
            }
        }

        private static void Set(IntPtr handle, int attribute, Color color)
        {
            // COLORREF, which is 0x00BBGGRR: blue and red the other way round from the ARGB these
            // colours arrive as.
            var colorRef = color.R | (color.G << 8) | (color.B << 16);

            DwmSetWindowAttribute(handle, attribute, ref colorRef, sizeof(int));
        }
    }
}
#endif

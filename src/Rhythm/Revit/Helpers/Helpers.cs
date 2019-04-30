using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Autodesk.Revit.UI;
using RevitServices.Persistence;

namespace Rhythm.Revit.Helpers
{
    /// <summary>
    /// Helpers Wrapper
    /// </summary>
    public class Helpers
    {
        private Helpers()
        {
        }
        /// <summary>
        /// This provides a simple user message.
        /// </summary>
        /// <param name="caption">The caption for the window title.</param>
        /// <param name="message">The message to give the user.</param>
        public static void SimpleUserMessage(string caption, string message)
        {
            TaskDialog td = new TaskDialog(caption)
            {
                TitleAutoPrefix = false,
                MainContent = message
            };
            td.Show();
        }
        /// <summary>
        /// This provides a user message with the option to cancel the process downstream. If no is selected the node will return null.
        /// This node was inspired by the work of Matt Nelson, William Wong and Kyle Morin on http://stockroom.mattbenimble.com/.
        /// </summary>
        /// <param name="caption">The caption for the window title.</param>
        /// <param name="message">The message to give the user.</param>
        /// <param name="obj">The object to passthrough.</param>
        /// <returns name = "result">The object.</returns>
        public static object UserMessage(string caption, string message, List<object> obj)
        {
            object result = new List<string>();
            TaskDialog td = new TaskDialog(caption)
            {
                TitleAutoPrefix = false,
                MainContent = message,
                AllowCancellation = true,
                CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No
            };
            TaskDialogResult tdResult = td.Show();
            if (tdResult == TaskDialogResult.Yes)
            {
                result = obj;
            }
            return result;
        }

        /// <summary>
        /// This provides a toggle based on boolean input. Replacement for Rhythm.Toggle.
        /// </summary>
        /// <param name="obj">The object to passthrough.</param>
        /// <param name="toggle">The caption for the window title.</param>
        /// <returns name = "result">The object.</returns>
        public static object Toggle(List<object> obj, bool toggle)
        {
            object result = new List<string>(); 
            
            if (toggle)
            {
                result = obj;
            }
            return result;
        }

        /// <summary>
        /// Creates a full screenshot of the main window.
        /// </summary>
        /// <param name="filepath">The image filepath</param>
        public static void ScreenshotMainWindow(String filepath)
        {
            Bitmap bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            Graphics graphics = Graphics.FromImage(bitmap as Image);

            graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);

            bitmap.Save(filepath, ImageFormat.Jpeg);
        }
        /// <summary>
        /// This returns the temporary path for the current user.
        /// </summary>
        /// <param name="refresh">Optional toggle to refresh the node</param>
        /// <returns></returns>
        public static string CurrentUserTempFolder(bool refresh = true)
        {
            return Path.GetTempPath();
        }
        /// <summary>
        /// This returns the appdata path for the current user.
        /// </summary>
        /// <param name="refresh">Optional toggle to refresh the node</param>
        /// <returns></returns>
        public static string CurrentUserAppData(bool refresh = true)
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        /// <summary>
        /// Returns the domain name of the current user.
        /// </summary>
        /// <param name="refresh">Optional toggle to refresh the node</param>
        /// <returns></returns>
        public static string CurrentUserDomainName(bool refresh = true)
        {
        return Environment.UserDomainName;
        }

        /// <summary>
        /// This allows you to turn off element binding in the DYN.
        /// </summary>
        /// <param name="toggle"></param>
        /// <returns></returns>
        public static bool ToggleElementBinder(bool toggle)
        {
            ElementBinder.IsEnabled = toggle;
            return toggle;
        }
       
    }
}

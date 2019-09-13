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

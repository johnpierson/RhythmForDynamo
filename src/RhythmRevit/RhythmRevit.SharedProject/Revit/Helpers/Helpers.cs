using System;
using System.Collections.Generic;
using Autodesk.Revit.UI;
using Dynamo.Graph.Nodes;
using DynamoServices;
using Revit.Elements;
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
        [NodeCategory("Create")]
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
        /// </summary>
        /// <param name="caption">The caption for the window title.</param>
        /// <param name="message">The message to give the user.</param>
        /// <param name="obj">The object to passthrough.</param>
        /// <returns name = "result">The object.</returns>
        [NodeCategory("Create")]
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
        [NodeCategory("Actions")]
        public static bool ToggleElementBinder(bool toggle)
        {
            ElementBinder.IsEnabled = toggle;
            return toggle;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="runIt"></param>
        public static void PurgeBindings(bool runIt = false)
        {
            if (!runIt) return;
            var lifecycleManager = RevitServices.Persistence.ElementIDLifecycleManager<int>.GetInstance();

            var ids = lifecycleManager.ToString().Split(new string[] { "Element ID ", ":" }, StringSplitOptions.None);

            foreach (var stringId in ids)
            {
                try
                {
                    int intId = Convert.ToInt32(stringId.Trim());
                    var currentElement = ElementSelector.ByElementId(intId, true);

                    lifecycleManager.UnRegisterAssociation(intId, currentElement);
                }
                catch (Exception)
                {
                    //
                }
            }


        }

        /// <summary>
        /// Returns the current Revit version
        /// </summary>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static string CurrentRevitVersion()
        {
            return DocumentManager.Instance.CurrentUIApplication.Application.VersionNumber;
        }

        
    }
}

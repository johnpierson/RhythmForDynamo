using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using RevitServices.Persistence;
using adWin = Autodesk.Windows;

namespace Rhythm.Revit.Ribbon
{
    /// <summary>
    /// Wrapper class for ribbon stuff.
    /// </summary>
    public class RibbonTab
    {
        private RibbonTab()
        {
        }
        /// <summary>
        /// This will rename a tab given a new name.
        /// </summary>
        /// <param name="tab">The tab.</param>
        /// <param name="name">The new name.</param>
        /// <returns name="result">The result..</returns>
        /// <search>
        /// Ribbon.RenameTab
        /// </search>
        public static string SetName(adWin.RibbonTab tab, string name)
        {
            string result = null;
            try
            {
                tab.Title = name;
                result = "Cool beans, it worked.";
            }
            catch (Exception)
            {
                result = "Yeah... that didn't work..";
            }

            return result;
        }
        /// <summary>
        /// This will hide or show the given tab.
        /// </summary>
        /// <param name="tab">The tab to modify.</param>
        /// <param name="toggle">True or false for tab visibility.</param>
        /// <returns name="result">The result..</returns>
        /// <search>
        /// Ribbon.RenameTab
        /// </search>
        public static string SetVisibility(adWin.RibbonTab tab, bool toggle)
        {
            string result = null;
            try
            {
                tab.IsVisible = toggle;
                result = "Cool beans, it worked.";
            }
            catch (Exception)
            {
                result = "Yeah... that didn't work..";
            }

            return result;
        }
        /// <summary>
        /// This will enable or disable the given tab.
        /// </summary>
        /// <param name="tab">The tab name to modify.</param>
        /// <param name="toggle">True or false for tab visibility.</param>
        /// <returns name="result">The result..</returns>
        /// <search>
        /// Ribbon.RenameTab
        /// </search>
        public static string SetEnabled(adWin.RibbonTab tab, bool toggle)
        {
            string result = null;
            try
            {
                tab.IsEnabled = toggle;
                result = "Cool beans, it worked.";
            }
            catch (Exception)
            {
                result = "Yeah... that didn't work..";
            }

            return result;
        }
        /// <summary>
        /// This will give you access to all tabs.
        /// </summary>
        /// <param name="toggle">True or false to refresh collection.</param>
        /// <returns name="ribbonTabs">The ribbon tabs in the application.</returns>
        /// <search>
        /// Ribbon.RenameTab
        /// </search>
        public static List<adWin.RibbonTab> GetTabs(bool toggle)
        {
            var uiapp = DocumentManager.Instance.CurrentUIApplication;
            adWin.RibbonControl ribbon = adWin.ComponentManager.Ribbon;

            List<adWin.RibbonTab> ribbonTabs = new List<adWin.RibbonTab>();
            foreach (adWin.RibbonTab tab in ribbon.Tabs)
            {
                ribbonTabs.Add(tab);
            }

            return ribbonTabs;
        }
        /// <summary>
        /// This will get the tab's name.
        /// </summary>
        /// <param name="tab">The original tab name.</param>
        /// <returns name="name">The result..</returns>
        /// <search>
        /// Ribbon.RenameTab
        /// </search>
        public static string Name(adWin.RibbonTab tab)
        {
            string result = null;
            try
            { 
                result = tab.Title;              
            }
            catch (Exception)
            {
                result = null;
            }

            return result;
        }
        /// <summary>
        /// This will create a temporary tab that disappears on Revit shutdown.
        /// </summary>
        /// <param name="tabName">The new tab name.</param>
        /// <returns name="name">The result..</returns>
        /// <search>
        /// Ribbon.RenameTab
        /// </search>
        [IsVisibleInDynamoLibrary(false)]
        public static adWin.RibbonTab CreateTab(string tabName)
        {
            var uiapp = DocumentManager.Instance.CurrentUIApplication;
            adWin.RibbonControl ribbon = adWin.ComponentManager.Ribbon;

            uiapp.CreateRibbonTab(tabName);

            return ribbon.Tabs.First(t => t.Name == tabName);
        }
        /// <summary>
        /// This will get the tab's visibility status.
        /// </summary>
        /// <param name="tab">The original tab name.</param>
        /// <returns name="name">The result..</returns>
        public static bool Visibility(adWin.RibbonTab tab)
        {
            return tab.IsVisible;
        }
    }
}

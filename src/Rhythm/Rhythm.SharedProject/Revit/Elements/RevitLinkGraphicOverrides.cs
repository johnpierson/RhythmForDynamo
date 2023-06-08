using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.Revit.DB;

namespace Rhythm.Revit.Elements
{
#if Revit2024
    /// <summary>
    /// Override Graphic Settings
    /// </summary>
    public class RevitLinkGraphicsSettings
    {
        #region Internal Properties

        /// <summary>
        /// Internal reference to the Revit Element
        /// </summary>
        internal Autodesk.Revit.DB.RevitLinkGraphicsSettings InternalRevitLinkGraphicsSettings
        {
            get; set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        internal RevitLinkGraphicsSettings(Autodesk.Revit.DB.RevitLinkGraphicsSettings internalRevitLinkGraphicsSettings)
        {
            this.InternalRevitLinkGraphicsSettings = internalRevitLinkGraphicsSettings;
        }

        #endregion

        #region Public static constructors
        /// <summary>
        /// Define link graphic settings by properties. (for now this is just the link visibility setting..)
        /// </summary>
        /// <param name="linkVisibility">Options = ByHostView, ByLinkView, Custom</param>
        /// <returns></returns>
        public static RevitLinkGraphicsSettings ByProperties(string linkVisibility = "ByHostView")
        {
            Autodesk.Revit.DB.RevitLinkGraphicsSettings graphics = new Autodesk.Revit.DB.RevitLinkGraphicsSettings();

            string simpleString = linkVisibility.ToLower();

            switch (simpleString)
            {
                case "byhostview":
                    graphics.LinkVisibilityType = LinkVisibility.ByHostView;
                    break;
                case "bylinkview":
                    graphics.LinkVisibilityType = LinkVisibility.ByLinkView;
                    break;
                case "custom":
                    graphics.LinkVisibilityType = LinkVisibility.Custom;
                    break;
                default:
                    graphics.LinkVisibilityType = LinkVisibility.ByHostView;
                    break;
            }
            

            return new RevitLinkGraphicsSettings(graphics);
        }

        public static string LinkVisibilityType(RevitLinkGraphicsSettings revitLinkGraphicsSettings)
        {
            return revitLinkGraphicsSettings.InternalRevitLinkGraphicsSettings.LinkVisibilityType.ToString();
        }
        #endregion

    }
#endif
}

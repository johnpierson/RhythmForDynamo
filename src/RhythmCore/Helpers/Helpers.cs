using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Autodesk.DesignScript.Runtime;

namespace Rhythm.Helpers
{
    /// <summary>
    /// Wrapper Class for Helpers
    /// </summary>
    public class Helpers
    {
        private Helpers()
        {
        }
        /// <summary>
        /// This provides a toggle based on boolean input. Replacement for Rhythm.Toggle.
        /// </summary>
        /// <param name="obj">The object to passthrough.</param>
        /// <param name="toggle">Toggle the passthrough.</param>
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
        /// This provides a toggle input to select between 2 inputs.
        /// </summary>
        /// <param name="obj1">First choice.</param>
        /// <param name="obj2">Second choice.</param>
        /// <param name="toggle">True for option 1, false for option 2.</param>
        /// <returns name = "result">The object.</returns>
        public static object ThisOrThat(List<object> obj1, List<object> obj2, bool toggle)
        {
            if (toggle)
            {
                return obj1;
            }

            return obj2;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ImportExport
    {
        private ImportExport()
        {
        }
        /// <summary>
        /// Creates a full screenshot of the main window.
        /// </summary>
        /// <param name="filepath">The image filepath</param>
        public static void ScreenshotMainWindow(string filepath)
        {
            Bitmap bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            Graphics graphics = Graphics.FromImage(bitmap);

            graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);

            bitmap.Save(filepath, ImageFormat.Jpeg);
        }
    }

    /// <summary>
    /// Wrapper class for system stuff
    /// </summary>
    public class System
    {
        private System()
        {
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
        /// Returns the current windows user.
        /// </summary>
        /// <param name="refresh">Optional toggle to refresh the node</param>
        /// <returns></returns>
        public static string CurrentUserName(bool refresh = true)
        {
            return Environment.UserName;
        }

        ///// <summary>
        ///// This will move your mouse back and forth slowly while periodic run is on. Suggested periodic run time is 1000ms.
        ///// </summary>
        ///// <param name="runIt">True will cause the mouse to move on it's own.</param>
        //[CanUpdatePeriodically(true)]
        //public static void JiggleMouse(bool runIt)
        //{
        //    if (runIt)
        //    {
        //        Random ran = new Random();
        //        SetCursorPos(Cursor.Position.X + ran.Next(-5, 5), Cursor.Position.Y + ran.Next(-5, 5));
        //    }

        //}
        /// <summary>
        /// This will move your mouse back and forth slowly while periodic run is on.
        /// </summary>
        /// <param name="runIt">True will cause the mouse to move on it's own.</param>
        /// <param name="interval">Time between movements (in seconds).</param>
        public static void JiggleMouse(bool runIt = false, double interval = 0)
        {
            if (!runIt || interval == 0)
            {
                JiggleTimer.Elapsed -= TimerOnTick;
                if (JiggleTimer.Enabled)
                {
                    JiggleTimer.Stop();
                }
                return;
            }

            JiggleTimer.Interval = interval * 1000;
            if (JiggleTimer.Enabled)
            {
                JiggleTimer.Stop();
            }
            JiggleTimer.Elapsed -= TimerOnTick;
            JiggleTimer.Elapsed += TimerOnTick;
            JiggleTimer.Start();


        }
        private static readonly global::System.Timers.Timer JiggleTimer = new global::System.Timers.Timer(2000);


        private static void TimerOnTick(object sender, EventArgs e)
        {
            Random ran = new Random();
            SetCursorPos(Cursor.Position.X + ran.Next(-15, 15), Cursor.Position.Y + ran.Next(-15, 15));
        }

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int x, int y);
    }
}

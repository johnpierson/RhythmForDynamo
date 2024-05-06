using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Dynamo.Graph.Nodes;

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
        [NodeCategory("Actions")]
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
        [NodeCategory("Actions")]
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
        [NodeCategory("Create")]
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
        [NodeCategory("Query")]
        public static string CurrentUserTempFolder(bool refresh = true)
        {
            return Path.GetTempPath();
        }

        /// <summary>
        /// This returns the appdata path for the current user.
        /// </summary>
        /// <param name="refresh">Optional toggle to refresh the node</param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static string CurrentUserAppData(bool refresh = true)
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        /// <summary>
        /// Returns the domain name of the current user.
        /// </summary>
        /// <param name="refresh">Optional toggle to refresh the node</param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static string CurrentUserDomainName(bool refresh = true)
        {
            return Environment.UserDomainName;
        }

        /// <summary>
        /// Send the given string to the clipboard
        /// </summary>
        /// <param name="str"></param>
        [NodeCategory("Actions")]
        public static void SendToClipboard(string str)
        {
            Clipboard.SetText(str);
        }

        /// <summary>
        /// Returns the current windows user.
        /// </summary>
        /// <param name="refresh">Optional toggle to refresh the node</param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static string CurrentUserName(bool refresh = true)
        {
            return Environment.UserName;
        }
        /// <summary>
        /// Returns the current computer name.
        /// </summary>
        /// <param name="refresh">Optional toggle to refresh the node</param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static string MachineName(bool refresh = true)
        {
            return Environment.MachineName;
        }

        /// <summary>
        /// Creates a .Resx (resources) file from a directory of images for a Dynamo package. This helps greatly when trying to add icons to zerotouch nodes and the like.
        /// </summary>
        /// <param name="imageDirectoryPath">The image directory path to search within.</param>
        /// <param name="resxFilePath">Where to write the resx file.</param>
        public static void CreateResxImageFile(string imageDirectoryPath, string resxFilePath)
        {
            var largeImages = Directory.GetFiles(imageDirectoryPath, "*Large.png");
            var smallImages = Directory.GetFiles(imageDirectoryPath, "*Small.png");

            string body = string.Empty;

            foreach (var image in smallImages)
            {
                byte[] imageArray = File.ReadAllBytes(image);
                string base64ImageRepresentation = Convert.ToBase64String(imageArray);

                var name = Path.GetFileNameWithoutExtension(image);

                string firstReplacement = ResxImageInfo.Replace("$name$", name);
                string secondReplacement = firstReplacement.Replace("$base64$", base64ImageRepresentation);
                body += secondReplacement;
            }
            foreach (var image in largeImages)
            {
                byte[] imageArray = File.ReadAllBytes(image);
                string base64ImageRepresentation = Convert.ToBase64String(imageArray);

                var name = Path.GetFileNameWithoutExtension(image);
                string firstReplacement = ResxImageInfo.Replace("$name$", name);
                string secondReplacement = firstReplacement.Replace("$base64$", base64ImageRepresentation);
                body += secondReplacement;
            }

            string resxContents = $"{ResxHead}{body}{ResxEnd}";

            File.WriteAllText(resxFilePath,resxContents);
        }
        #region ResxFileStuff

        private static string ResxHead =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<root>\r\n  <!-- \r\n    Microsoft ResX Schema \r\n    \r\n    Version 2.0\r\n    \r\n    The primary goals of this format is to allow a simple XML format \r\n    that is mostly human readable. The generation and parsing of the \r\n    various data types are done through the TypeConverter classes \r\n    associated with the data types.\r\n    \r\n    Example:\r\n    \r\n    ... ado.net/XML headers & schema ...\r\n    <resheader name=\"resmimetype\">text/microsoft-resx</resheader>\r\n    <resheader name=\"version\">2.0</resheader>\r\n    <resheader name=\"reader\">System.Resources.ResXResourceReader, System.Windows.Forms, ...</resheader>\r\n    <resheader name=\"writer\">System.Resources.ResXResourceWriter, System.Windows.Forms, ...</resheader>\r\n    <data name=\"Name1\"><value>this is my long string</value><comment>this is a comment</comment></data>\r\n    <data name=\"Color1\" type=\"System.Drawing.Color, System.Drawing\">Blue</data>\r\n    <data name=\"Bitmap1\" mimetype=\"application/x-microsoft.net.object.binary.base64\">\r\n        <value>[base64 mime encoded serialized .NET Framework object]</value>\r\n    </data>\r\n    <data name=\"Icon1\" type=\"System.Drawing.Icon, System.Drawing\" mimetype=\"application/x-microsoft.net.object.bytearray.base64\">\r\n        <value>[base64 mime encoded string representing a byte array form of the .NET Framework object]</value>\r\n        <comment>This is a comment</comment>\r\n    </data>\r\n                \r\n    There are any number of \"resheader\" rows that contain simple \r\n    name/value pairs.\r\n    \r\n    Each data row contains a name, and value. The row also contains a \r\n    type or mimetype. Type corresponds to a .NET class that support \r\n    text/value conversion through the TypeConverter architecture. \r\n    Classes that don't support this are serialized and stored with the \r\n    mimetype set.\r\n    \r\n    The mimetype is used for serialized objects, and tells the \r\n    ResXResourceReader how to depersist the object. This is currently not \r\n    extensible. For a given mimetype the value must be set accordingly:\r\n    \r\n    Note - application/x-microsoft.net.object.binary.base64 is the format \r\n    that the ResXResourceWriter will generate, however the reader can \r\n    read any of the formats listed below.\r\n    \r\n    mimetype: application/x-microsoft.net.object.binary.base64\r\n    value   : The object must be serialized with \r\n            : System.Runtime.Serialization.Formatters.Binary.BinaryFormatter\r\n            : and then encoded with base64 encoding.\r\n    \r\n    mimetype: application/x-microsoft.net.object.soap.base64\r\n    value   : The object must be serialized with \r\n            : System.Runtime.Serialization.Formatters.Soap.SoapFormatter\r\n            : and then encoded with base64 encoding.\r\n\r\n    mimetype: application/x-microsoft.net.object.bytearray.base64\r\n    value   : The object must be serialized into a byte array \r\n            : using a System.ComponentModel.TypeConverter\r\n            : and then encoded with base64 encoding.\r\n    -->\r\n  <xsd:schema id=\"root\" xmlns=\"\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">\r\n    <xsd:import namespace=\"http://www.w3.org/XML/1998/namespace\" />\r\n    <xsd:element name=\"root\" msdata:IsDataSet=\"true\">\r\n      <xsd:complexType>\r\n        <xsd:choice maxOccurs=\"unbounded\">\r\n          <xsd:element name=\"metadata\">\r\n            <xsd:complexType>\r\n              <xsd:sequence>\r\n                <xsd:element name=\"value\" type=\"xsd:string\" minOccurs=\"0\" />\r\n              </xsd:sequence>\r\n              <xsd:attribute name=\"name\" use=\"required\" type=\"xsd:string\" />\r\n              <xsd:attribute name=\"type\" type=\"xsd:string\" />\r\n              <xsd:attribute name=\"mimetype\" type=\"xsd:string\" />\r\n              <xsd:attribute ref=\"xml:space\" />\r\n            </xsd:complexType>\r\n          </xsd:element>\r\n          <xsd:element name=\"assembly\">\r\n            <xsd:complexType>\r\n              <xsd:attribute name=\"alias\" type=\"xsd:string\" />\r\n              <xsd:attribute name=\"name\" type=\"xsd:string\" />\r\n            </xsd:complexType>\r\n          </xsd:element>\r\n          <xsd:element name=\"data\">\r\n            <xsd:complexType>\r\n              <xsd:sequence>\r\n                <xsd:element name=\"value\" type=\"xsd:string\" minOccurs=\"0\" msdata:Ordinal=\"1\" />\r\n                <xsd:element name=\"comment\" type=\"xsd:string\" minOccurs=\"0\" msdata:Ordinal=\"2\" />\r\n              </xsd:sequence>\r\n              <xsd:attribute name=\"name\" type=\"xsd:string\" use=\"required\" msdata:Ordinal=\"1\" />\r\n              <xsd:attribute name=\"type\" type=\"xsd:string\" msdata:Ordinal=\"3\" />\r\n              <xsd:attribute name=\"mimetype\" type=\"xsd:string\" msdata:Ordinal=\"4\" />\r\n              <xsd:attribute ref=\"xml:space\" />\r\n            </xsd:complexType>\r\n          </xsd:element>\r\n          <xsd:element name=\"resheader\">\r\n            <xsd:complexType>\r\n              <xsd:sequence>\r\n                <xsd:element name=\"value\" type=\"xsd:string\" minOccurs=\"0\" msdata:Ordinal=\"1\" />\r\n              </xsd:sequence>\r\n              <xsd:attribute name=\"name\" type=\"xsd:string\" use=\"required\" />\r\n            </xsd:complexType>\r\n          </xsd:element>\r\n        </xsd:choice>\r\n      </xsd:complexType>\r\n    </xsd:element>\r\n  </xsd:schema>\r\n  <resheader name=\"resmimetype\">\r\n    <value>text/microsoft-resx</value>\r\n  </resheader>\r\n  <resheader name=\"version\">\r\n    <value>2.0</value>\r\n  </resheader>\r\n  <resheader name=\"reader\">\r\n    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>\r\n  </resheader>\r\n  <resheader name=\"writer\">\r\n    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>\r\n  </resheader>\r\n  <assembly alias=\"System.Drawing\" name=\"System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\" />\r\n";

        private static string ResxImageInfo =
            "<data name=\"$name$\" type=\"System.Drawing.Bitmap, System.Drawing\" mimetype=\"application/x-microsoft.net.object.bytearray.base64\">\r\n    <value>\r\n$base64$\r\n</value>\r\n  </data>";
        private static string ResxEnd = "</root>";
        #endregion



        /// <summary>
        /// This will move your mouse back and forth slowly while toggled true.
        /// </summary>
        /// <param name="runIt">True will cause the mouse to move on it's own.</param>
        /// <param name="interval">Time between movements (in seconds).</param>
        [NodeCategory("Create")]
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

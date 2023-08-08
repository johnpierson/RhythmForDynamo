using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace RhythmViewExtension
{
    internal class Global
    {
        internal static string PackageBinFolder => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        internal static string PackageExtraFolder => PackageBinFolder.Replace("bin", "extra");
    }
}

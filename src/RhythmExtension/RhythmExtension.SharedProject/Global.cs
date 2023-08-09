using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RhythmExtension
{
    internal class Global
    {
        internal static Assembly ExecutingAssembly = Assembly.GetExecutingAssembly();
        internal static string[] EmbeddedLibraries =
            ExecutingAssembly.GetManifestResourceNames().Where(x => x.EndsWith(".dll")).ToArray();
        internal static string PackageBinFolder => Path.GetDirectoryName(ExecutingAssembly.Location);
        internal static string PackageExtraFolder => PackageBinFolder.Replace("bin", "extra");
    }
}

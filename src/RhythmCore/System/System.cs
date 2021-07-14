using System;
using System.IO;
using System.IO.Compression;

namespace Rhythm.System
{
    public class System
    {
        private System() { }
        /// <summary>
        /// This will compress (zip) a given directory
        /// </summary>
        /// <param name="directoryName">The directory to try to zip.</param>
        public static string Compress(string directoryName)
        {
            string zipName = $"{directoryName}.zip";
            try
            {
                ZipFile.CreateFromDirectory(directoryName, zipName);

                return $"{zipName} archive created.";
            }
            catch (Exception e)
            {
                return $"Zip file failed: Error Message {e.Message}";
            }
            
        }
    }
}

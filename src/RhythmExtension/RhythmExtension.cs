using Dynamo.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RhythmExtension
{
    /// <summary>
    ///Rhythm extension for hot loading the appropriate DLLs on the fly
    /// </summary>
    public class Extension : IExtension
    {
        public bool readyCalled = false;
        
        public string Name => "RhythmExtension";

        public string UniqueId => "CD7A123A-7121-4FA4-99D3-D941CD049EA5";

        public void Dispose()
        {

        }
        /// <summary>
        /// Ready is called when the DynamoModel is finished being built, or when the extension is installed
        /// sometime after the DynamoModel is already built. ReadyParams provide access to references like the
        /// CurrentWorkspace.
        /// </summary>
        /// <param name="rp"></param>
        public void Ready(ReadyParams rp)
        {
            this.readyCalled = true;
        }


        public void Shutdown()
        {
        }

        public void Startup(StartupParams sp)
        {
            if (!File.Exists(Global.RhythmRevitDll))
            {
                var revitApi = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name.Equals(("RevitAPI")));

                if (revitApi != null)
                {
                    var version = revitApi.GetName().Version.Major;
                    try
                    {
                        FirstRunSetup(sp, $"{version}");
                    }
                    catch (Exception)
                    {
                       //suppress, this didn't work
                    }
                   
                }
            }
        }
        /// <summary>
        /// We get here because someone is missing the freakin DLLs
        /// </summary>
        /// <param name="version"></param>
        private void FirstRunSetup(StartupParams sp,string version)
        {
            // Get resource name
            var resourceName = Global.EmbeddedLibraries.FirstOrDefault(x => x.Contains(version));
            if (resourceName == null)
            {
                return;
            }

            // Load assembly from resource
            using (var stream = Global.ExecutingAssembly.GetManifestResourceStream(resourceName))
            {
                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);

                File.WriteAllBytes(Global.RhythmRevitDll, bytes);
            }

            var assembly = Assembly.Load(Global.RhythmRevitDll);

            sp.LibraryLoader.LoadNodeLibrary(assembly);


            //rewrite the json
            File.WriteAllText(Global.PackageJson, Global.PackageJsonText);
        }

    }
}

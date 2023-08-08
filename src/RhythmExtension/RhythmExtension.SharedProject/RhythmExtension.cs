using Dynamo.Extensions;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RhythmExtension
{
    /// <summary>
    ///Rhythm extension for hot loading the appropriate DLLs on the fly
    /// </summary>
    public class Extension : IExtension
    {
        public List<NodeModel> nodes = new List<NodeModel>();
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
            //TODO: make this check if Dynamo UI is loaded. If not load the library
            var asses = AppDomain.CurrentDomain.GetAssemblies().ToList();
            sp.LibraryLoader.LoadNodeLibrary(Assembly.LoadFrom("C:\\Users\\johnpierson\\AppData\\Roaming\\Dynamo\\Dynamo Core\\2.17\\packages\\Rhythm\\bin\\RhythmCore.dll"));
        }
    }
}

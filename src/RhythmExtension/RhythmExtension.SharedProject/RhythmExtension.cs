using Dynamo.Extensions;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dynamo.Applications;
using Dynamo.Interfaces;
using Dynamo.Engine;
using System.Runtime.InteropServices;
using Dynamo.Applications.Models;

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
            RevitDynamoModel dynamoModel = DynamoRevit.RevitDynamoModel;

            if (dynamoModel != null)
            {
                var version = dynamoModel.Context;

                LoadPackage(version.Replace("Revit ", ""));
            }
        }

        internal void LoadPackage(string version)
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

                File.WriteAllBytes(Path.Combine(Global.PackageBinFolder,"RhythmRevit.dll"), bytes);

                //AppDomain.CurrentDomain.Load(bytes);
            }
        }

    }
}

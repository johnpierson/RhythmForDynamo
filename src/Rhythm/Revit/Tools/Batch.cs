using Autodesk.DesignScript.Runtime;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.UI;
using RevitServices.Persistence;

namespace Rhythm.Revit.Tools
{
    /// <summary>
    /// Wrapper class for batchers.
    /// </summary>
    public class Batch
    {
        private Batch()
        { }
        /// <summary>
        /// This tool with batch upgrade all the Revit families in a directory and delete the backup files that are generated.
        /// </summary>
        /// <param name="directoryPath">The directory to read for ALL families. Including subdirectories.</param>
        /// <returns name="Succesfully Upgraded">Files that were upgraded.</returns>
        /// <returns name="Not So Succesfully Upgraded">Files that were not upgraded.</returns>
        /// <search>
        /// Application.OpenDocumentFile, rhythm
        /// </search>
        //this is the node Application.OpenDocumentFile
        [MultiReturn(new[] { "Succesfully Upgraded", "Not So Succesfully Upgraded" })]
        public static Dictionary<string, object> UpgradeFamilies(string directoryPath)
        {
            //get UIAPP and APP
            var uiapp = DocumentManager.Instance.CurrentUIApplication;
            var app = uiapp.Application;
            //read files from directory
            string[] allfiles = System.IO.Directory.GetFiles(directoryPath, "*.rfa*", System.IO.SearchOption.AllDirectories);           
            //create a new form!
            FamilyUpgradeForm statusBar = new FamilyUpgradeForm("Rhythm - Bulk Upgrade Families", "Upgrading family {0} of " + allfiles.Length.ToString(), "Batch Family Upgrayedd", allfiles.Length);
            //declare lists to output
            List<string> upgradedFiles = new List<string>();
            //List<string> notUpgradedFiles = new List<string>();

            //build a file info to see if any of the families are read only
            List<FileInfo> fileInfos = new List<FileInfo>();
            foreach (var file in allfiles)
            {
                fileInfos.Add(new FileInfo(file));
            }
            //grab the files that can be upgraded and place in new list (reduces memory consumption)
            string[] toUpgrade = new List<string>(fileInfos.Where(fi => !fi.IsReadOnly).Select(fi => fi.ToString())).ToArray();
            //add the ones that are read only to the list
            List<string> notUpgradedFiles = new List<string>(fileInfos.Where(fi => fi.IsReadOnly).Select(fi => fi.ToString()));

            //flag for read only
            bool flag = fileInfos.Any(f => f.IsReadOnly);
            //show message for Read Only
            if (flag)
            {
                TaskDialog.Show("Family Upgrade Alert",
                    "Some of the families you are trying to upgrade are read only. These will be output in the failed category. If you want to upgrade them, please change from read only and try again.");
            }

            //something to increase each time
            int step = 0;
            //loop through each file and  try to upgrade. This is important because we open and close each file on its own.
            while (step < toUpgrade.Length)
            {
                statusBar.Activate();
                try
                {
                    if (statusBar.getAbortFlag())
                        break;
                    Autodesk.Revit.DB.Document document = app.OpenDocumentFile(toUpgrade[step]);
                    document.Close(true);
                    upgradedFiles.Add(toUpgrade[step]);
                }
                catch
                {
                    Autodesk.Revit.DB.Document document = app.OpenDocumentFile(toUpgrade[step]);
                    document.Close(false);
                    notUpgradedFiles.Add(toUpgrade[step]);
                }
                ++step;
                statusBar.Increment();
            }
            statusBar.Close();
            //clean up some backup files
            string[] backupFiles = System.IO.Directory.GetFiles(directoryPath, "*.0001*", System.IO.SearchOption.AllDirectories);
            foreach (string file in backupFiles)
            {
                File.Delete(file);
            }
            //returns the outputs
            var outInfo = new Dictionary<string, object>
            {
                { "Successfully Upgraded", upgradedFiles},
                { "Not So Successfully Upgraded", notUpgradedFiles}
            };
            return outInfo;
        } 
    }
}

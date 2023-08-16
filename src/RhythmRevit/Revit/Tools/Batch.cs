using Autodesk.DesignScript.Runtime;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
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
        /// <param name="suffix">Optional suffix to save the files as. Useful for read-only files.</param>
        /// <returns name="Succesfully Upgraded">Files that were upgraded.</returns>
        /// <returns name="Not So Succesfully Upgraded">Files that were not upgraded.</returns>
        /// <search>
        /// Application.OpenDocumentFile, rhythm
        /// </search>
        //this is the node Application.OpenDocumentFile
        [MultiReturn(new[] { "Successfully Upgraded", "Not So Successfully Upgraded" })]
        public static Dictionary<string, object> UpgradeFamilies(string directoryPath, string suffix = "")
        {
            //get UIAPP and APP
            var uiapp = DocumentManager.Instance.CurrentUIApplication;
            var app = uiapp.Application;
            //read files from directory
            string[] allfiles = global::System.IO.Directory.GetFiles(directoryPath, "*.rfa*", global::System.IO.SearchOption.AllDirectories);           
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
            var upgradeList = string.IsNullOrWhiteSpace(suffix)
                ? fileInfos.Where(fi => !fi.IsReadOnly).Select(fi => fi.ToString()).ToArray()
                : fileInfos.Select(fi => fi.ToString()).ToArray();
            var toUpgrade = upgradeList;

            //add the ones that are read only to the list
            var notUpgradeList = string.IsNullOrWhiteSpace(suffix)
                ? fileInfos.Where(fi => fi.IsReadOnly).Select(fi => fi.ToString()).ToList()
                : new List<string>();
            List<string> notUpgradedFiles = notUpgradeList;

            //flag for read only
            bool flag = fileInfos.Any(f => f.IsReadOnly);
            //show message for Read Only
            if (flag && string.IsNullOrWhiteSpace(suffix))
            {
                TaskDialog.Show("Family Upgrade Alert",
                    "Some of the families you are trying to upgrade are read only. These will be output in the failed category. If you want to upgrade them, please change from read only and try again. Or select a suffix to add to the families to do a save as.");
            }

            //something to increase each time
            int step = 0;
            //loop through each file and  try to upgrade. This is important because we open and close each file on its own.
            while (step < toUpgrade.Length)
            {
                statusBar.Activate();
                try
                {
                    if (statusBar.GetAbortFlag())
                        break;
                    Autodesk.Revit.DB.Document document = app.OpenDocumentFile(toUpgrade[step]);

                    if (!string.IsNullOrWhiteSpace(suffix))
                    {
                        var fileName = document.Title;
                        var newFileName = $"{fileName}{suffix}";
                        string newFilePath = document.PathName.Replace(fileName, newFileName);
                        document.SaveAs(newFilePath);
                        foreach (Document openDoc in app.Documents)
                        {
                            if (openDoc.Title.Equals(newFileName))
                            {
                                openDoc.Close(false);
                            }
                        }
                        upgradedFiles.Add(newFilePath);
                    }
                    else
                    {
                        document.Close(true);
                        upgradedFiles.Add(toUpgrade[step]);
                    }
                  
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
            string[] backupFiles = global::System.IO.Directory.GetFiles(directoryPath, "*.0001*", global::System.IO.SearchOption.AllDirectories);
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

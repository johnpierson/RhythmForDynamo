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
        /// Revit writes backups alongside the original as "Door.0001.rfa", "Door.0002.rfa" and so
        /// on. They are not families to upgrade, and pre-existing ones are not ours to delete.
        /// </summary>
        [IsVisibleInDynamoLibrary(false)]
        private static bool IsRevitBackupFile(string path)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(
                System.IO.Path.GetFileName(path),
                @"\.\d{4}\.rfa$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// This tool will batch upgrade all the Revit families in a directory, and delete the backup files that this run generates. Backup files that already existed are left alone.
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
            //read files from directory. The "*.rfa*" pattern also matches Revit's own backups
            //(Door.0001.rfa), which were being opened and "upgraded" before being deleted below.
            string[] allfiles = System.IO.Directory
                .GetFiles(directoryPath, "*.rfa*", System.IO.SearchOption.AllDirectories)
                .Where(f => !IsRevitBackupFile(f))
                .ToArray();

            //Record the backups that already existed so the cleanup at the end can remove only the
            //ones this run created. The previous code deleted every *.0001* under the directory
            //tree - including backups of unrelated projects, and families this tool never touched.
            var preExistingBackups = new HashSet<string>(
                System.IO.Directory.GetFiles(directoryPath, "*.0001*", System.IO.SearchOption.AllDirectories),
                System.StringComparer.OrdinalIgnoreCase);
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
                //Held outside the try so the catch can close it without reopening the file.
                Autodesk.Revit.DB.Document document = null;
                try
                {
                    if (statusBar.GetAbortFlag())
                        break;
                    document = app.OpenDocumentFile(toUpgrade[step]);

                    if (!string.IsNullOrWhiteSpace(suffix))
                    {
                        //Build the new path from its parts. String.Replace substituted the family
                        //name everywhere it appeared, so C:\Families\Door\Door.rfa with suffix _v2
                        //became C:\Families\Door_v2\Door_v2.rfa - a directory that does not exist.
                        var sourcePath = document.PathName;
                        var newFileName = System.IO.Path.GetFileNameWithoutExtension(sourcePath) + suffix;
                        string newFilePath = System.IO.Path.Combine(
                            System.IO.Path.GetDirectoryName(sourcePath),
                            newFileName + System.IO.Path.GetExtension(sourcePath));
                        document.SaveAs(newFilePath);
                        foreach (Autodesk.Revit.DB.Document openDoc in app.Documents)
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

                    document = null;
                }
                catch
                {
                    //Do not reopen the file here. If the failure was the open itself - a corrupt or
                    //newer-version family - opening it again throws again, and that second
                    //exception escapes the loop, aborting the whole batch partway through and
                    //leaving the always-on-top progress form stranded over Revit.
                    if (document != null)
                    {
                        try
                        {
                            document.Close(false);
                        }
                        catch
                        {
                            //The document is already unusable; recording the failure is all we can do.
                        }
                    }
                    notUpgradedFiles.Add(toUpgrade[step]);
                }
                ++step;
                statusBar.Increment();
            }
            statusBar.Close();
            //Clean up only the backups this run created. Anything that was already on disk when we
            //started is somebody else's file, and deleting it is not this node's business.
            string[] backupFiles = System.IO.Directory
                .GetFiles(directoryPath, "*.0001*", System.IO.SearchOption.AllDirectories)
                .Where(f => !preExistingBackups.Contains(f))
                .ToArray();
            foreach (string file in backupFiles)
            {
                try
                {
                    File.Delete(file);
                }
                catch (IOException)
                {
                    //Locked by another process. Leaving a backup behind is harmless; throwing here
                    //would discard the results the batch has already collected.
                }
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

using System;
using Autodesk.Revit.DB;
using Revit.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for group types
    /// </summary>
    public class GroupTypes
    {
        private GroupTypes()
        {
        }
#if Revit2024
        /// <summary>
        /// Reload a group type from a given location. Note: This will override group types in the current file!
        /// </summary>
        /// <param name="group">The group to reload the type for</param>
        /// <param name="reloadFrom">Location path</param>
        /// <param name="includeAttachedDetails">Include attached details with this group type?</param>
        /// <param name="includeGrids">Include grids with this group type?</param>
        /// <param name="includeLevels">Include levels with this group type?></param>
        /// <returns name="groupType">The updated group type definition</returns>
        /// <exception cref="Exception"></exception>
        public static global::Revit.Elements.Element ReloadGroupType(global::Revit.Elements.Group group, string reloadFrom, bool includeAttachedDetails = true, bool includeGrids = false, bool includeLevels = false)
        {
            Autodesk.Revit.DB.Group internalGroup = group.InternalElement as Autodesk.Revit.DB.Group;
            Autodesk.Revit.DB.GroupType internalGroupType = internalGroup.GroupType;
            var doc = internalGroup.Document;

            //define our load options
            GroupLoadOptions options = new GroupLoadOptions();
            options.ReplaceDuplicatedGroups = true;
            options.IncludeAttachedDetails = includeAttachedDetails;
            options.IncludeGrids = includeGrids;
            options.IncludeLevels = includeLevels;
            options.SetDuplicateTypeNamesHandler(new Application.HideAndAcceptDuplicateTypeNamesHandler());

            //start our transaction and try to reload the group type
            TransactionManager.Instance.EnsureInTransaction(doc);
            try
            {
                internalGroupType.LoadFrom(reloadFrom, options);
                TransactionManager.Instance.TransactionTaskDone();

                return internalGroupType.ToDSType(true);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
           
        }
#endif

     
    }
}
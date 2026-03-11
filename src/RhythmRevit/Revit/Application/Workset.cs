using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Rhythm.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rhythm.Revit.Application
{
    /// <summary>
    /// Wrapper class for workset nodes.
    /// </summary>
    public class Workset
    {
        private Workset()
        { }

        /// <summary>
        /// Creates a new user workset in the active document.
        /// </summary>
        /// <param name="name">The name of the workset to create.</param>
        /// <returns name="workset">The newly created workset.</returns>
        /// <search>
        /// workset, create, workshared, rhythm
        /// </search>
        [NodeCategory("Actions")]
        public static Autodesk.Revit.DB.Workset Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Workset name cannot be null or empty.", nameof(name));

            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            if (!doc.IsWorkshared)
                throw new InvalidOperationException(
                    "The current document is not workshared. Worksets can only be created or modified in workshared models.");

            // Check for duplicate workset name
            var existingWorksets = new FilteredWorksetCollector(doc)
                .OfKind(WorksetKind.UserWorkset)
                .ToList();

            if (existingWorksets.Any(w => string.Equals(w.Name, name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException(
                    $"A workset with the name \"{name}\" already exists in the document.");

            TransactionManager.Instance.EnsureInTransaction(doc);
            Autodesk.Revit.DB.Workset createdWorkset = Autodesk.Revit.DB.Workset.Create(doc, name);
            TransactionManager.Instance.TransactionTaskDone();

            return createdWorkset;
        }

        /// <summary>
        /// Renames an existing user workset in the active document.
        /// </summary>
        /// <param name="workset">The workset to rename.</param>
        /// <param name="newName">The new name for the workset.</param>
        /// <returns name="workset">The renamed workset.</returns>
        /// <search>
        /// workset, rename, workshared, rhythm
        /// </search>
        [NodeCategory("Actions")]
        public static Autodesk.Revit.DB.Workset Rename(Autodesk.Revit.DB.Workset workset, string newName)
        {
            if (workset == null)
                throw new ArgumentNullException(nameof(workset), "Workset cannot be null.");

            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("New workset name cannot be null or empty.", nameof(newName));

            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            if (!doc.IsWorkshared)
                throw new InvalidOperationException(
                    "The current document is not workshared. Worksets can only be created or modified in workshared models.");

            WorksetTable worksetTable = doc.GetWorksetTable();

            // Check for duplicate workset name
            var existingWorksets = new FilteredWorksetCollector(doc)
                .OfKind(WorksetKind.UserWorkset)
                .ToList();

            if (existingWorksets.Any(w => w.Id != workset.Id && string.Equals(w.Name, newName, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException(
                    $"A workset with the name \"{newName}\" already exists in the document.");

            TransactionManager.Instance.EnsureInTransaction(doc);
            WorksetTable.RenameWorkset(doc, workset.Id, newName);
            TransactionManager.Instance.TransactionTaskDone();

            return worksetTable.GetWorkset(workset.Id);
        }

        /// <summary>
        /// Returns all user worksets in the active document.
        /// </summary>
        /// <param name="document">The document to retrieve worksets from. Defaults to the current document.</param>
        /// <returns name="worksets">All user worksets in the document.</returns>
        /// <search>
        /// workset, getall, list, workshared, rhythm
        /// </search>
        [NodeCategory("Query")]
        public static List<Autodesk.Revit.DB.Workset> GetAll(global::Revit.Application.Document document = null)
        {
            Autodesk.Revit.DB.Document doc = document.ToRevitType();

            
            if (!doc.IsWorkshared)
                throw new InvalidOperationException(
                    "The current document is not workshared. Workset operations require a workshared model.");

            return new FilteredWorksetCollector(doc)
                .OfKind(WorksetKind.UserWorkset)
                .ToList();
        }

        /// <summary>
        /// Retrieves a workset by name.
        /// </summary>
        /// <param name="name">The name of the workset to retrieve.</param>
        /// <returns name="workset">The workset matching the given name.</returns>
        /// <search>
        /// workset, byname, find, workshared, rhythm
        /// </search>
        [NodeCategory("Query")]
        public static Autodesk.Revit.DB.Workset ByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Workset name cannot be null or empty.", nameof(name));

            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            if (!doc.IsWorkshared)
                throw new InvalidOperationException(
                    "The current document is not workshared. Workset operations require a workshared model.");

            var workset = new FilteredWorksetCollector(doc)
                .OfKind(WorksetKind.UserWorkset)
                .FirstOrDefault(w => string.Equals(w.Name, name, StringComparison.OrdinalIgnoreCase));

            if (workset == null)
                throw new InvalidOperationException(
                    $"No workset with the name \"{name}\" was found in the document.");

            return workset;
        }

        /// <summary>
        /// Deletes a user workset from the active document.
        /// </summary>
        /// <param name="workset">The workset to delete.</param>
        /// <returns name="success">Whether the deletion succeeded.</returns>
        /// <search>
        /// workset, delete, remove, workshared, rhythm
        /// </search>
        [NodeCategory("Actions")]
        public static bool Delete(Autodesk.Revit.DB.Workset workset)
        {
            if (workset == null)
                throw new ArgumentNullException(nameof(workset), "Workset cannot be null.");

            if (workset.Kind != WorksetKind.UserWorkset)
                throw new InvalidOperationException("Only user worksets can be deleted.");

            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            if (!doc.IsWorkshared)
                throw new InvalidOperationException(
                    "The current document is not workshared. Workset operations require a workshared model.");

#if R22_OR_GREATER
            TransactionManager.Instance.EnsureInTransaction(doc);
            var settings = new DeleteWorksetSettings();
            WorksetTable.DeleteWorkset(doc, workset.Id, settings);
            TransactionManager.Instance.TransactionTaskDone();
            return true;
#else
            throw new NotSupportedException("Workset.Delete is only supported in Revit 2022 or later.");
#endif
        }

        /// <summary>
        /// Returns whether a workset is visible in a specific view.
        /// </summary>
        /// <param name="workset">The workset to query.</param>
        /// <param name="view">The view to check visibility in.</param>
        /// <returns name="visible">Whether the workset is visible in the view.</returns>
        /// <search>
        /// workset, visible, visibility, view, rhythm
        /// </search>
        [NodeCategory("Query")]
        public static bool VisibleInView(Autodesk.Revit.DB.Workset workset, global::Revit.Elements.Element view)
        {
            if (workset == null)
                throw new ArgumentNullException(nameof(workset), "Workset cannot be null.");

            if (view == null)
                throw new ArgumentNullException(nameof(view), "View cannot be null.");

            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            if (!doc.IsWorkshared)
                throw new InvalidOperationException(
                    "The current document is not workshared. Workset operations require a workshared model.");

            Autodesk.Revit.DB.View internalView = view.InternalElement as Autodesk.Revit.DB.View;
            if (internalView == null)
                throw new ArgumentException("The provided element is not a valid Revit view.", nameof(view));
            WorksetVisibility visibility = internalView.GetWorksetVisibility(workset.Id);

            if (visibility == WorksetVisibility.Visible)
                return true;

            if (visibility == WorksetVisibility.Hidden)
                return false;

            // WorksetVisibility.UseGlobalSetting - fall back to the workset's default visibility
            return workset.IsVisibleByDefault;
        }

        /// <summary>
        /// Sets workset visibility in a view.
        /// </summary>
        /// <param name="workset">The workset to modify visibility for.</param>
        /// <param name="view">The view in which to set visibility.</param>
        /// <param name="visible">True to show the workset, false to hide it.</param>
        /// <returns name="view">The updated view.</returns>
        /// <search>
        /// workset, setvisible, visibility, view, rhythm
        /// </search>
        [NodeCategory("Actions")]
        public static global::Revit.Elements.Element SetVisibleInView(Autodesk.Revit.DB.Workset workset, global::Revit.Elements.Element view, bool visible)
        {
            if (workset == null)
                throw new ArgumentNullException(nameof(workset), "Workset cannot be null.");

            if (view == null)
                throw new ArgumentNullException(nameof(view), "View cannot be null.");

            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            if (!doc.IsWorkshared)
                throw new InvalidOperationException(
                    "The current document is not workshared. Workset operations require a workshared model.");

            Autodesk.Revit.DB.View internalView = view.InternalElement as Autodesk.Revit.DB.View;
            if (internalView == null)
                throw new ArgumentException("The provided element is not a valid Revit view.", nameof(view));
            WorksetVisibility visibilityValue = visible ? WorksetVisibility.Visible : WorksetVisibility.Hidden;

            TransactionManager.Instance.EnsureInTransaction(doc);
            internalView.SetWorksetVisibility(workset.Id, visibilityValue);
            TransactionManager.Instance.TransactionTaskDone();

            return view;
        }
    }
}

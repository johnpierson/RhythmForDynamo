using System;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Rhythm.Revit.Document
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
    }
}

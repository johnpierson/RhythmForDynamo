// The 3d spatial tagging node is Revit 2025 and up. The tag family it places is saved in Revit
// 2025, and Revit loads families forward but never backward, so there is nothing for an earlier
// version to place.
#if R25_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Interop;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using Rhythm.Revit.Tools.SpatialTagging;
using Rhythm.SpatialTagsUi;
using FamilyInstance = Autodesk.Revit.DB.FamilyInstance;

namespace Rhythm.Revit.Tools
{
    public partial class Tools
    {
        /// <summary>
        /// Open the 3d Spatial Tags dialog, and report what it did.
        ///
        /// This is the 3d Spatial Tags add-in as a single node: the same dialog, drawn the same
        /// way, doing the same work. Set runIt to true and it opens over Revit. Pick rooms or
        /// spaces, optionally a linked model to read them out of, the phase, the tag family type
        /// and a text height, then press Create / Update Tags. Close it and the node reports what
        /// the run did.
        ///
        /// The tag family loads itself. Opening the dialog puts the one Rhythm ships into the model
        /// if it is not already there, and replaces an older copy of it with the current one, so
        /// the family type list is never empty and never offers a family this node cannot write to.
        /// Type parameter values already set on it — text height, materials, visibility — are left
        /// as the user set them.
        ///
        /// Re-running follows the model rather than piling up duplicates: a tag records the element
        /// it belongs to, and the link instance it was read through, in the family's
        /// SpatialElementId parameter. Rooms with a blank name or number are skipped, because there
        /// would be nothing to put in the tag, and so are unplaced ones, because there is nowhere
        /// to put it. Unbounded and redundant rooms still have a marker, so they are still tagged,
        /// and the dialog says how many. In a workshared model a tag owned by somebody else is
        /// reported, never overwritten. Tags whose room has been deleted are counted and left
        /// alone, because deleting elements out of somebody's model is not this node's business.
        ///
        /// The dialog opens whenever this node evaluates with runIt true, so drive it from a
        /// Boolean toggle rather than from something that changes on every graph run.
        /// </summary>
        /// <param name="runIt">Set to true to open the dialog. False changes nothing and opens nothing.</param>
        /// <returns name="tags">The tags the run created or updated.</returns>
        /// <returns name="created">How many tags were placed.</returns>
        /// <returns name="updated">How many existing tags were moved and rewritten.</returns>
        /// <returns name="skipped">How many rooms or spaces could not be tagged.</returns>
        /// <returns name="orphanedTags">How many tags in the model name a room that has been deleted.</returns>
        /// <returns name="message">What the run did, in words.</returns>
        /// <search>
        /// 3d room tags, 3d space tags, spatial tags, rhythm
        /// </search>
        [MultiReturn(new[] { "tags", "created", "updated", "skipped", "orphanedTags", "message" })]
        [NodeCategory("Actions")]
        public static Dictionary<string, object> ThreeDeeSpatialTags(bool runIt)
        {
            if (!runIt)
            {
                return Report(null, "runIt is false, so nothing was opened and nothing was written to the model.");
            }

            // Dynamo evaluates a Revit graph on Revit's own UI thread, which is what lets any of
            // these nodes touch the API at all. A dialog needs that thread to be STA as well, and
            // if some host ever evaluates elsewhere, saying so is better than the access violation
            // that showing a window from the wrong apartment produces.
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            {
                throw new InvalidOperationException(
                    "The 3d Spatial Tags dialog cannot be opened from this thread. Run the graph from Dynamo inside Revit.");
            }

            var model = new SpatialTagModel();

            if (model.Doc == null)
            {
                throw new InvalidOperationException("There is no open Revit document to tag.");
            }

            var viewModel = new SpatialTagDialogViewModel(model);
            var dialog = new SpatialTagDialog { DataContext = viewModel };

            // Owned by Revit's main window, so it stays in front of it, minimises with it, and does
            // not turn up as a second entry in the taskbar.
            var owner = RevitMainWindow();

            if (owner != IntPtr.Zero)
            {
                new WindowInteropHelper(dialog).Owner = owner;
            }

            dialog.ShowDialog();

            if (viewModel.LastFailure != null && viewModel.LastResult == null)
            {
                return Report(null, "The run failed: " + viewModel.LastFailure);
            }

            return Report(viewModel.LastResult, null);
        }

        /// <summary>
        /// Revit's main window handle, or zero if it cannot be reached.
        ///
        /// Read from the ribbon component manager, which Rhythm already talks to elsewhere, rather
        /// than from a UIApplication: this node never has one to hand and does not otherwise need
        /// it. A dialog with no owner still works, so a failure here is not worth reporting.
        /// </summary>
        private static IntPtr RevitMainWindow()
        {
            try
            {
                return Autodesk.Windows.ComponentManager.ApplicationWindow;
            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// The node's outputs. A null result means no run happened, which is what closing the
        /// dialog without pressing the button looks like, and is reported as such rather than as a
        /// run that placed nothing.
        /// </summary>
        private static Dictionary<string, object> Report(TaggingResult result, string overrideMessage)
        {
            if (result == null)
            {
                return new Dictionary<string, object>
                {
                    { "tags", new List<object>() },
                    { "created", 0 },
                    { "updated", 0 },
                    { "skipped", 0 },
                    { "orphanedTags", 0 },
                    { "message", overrideMessage ?? "The dialog was closed without placing any tags." }
                };
            }

            return new Dictionary<string, object>
            {
                { "tags", result.Tags.Select(t => (object)t.ToDSType(true)).ToList() },
                { "created", result.Created },
                { "updated", result.Updated },
                { "skipped", result.Skipped },
                { "orphanedTags", result.OrphanedTags },
                { "message", overrideMessage ?? Describe(result) }
            };
        }

        /// <summary>What the run did, in the words the dialog's status card uses.</summary>
        private static string Describe(TaggingResult result)
        {
            if (result.MissingParameter != null)
            {
                return "The selected tag family type has no \"" + result.MissingParameter +
                       "\" parameter, so nothing was placed.";
            }

            var message = new System.Text.StringBuilder();

            message.Append(result.Created).Append(result.Created == 1 ? " tag placed, " : " tags placed, ");
            message.Append(result.Updated).Append(" updated.");

            if (result.SkippedNotPlaced > 0)
            {
                message.Append(" ").Append(result.SkippedNotPlaced)
                    .Append(result.SkippedNotPlaced == 1 ? " is unplaced" : " are unplaced")
                    .Append(", so there is nowhere to put a tag.");
            }

            if (result.SkippedMissingNameOrNumber > 0)
            {
                message.Append(" ").Append(result.SkippedMissingNameOrNumber)
                    .Append(result.SkippedMissingNameOrNumber == 1 ? " has" : " have")
                    .Append(" a blank name or number, so there would be nothing in the tag.");
            }

            if (result.Unbounded > 0)
            {
                message.Append(" ").Append(result.Unbounded)
                    .Append(result.Unbounded == 1 ? " tagged element is" : " tagged elements are")
                    .Append(" unbounded or redundant, so the tag sits on the marker rather than in a room.");
            }

            if (result.SkippedNotEditable > 0)
            {
                message.Append(" ").Append(result.SkippedNotEditable)
                    .Append(result.SkippedNotEditable == 1 ? " tag is" : " tags are")
                    .Append(" owned by another user and were left alone.");
            }

            if (result.MigratedLegacyTags > 0)
            {
                message.Append(" ").Append(result.MigratedLegacyTags).Append(result.MigratedLegacyTags == 1
                    ? " tag was written before link instances were part of a tag's identity, and has been rewritten with it."
                    : " tags were written before link instances were part of a tag's identity, and have been rewritten with it.");
                message.Append(" That only happens once.");
            }

            if (result.OrphanedTags > 0)
            {
                message.Append(" ").Append(result.OrphanedTags).Append(result.OrphanedTags == 1
                    ? " tag names an element that has been deleted, so its text is now wrong. It was counted, not removed."
                    : " tags name elements that have been deleted, so their text is now wrong. They were counted, not removed.");
            }

            if (result.SuppressedDuplicateWarnings > 0)
            {
                message.Append(" ").Append(result.SuppressedDuplicateWarnings)
                    .Append(" duplicate-instance warning")
                    .Append(result.SuppressedDuplicateWarnings == 1 ? " was" : "s were")
                    .Append(" hidden, because this run stacked tags on its own previous ones.");
            }

            return message.ToString();
        }
    }
}
#endif

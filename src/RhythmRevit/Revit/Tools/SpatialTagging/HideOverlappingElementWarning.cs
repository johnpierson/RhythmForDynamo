// The 3d spatial tagging node, and the planning it is built on, are Revit 2025 and up.
// The tag family it places is saved in Revit 2025, and Revit loads families forward but never
// backward, so there is nothing for an earlier version to place.
#if R25_OR_GREATER
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace Rhythm.Revit.Tools.SpatialTagging
{
    /// <summary>
    /// Hides the duplicate-instance warning for tags this run placed on top of each other, and only
    /// those.
    ///
    /// Deleting every duplicate-instance warning raised at commit would be worse than the noise: a
    /// tagging run surfaces warnings about geometry it had nothing to do with, and a coordination
    /// model quietly losing its duplicate warnings is exactly the kind of data-quality erosion this
    /// node is supposed to help with.
    ///
    /// What is legitimately suppressed is the node arguing with itself. With updateExisting off the
    /// node places a fresh set every time, which means stacking tags on the previous ones on
    /// purpose. Revit says so once per pair, and the user already asked for it.
    /// </summary>
    internal class HideOverlappingElementWarning : IFailuresPreprocessor
    {
        private readonly HashSet<ElementId> _ownTags;

        /// <param name="ownTags">The tags this run created or updated.</param>
        public HideOverlappingElementWarning(IEnumerable<ElementId> ownTags)
        {
            _ownTags = new HashSet<ElementId>(ownTags ?? Enumerable.Empty<ElementId>());
        }

        /// <summary>How many warnings were suppressed, so the run can say so.</summary>
        public int SuppressedDuplicateWarnings { get; private set; }

        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            foreach (var failure in failuresAccessor.GetFailureMessages())
            {
                if (failure.GetFailureDefinitionId() != BuiltInFailures.OverlapFailures.DuplicateInstances) continue;

                var failing = failure.GetFailingElementIds();

                // Every element the warning names has to be one of ours. A warning that involves
                // anything else is somebody's real problem and stays on screen.
                if (failing.Count == 0 || !failing.All(id => _ownTags.Contains(id))) continue;

                failuresAccessor.DeleteWarning(failure);
                SuppressedDuplicateWarnings++;
            }

            // Everything else is handled the way it normally would be.
            return FailureProcessingResult.Continue;
        }
    }
}
#endif

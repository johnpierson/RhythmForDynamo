// The 3d spatial tagging node, and the planning it is built on, are Revit 2025 and up.
// The tag family it places is saved in Revit 2025, and Revit loads families forward but never
// backward, so there is nothing for an earlier version to place.
#if R25_OR_GREATER
using System;
using System.Collections.Generic;

namespace Rhythm.Revit.Tools.SpatialTagging
{
    /// <summary>
    /// Decides what a run will do, before it does any of it.
    ///
    /// Deliberately free of the Revit API. The rules about what gets skipped, what gets updated and
    /// what gets left alone are the ones most likely to be wrong and the ones a user notices when
    /// they are, so they are a value that can be read on its own rather than a shape that only
    /// exists in the middle of writing to a document.
    /// </summary>
    internal static class TagPlanner
    {
        /// <summary>
        /// Works out, for each spatial element, whether it gets a new tag, an update to an existing
        /// one, or nothing.
        /// </summary>
        /// <param name="sources">The elements in scope, already in host coordinates.</param>
        /// <param name="existingTags">Tags already in the document. Ignored unless updating.</param>
        /// <param name="updateExisting">
        /// Whether to adopt matching tags. Off means every run places a fresh set, which is what
        /// leaves duplicates behind on purpose.
        /// </param>
        public static TagRunPlan Plan(
            IList<SpatialElementSnapshot> sources,
            IList<ExistingTagSnapshot> existingTags,
            bool updateExisting)
        {
            var plan = new TagRunPlan();

            if (sources == null) return plan;

            var considered = updateExisting ? existingTags : null;
            var byIdentity = IndexByStoredValue(considered);
            var legacyBySourceId = IndexLegacyBySourceId(considered);
            var claimed = new HashSet<string>(StringComparer.Ordinal);

            foreach (var source in sources)
            {
                if (source == null) continue;

                // Order matters: an unplaced element is rejected before its name is looked at, and
                // both are rejected before any existing tag is considered.
                if (!source.IsPlaced)
                {
                    plan.Operations.Add(Skip(source, SkipReason.NotPlaced));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(source.Name) || string.IsNullOrWhiteSpace(source.Number))
                {
                    plan.Operations.Add(Skip(source, SkipReason.MissingNameOrNumber));
                    continue;
                }

                var identity = source.Identity;
                bool isLegacyMigration;
                var match = FindMatch(identity, byIdentity, legacyBySourceId, claimed, out isLegacyMigration);

                if (match != null)
                {
                    if (!match.IsEditable)
                    {
                        plan.Operations.Add(Skip(source, SkipReason.NotEditable));
                        continue;
                    }

                    claimed.Add(match.TagId);

                    plan.Operations.Add(new TagOperation
                    {
                        Kind = TagOperationKind.Update,
                        Source = source,
                        ExistingTagId = match.TagId,
                        IsLegacyMigration = isLegacyMigration
                    });

                    continue;
                }

                plan.Operations.Add(new TagOperation
                {
                    Kind = TagOperationKind.Create,
                    Source = source
                });
            }

            plan.OrphanedTagCount = CountOrphans(existingTags, claimed);

            return plan;
        }

        /// <summary>
        /// The tag this element already has, if it has one.
        ///
        /// Tried in two passes. The exact identity first. That is the normal case, and for a host
        /// element it is also the only case, because a host element's stored form has always been
        /// its bare id.
        ///
        /// Then, only for elements read through a link, a tag carrying the bare id and nothing
        /// else. Those were written before link instances were part of the identity, and refusing
        /// to adopt them would hand every existing user a duplicate set on their first run after
        /// upgrading. Adopting one rewrites it with the full identity, so a second placement of the
        /// same link finds no legacy tag left to claim and gets its own set.
        /// </summary>
        private static ExistingTagSnapshot FindMatch(
            TagSourceIdentity identity,
            IDictionary<string, ExistingTagSnapshot> byIdentity,
            IDictionary<string, ExistingTagSnapshot> legacyBySourceId,
            HashSet<string> claimed,
            out bool isLegacyMigration)
        {
            isLegacyMigration = false;

            ExistingTagSnapshot exact;
            if (byIdentity.TryGetValue(identity.ToStoredValue(), out exact) && !claimed.Contains(exact.TagId))
            {
                return exact;
            }

            if (!identity.IsFromLink) return null;

            ExistingTagSnapshot legacy;
            if (legacyBySourceId.TryGetValue(identity.SourceId ?? string.Empty, out legacy) && !claimed.Contains(legacy.TagId))
            {
                isLegacyMigration = true;
                return legacy;
            }

            return null;
        }

        /// <summary>
        /// Tags keyed by exactly what is written on them.
        ///
        /// First one wins, which is what a linear search for the first match would do, so a model
        /// that somehow carries two tags for one room behaves as it always has. Building the index
        /// once also takes the matching from a scan per element to a lookup per element, which
        /// matters on a model with thousands of both.
        ///
        /// Tags with no stored value are left out entirely: they match nothing, and treating a null
        /// as a key would let two of them collide with each other.
        /// </summary>
        private static Dictionary<string, ExistingTagSnapshot> IndexByStoredValue(IList<ExistingTagSnapshot> existingTags)
        {
            var index = new Dictionary<string, ExistingTagSnapshot>(StringComparer.Ordinal);

            if (existingTags == null) return index;

            foreach (var tag in existingTags)
            {
                if (tag == null) continue;

                TagSourceIdentity identity;
                bool isLegacy;
                if (!TagSourceIdentity.TryResolve(tag.StoredSourceId, tag.SourceLinkInstanceId, out identity, out isLegacy)) continue;

                var key = identity.ToStoredValue();

                if (!index.ContainsKey(key)) index.Add(key, tag);
            }

            return index;
        }

        /// <summary>Tags carrying a bare element id and nothing else, keyed by it.</summary>
        private static Dictionary<string, ExistingTagSnapshot> IndexLegacyBySourceId(IList<ExistingTagSnapshot> existingTags)
        {
            var index = new Dictionary<string, ExistingTagSnapshot>(StringComparer.Ordinal);

            if (existingTags == null) return index;

            foreach (var tag in existingTags)
            {
                if (tag == null) continue;

                TagSourceIdentity identity;
                bool isLegacy;
                if (!TagSourceIdentity.TryResolve(tag.StoredSourceId, tag.SourceLinkInstanceId, out identity, out isLegacy)) continue;
                if (!isLegacy) continue;

                if (!index.ContainsKey(identity.SourceId)) index.Add(identity.SourceId, tag);
            }

            return index;
        }

        /// <summary>
        /// How many tags name an element that is no longer there.
        ///
        /// Only tags the caller has told us about. It decides which are in the scope being tagged,
        /// because a tag for a room in a different phase is not an orphan. A tag this run just
        /// adopted is never one either, whatever the caller thinks: it has a live element sitting
        /// in front of it.
        /// </summary>
        private static int CountOrphans(IList<ExistingTagSnapshot> existingTags, HashSet<string> claimed)
        {
            if (existingTags == null) return 0;

            var orphans = 0;

            foreach (var tag in existingTags)
            {
                if (tag == null || !tag.SourceMissing) continue;
                if (claimed.Contains(tag.TagId)) continue;

                orphans++;
            }

            return orphans;
        }

        private static TagOperation Skip(SpatialElementSnapshot source, SkipReason reason)
        {
            return new TagOperation
            {
                Kind = TagOperationKind.Skip,
                Source = source,
                Reason = reason
            };
        }
    }
}
#endif

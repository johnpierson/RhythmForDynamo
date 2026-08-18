// The 3d spatial tagging node, and the planning it is built on, are Revit 2025 and up.
// The tag family it places is saved in Revit 2025, and Revit loads families forward but never
// backward, so there is nothing for an earlier version to place.
#if R25_OR_GREATER
using System;

namespace Rhythm.Revit.Tools.SpatialTagging
{
    /// <summary>
    /// What a tag is a tag *of*: a spatial element, and the link instance it was read through.
    ///
    /// The element's own id is not enough. Two instances of one linked file share a single link
    /// document, so the same room hands back byte-identical ids through both — and a run for the
    /// second placement would find the first placement's tags, drag them across, and leave the
    /// first untagged. Nothing was reported, because as far as the matching was concerned it had
    /// found its tag.
    ///
    /// The link instance is therefore part of the identity. Host elements have no link, and their
    /// stored form is the bare element id, which is exactly what tags carried before — so every
    /// existing host tag still matches itself and nothing has to be migrated.
    ///
    /// The identity is written twice, deliberately. The tag family carries it split across
    /// SourceDocumentId and SourceLinkInstanceId, where Revit can schedule and filter on it, and
    /// composed into the SpatialElementId text parameter, which is what matching reads and what
    /// every tag placed before those parameters existed already carries. Reading prefers the split
    /// form and falls back to the composite, so a tag from any version resolves.
    /// </summary>
    internal struct TagSourceIdentity : IEquatable<TagSourceIdentity>
    {
        /// <summary>
        /// Marks a stored value as carrying a link instance. Chosen so it cannot be mistaken for
        /// the bare element id that older tags carry: those are Revit UniqueIds, which are hex and
        /// hyphens and never contain a colon.
        /// </summary>
        private const string LinkPrefix = "link:";

        private const char Separator = '|';

        public TagSourceIdentity(string sourceId, string linkInstanceId)
        {
            SourceId = sourceId;
            LinkInstanceId = string.IsNullOrWhiteSpace(linkInstanceId) ? null : linkInstanceId;
        }

        /// <summary>The spatial element's own UniqueId, in whichever document it lives.</summary>
        public string SourceId { get; }

        /// <summary>The link instance it was read through, or null when it is a host element.</summary>
        public string LinkInstanceId { get; }

        public bool IsFromLink
        {
            get { return LinkInstanceId != null; }
        }

        /// <summary>
        /// What goes on the tag. A host element stores its bare id, unchanged from what this tool
        /// has always written.
        /// </summary>
        public string ToStoredValue()
        {
            return IsFromLink ? LinkPrefix + LinkInstanceId + Separator + SourceId : SourceId;
        }

        /// <summary>
        /// Reads back what a tag carries.
        /// </summary>
        /// <param name="stored">What the tag carries, read out of its SpatialElementId parameter.</param>
        /// <param name="identity">The element, and the link instance it was read through.</param>
        /// <param name="isLegacy">
        /// True when the value is a bare element id with no link instance in it. For a host tag
        /// that is simply the current format; for a tag that came from a link it is a tag written
        /// before this identity existed, and a candidate for migration.
        /// </param>
        public static bool TryParse(string stored, out TagSourceIdentity identity, out bool isLegacy)
        {
            identity = default(TagSourceIdentity);
            isLegacy = false;

            if (string.IsNullOrWhiteSpace(stored)) return false;

            if (!stored.StartsWith(LinkPrefix, StringComparison.Ordinal))
            {
                identity = new TagSourceIdentity(stored, null);
                isLegacy = true;
                return true;
            }

            var body = stored.Substring(LinkPrefix.Length);
            var separator = body.IndexOf(Separator);

            // A prefix with nothing usable after it is not something this tool wrote. Treated as
            // unreadable rather than guessed at, so it matches nothing and is left alone.
            if (separator <= 0 || separator == body.Length - 1) return false;

            identity = new TagSourceIdentity(body.Substring(separator + 1), body.Substring(0, separator));
            return true;
        }

        /// <summary>
        /// The identity a tag records, preferring the parameters the family carries for it and
        /// falling back to the composite value.
        ///
        /// The element id only ever lives in the composite, so that is always parsed; the split
        /// parameters contribute the link instance. A tag written before those parameters existed
        /// simply has nothing in them, and resolves from the composite alone.
        /// </summary>
        public static bool TryResolve(
            string storedSourceId,
            string sourceLinkInstanceId,
            out TagSourceIdentity identity,
            out bool isLegacy)
        {
            if (!TryParse(storedSourceId, out identity, out isLegacy)) return false;

            if (string.IsNullOrWhiteSpace(sourceLinkInstanceId)) return true;

            // The split parameter wins where the two disagree: it is the one Revit schedules, so it
            // is the one a user would have seen and trusted.
            identity = new TagSourceIdentity(identity.SourceId, sourceLinkInstanceId);
            isLegacy = false;

            return true;
        }

        public bool Equals(TagSourceIdentity other)
        {
            return string.Equals(SourceId, other.SourceId, StringComparison.Ordinal)
                   && string.Equals(LinkInstanceId, other.LinkInstanceId, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is TagSourceIdentity && Equals((TagSourceIdentity)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = SourceId == null ? 0 : SourceId.GetHashCode();
                return (hash * 397) ^ (LinkInstanceId == null ? 0 : LinkInstanceId.GetHashCode());
            }
        }

        public override string ToString()
        {
            return ToStoredValue();
        }
    }
}
#endif

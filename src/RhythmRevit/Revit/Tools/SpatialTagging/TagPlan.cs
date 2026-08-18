// The 3d spatial tagging node, and the planning it is built on, are Revit 2025 and up.
// The tag family it places is saved in Revit 2025, and Revit loads families forward but never
// backward, so there is nothing for an earlier version to place.
#if R25_OR_GREATER
using System.Collections.Generic;

namespace Rhythm.Revit.Tools.SpatialTagging
{
    /// <summary>What a run intends to do about one spatial element.</summary>
    internal enum TagOperationKind
    {
        Create,
        Update,
        Skip
    }

    /// <summary>Why an element is not getting a tag.</summary>
    internal enum SkipReason
    {
        None,

        /// <summary>No point location. An unplaced room has nowhere to put a tag.</summary>
        NotPlaced,

        /// <summary>A blank name or number. There would be nothing to write in the tag.</summary>
        MissingNameOrNumber,

        /// <summary>
        /// The matching tag belongs to somebody else, or has moved on in central. Placing a
        /// replacement on top would leave the model with two tags for one room.
        /// </summary>
        NotEditable
    }

    /// <summary>
    /// A location in host-document coordinates.
    ///
    /// Plain doubles rather than Revit's XYZ so that planning stays free of the API it is about to
    /// drive. The link transform, when there is one, is applied on the way in.
    /// </summary>
    internal struct TagPoint
    {
        public TagPoint(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; }
        public double Y { get; }
        public double Z { get; }
    }

    /// <summary>
    /// Everything the planner needs to know about one room or space, read out of the model before
    /// any decision is made.
    /// </summary>
    internal class SpatialElementSnapshot
    {
        /// <summary>Identifies the element this tag is for. Matched against a tag's stored id.</summary>
        public string SourceId { get; set; }

        /// <summary>
        /// The link instance this element was read through, or null for a host element. Part of the
        /// identity because two instances of one linked file hand back the same element ids.
        /// </summary>
        public string LinkInstanceId { get; set; }

        /// <summary>The element and the link instance together — what a tag records.</summary>
        public TagSourceIdentity Identity
        {
            get { return new TagSourceIdentity(SourceId, LinkInstanceId); }
        }

        public string Name { get; set; }
        public string Number { get; set; }

        /// <summary>Whether the element has a point location at all.</summary>
        public bool IsPlaced { get; set; }

        /// <summary>Where the tag goes, already in host coordinates.</summary>
        public TagPoint Point { get; set; }

        /// <summary>
        /// Zero for a room that is unbounded, redundant or unplaced. Carried so the caller and the
        /// planner can agree on one definition of "taggable" rather than counting separately and
        /// disagreeing.
        /// </summary>
        public double Area { get; set; }
    }

    /// <summary>A tag already in the model, and whether this user is allowed to touch it.</summary>
    internal class ExistingTagSnapshot
    {
        public string TagId { get; set; }

        /// <summary>The composite value written on the tag, or null if it carries none.</summary>
        public string StoredSourceId { get; set; }

        /// <summary>
        /// The link instance recorded in the family's own parameter, where it has one. Empty or
        /// null on a host tag, and on any tag written before that parameter existed.
        /// </summary>
        public string SourceLinkInstanceId { get; set; }

        public bool IsEditable { get; set; }

        /// <summary>
        /// Whether the element this tag names has been deleted from the document it came from.
        ///
        /// Answered by the caller, because it takes a document lookup — and answered only for tags
        /// in the scope currently being tagged. A tag for a room in another phase has not been
        /// orphaned just because this run is not about it.
        /// </summary>
        public bool SourceMissing { get; set; }
    }

    /// <summary>One decision, for one element.</summary>
    internal class TagOperation
    {
        public TagOperationKind Kind { get; set; }
        public SpatialElementSnapshot Source { get; set; }

        /// <summary>Which tag to update. Null for anything other than an update.</summary>
        public string ExistingTagId { get; set; }

        public SkipReason Reason { get; set; }

        /// <summary>
        /// Whether this update adopts a tag written before link instances were part of the
        /// identity. The tag is rewritten with the full identity, so it only happens once.
        /// </summary>
        public bool IsLegacyMigration { get; set; }
    }

    /// <summary>Everything a run intends to do, and what it noticed on the way.</summary>
    internal class TagRunPlan
    {
        public List<TagOperation> Operations { get; set; }

        public TagRunPlan()
        {
            Operations = new List<TagOperation>();
        }

        /// <summary>
        /// Tags whose spatial element no longer exists. Counted and reported, not deleted: removing
        /// elements from somebody's model is not something to do without being asked.
        /// </summary>
        public int OrphanedTagCount { get; set; }
    }
}
#endif

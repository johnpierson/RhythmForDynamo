// The 3d spatial tags dialog is Revit 2025 and up, with the node it belongs to.
#if R25_OR_GREATER
namespace Rhythm.SpatialTagsUi
{
    /// <summary>
    /// What the dialog remembers between openings: the target, the tag type and the text height.
    ///
    /// The add-in kept these in user settings, so they survived Revit restarting. This keeps them
    /// in memory for the length of the Dynamo session instead. A Dynamo package writing a settings
    /// file into a shared bin folder is a support question nobody wants, and the thing this is
    /// really for is re-opening the dialog on the same graph a minute later, which memory covers.
    ///
    /// A graph that wants these choices pinned should say so in the graph, which is what the
    /// headless inputs on the node were for before the dialog replaced them.
    /// </summary>
    internal static class SpatialTagSession
    {
        /// <summary>0 for rooms, 1 for spaces.</summary>
        public static int TargetIndex { get; set; }

        /// <summary>The position in the tag family type list, clamped by the caller.</summary>
        public static int FamilySymbolIndex { get; set; }

        /// <summary>The text height exactly as typed, so it reads back the way it was entered.</summary>
        public static string TextHeight { get; set; }
    }
}
#endif

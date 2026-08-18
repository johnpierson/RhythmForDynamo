// The 3d spatial tagging node, and the planning it is built on, are Revit 2025 and up.
// The tag family it places is saved in Revit 2025, and Revit loads families forward but never
// backward, so there is nothing for an earlier version to place.
#if R25_OR_GREATER
using Autodesk.Revit.DB;

namespace Rhythm.Revit.Tools.SpatialTagging
{
    /// <summary>
    /// Answers Revit's "this family is already in the project" question, so it is not asked.
    ///
    /// It has to be answered by something. Calling LoadFamily without load options puts Revit's own
    /// dialog on screen the moment a model already carries a family of this name, and the user gets
    /// a prompt about a file they never chose to load, in the middle of opening a different dialog.
    ///
    /// The definition is taken, the values are not. Those are two separate answers and they pull in
    /// opposite directions here:
    ///
    /// Taking the definition is the whole point. The family already in a model is very often the
    /// older one Rhythm has always shipped, which has no SpatialElementId parameter — so a run
    /// against it rolls back and reports a missing parameter. The newer definition is what puts
    /// that parameter there, and with it every existing tag becomes something a later run can find
    /// and update rather than duplicate.
    ///
    /// Keeping the values is what stops that being rude. Type parameter values are the user's:
    /// their text height, their materials, their visibility switches. Overwriting them would resize
    /// and restyle every tag already standing in the model as a side effect of opening a dialog,
    /// which is not what anybody asked for.
    /// </summary>
    internal class OverwriteFamilyDefinition : IFamilyLoadOptions
    {
        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {
            overwriteParameterValues = false;

            return true;
        }

        public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
        {
            // FamilySource.Family, so the definition being loaded wins over the copy in the project,
            // which is the same answer OnFamilyFound gives and for the same reason.
            source = FamilySource.Family;
            overwriteParameterValues = false;

            return true;
        }
    }
}
#endif

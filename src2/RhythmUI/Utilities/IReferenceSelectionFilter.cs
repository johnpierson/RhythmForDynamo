
using Autodesk.Revit.UI.Selection;

namespace RhythmUI.Utilities
	{
	/// <summary>
	/// Used as a flag to show "this is a filter that will only filter for references;
	/// the AllowElement-Method always passes true.
	/// Instances of IElementSelectionFilter and IReferenceSelectionFilter can only be combined
	/// with an "LogicalAnd"; combining them with "or" will result in a filter that will let everything pass.
	/// </summary>
	public interface IReferenceSelectionFilter : ISelectionFilter
		{}
	}
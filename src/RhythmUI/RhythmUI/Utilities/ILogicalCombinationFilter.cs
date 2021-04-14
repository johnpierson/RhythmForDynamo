
using Autodesk.Revit.UI.Selection;

namespace RhythmUI.Utilities
	{
	/// <summary>
	/// Interface for logical filters. Allows the user to read and set the executeAll-Property
	/// </summary>
	public interface ILogicalCombinationFilter : ISelectionFilter
		{
		/// <summary>
		/// If true, all filters are executed, even if the result is already defined.
		/// Default is false
		/// </summary>
		bool ExecuteAll { get; set; }
		}
	}
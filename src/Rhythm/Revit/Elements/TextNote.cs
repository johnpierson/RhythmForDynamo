using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for textnotes.
    /// </summary>
    public class TextNotes
    {
        private TextNotes()
        { }
        /// <summary>
        /// This node will convert the text note to upper with formatting.
        /// </summary>
        /// <param name="textNote">The text note to convert.</param>
        /// <returns name="textNote">The converted text note.</returns>
        /// <search>
        /// textnote, toupper, rhythm
        /// </search>
        public static global::Revit.Elements.Element ToUpper(global::Revit.Elements.Element textNote)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.TextNote internalNote = (Autodesk.Revit.DB.TextNote)textNote.InternalElement;

            //Get the text from the text box
            //we obtain formatted text and modify the caps instead of the string. Preserves formatting.
            FormattedText formattedText = internalNote.GetFormattedText();
            formattedText.SetAllCapsStatus(true);
            //Change all the text to upper case and reassign to the text box
            //using formatted text allows for the formatting to stay - John
            TransactionManager.Instance.EnsureInTransaction(doc);
            internalNote.SetFormattedText(formattedText);
            TransactionManager.Instance.TransactionTaskDone();

            return textNote;
        }
        /// <summary>
        /// This node will convert the text note to lower with formatting.
        /// </summary>
        /// <param name="textNote">The text note to convert.</param>
        /// <returns name="textNote">The converted text note.</returns>
        /// <search>
        /// textnote, tolower, rhythm
        /// </search>
        public static global::Revit.Elements.Element ToLower(global::Revit.Elements.Element textNote)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.TextNote internalNote = (Autodesk.Revit.DB.TextNote)textNote.InternalElement;

            //Get the text from the text box
            //we obtain formated text and modify the caps instead of the string. Preserves formatting. - John
            FormattedText formattedText = internalNote.GetFormattedText();
            formattedText.SetAllCapsStatus(false);
            //Change all the text to upper case and reassign to the text box
            //using formatted text allows for the formatting to stay - John
            TransactionManager.Instance.EnsureInTransaction(doc);
            internalNote.SetFormattedText(formattedText);
            TransactionManager.Instance.TransactionTaskDone();

            return textNote;
        }
        /// <summary>
        /// This node will return all of the leaders associated with the text note.
        /// </summary>
        /// <param name="textNote">The text note to get leaders from.</param>
        /// <returns name="leaders">The leaders..</returns>
        /// <search>
        /// textnote, getleaders, rhythm
        /// </search>
        public static List<Leader> GetLeaders(global::Revit.Elements.Element textNote)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.TextNote internalNote = (Autodesk.Revit.DB.TextNote)textNote.InternalElement;

            return internalNote.GetLeaders().ToList();
        }
    }
}

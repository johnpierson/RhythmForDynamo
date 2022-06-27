using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Rhythm.Utilities;

namespace Rhythm.Revit.Application
{

    /// <summary>
    /// Wrapper class for document nodes.
    /// </summary>
    public class Documents
    {
        private Documents()
        { }

        /// <summary>
        /// This node will copy the given elements from the given linked document into the active document.
        /// </summary>
        /// <param name="sourceDocument">The background opened document object, (preferably this is the title as obtained with Applications.OpenDocumentFile from Rhythm).</param>
        /// <param name="sourceInstance">The instance of the link to copy from.</param>
        /// <param name="elements">The elements to copy.</param>
        /// <returns name="newElements">The copied elements.</returns>
        /// <search>
        /// copy
        /// </search>
        [NodeCategory("Actions")]
        public static List<global::Revit.Elements.Element> CopyElementsFromLinkedDocument(object sourceDocument,
            global::Revit.Elements.Element sourceInstance, List<global::Revit.Elements.Element> elements)
        {
            Document sourceDbDoc = null;

            if (sourceDocument is global::Revit.Application.Document dynamoDoc)
            {
                sourceDbDoc = dynamoDoc.ToRevitType();
            }
            else
            {
                sourceDbDoc = sourceDocument as Document;
            }

            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //converts elements to ids in a collection
            ICollection<ElementId> idCollection = new List<ElementId>();

            foreach (global::Revit.Elements.Element element in elements)
            {
                idCollection.Add(element.InternalElement.Id);
            }
            Autodesk.Revit.DB.RevitLinkInstance internalInstance = (Autodesk.Revit.DB.RevitLinkInstance)sourceInstance.InternalElement;

            //creates copy paste options with type name handler
            CopyPasteOptions copyOpts = new CopyPasteOptions();
            copyOpts.SetDuplicateTypeNamesHandler(new HideAndAcceptDuplicateTypeNamesHandler());

            //creates blank transform.
            Transform transform = Transform.CreateTranslation(Vector.XAxis().ToRevitType());
            //commits the transaction
            TransactionManager.Instance.EnsureInTransaction(doc);
            ICollection<ElementId> newElementIds = ElementTransformUtils.CopyElements(sourceDbDoc, idCollection, doc, internalInstance.GetTransform(), copyOpts);
            TransactionManager.Instance.TransactionTaskDone();
            //create a new list for the new elements
            List<global::Revit.Elements.Element> newElements = new List<global::Revit.Elements.Element>();
            //gets the new elements
            foreach (ElementId id in newElementIds)
            {
                newElements.Add(doc.GetElement(id).ToDSType(true));
            }

            return newElements;
        }

        /// <summary>
        /// This node will copy the given elements from the given document into the active document.
        /// </summary>
        /// <param name="sourceDocument">The background opened document object, (preferably this is the title as obtained with Applications.OpenDocumentFile from Rhythm).</param>
        /// <param name="elements">The elements to copy.</param>
        /// <returns name="newElements">The copied elements.</returns>
        /// <search>
        /// copy
        /// </search>
        [NodeCategory("Actions")]
        public static List<global::Revit.Elements.Element> CopyElementsFromDocument(object sourceDocument, List<global::Revit.Elements.Element> elements)
        {
            Document sourceDbDoc = null;

            if (sourceDocument is global::Revit.Application.Document dynamoDoc)
            {
                sourceDbDoc = dynamoDoc.ToRevitType();
            }
            else
            {
                sourceDbDoc = sourceDocument as Document;
            }


            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //converts elements to ids in a collection
            ICollection<ElementId> idCollection = new List<ElementId>();
            foreach (global::Revit.Elements.Element element in elements)
            {
                idCollection.Add(element.InternalElement.Id);
            }
            //creates blank copy paste options
            CopyPasteOptions copyOpts = new CopyPasteOptions();
            copyOpts.SetDuplicateTypeNamesHandler(new HideAndAcceptDuplicateTypeNamesHandler());

            //creates blank transform.
            Transform transform = Transform.CreateTranslation(Vector.XAxis().ToRevitType());

            //commits the transaction
            TransactionManager.Instance.ForceCloseTransaction();

            ICollection<ElementId> newElementIds;

            using (Transaction copyTransaction = new Transaction(doc,"Copying Elements"))
            {
                copyTransaction.Start();

                //handle failures
                FailureHandlingOptions failureOptions = copyTransaction.GetFailureHandlingOptions();
                failureOptions.SetFailuresPreprocessor(new HidePasteDuplicateTypesPreprocessor());

                newElementIds = ElementTransformUtils.CopyElements(sourceDbDoc, idCollection, doc, transform, copyOpts);

                copyTransaction.Commit();
            }

            //create a new list for the new elements
            List<global::Revit.Elements.Element> newElements = new List<global::Revit.Elements.Element>();
            //gets the new elements
            foreach (ElementId id in newElementIds)
            {
                newElements.Add(doc.GetElement(id).ToDSType(true));
            }
            
            return newElements;
        }
        /// <summary>
        /// This node will set the starting view of the document, given the view element.
        /// </summary>
        /// <param name="sourceDocument">The background opened document object, (preferably this is the title as obtained with Applications.OpenDocumentFile from Rhythm).</param>
        /// <param name="view">The view to set.</param>
        /// <returns name="result">The result.</returns>
        /// <search>
        /// startingView
        /// </search>
        [NodeCategory("Actions")]
        public static string SetStartingView(object sourceDocument, global::Revit.Elements.Element view)
        {
            Document dbDoc = null;

            if (sourceDocument is global::Revit.Application.Document dynamoDoc)
            {
                dbDoc = dynamoDoc.ToRevitType();
            }
            else
            {
                dbDoc = sourceDocument as Document;
            }

            try
            {
                TransactionManager.Instance.EnsureInTransaction(dbDoc);
                StartingViewSettings.GetStartingViewSettings(dbDoc).ViewId = view.InternalElement.Id;
                TransactionManager.Instance.TransactionTaskDone();
                return "Success";
            }
            catch
            {
                return "Failure";
            }
        }
        /// <summary>
        /// This node will copy the given drafting views and their contents from the given document into the active document.
        /// </summary>
        /// <param name="sourceDocument">The background opened document object, (preferably this is the title as obtained with Applications.OpenDocumentFile from Rhythm).</param>
        /// <param name="draftingViews">The drafting views to copy.</param>
        /// <returns name="newDraftingViews">The copied drafting views with their elements.</returns>
        /// <search>
        /// copy
        /// </search>
        [NodeCategory("Actions")]
        public static List<global::Revit.Elements.Element> CopyDraftingViewsFromDocument(object sourceDocument, List<global::Revit.Elements.Element> draftingViews)
        {
            Document sourceDbDoc = null;

            if (sourceDocument is global::Revit.Application.Document dynamoDoc)
            {
                sourceDbDoc = dynamoDoc.ToRevitType();
            }
            else
            {
                sourceDbDoc = sourceDocument as Document;
            }

            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            
            //converts elements to ids in a collection
            ICollection<ElementId> idCollection = new List<ElementId>();

            foreach (global::Revit.Elements.Element view in draftingViews)
            {
                idCollection.Add(view.InternalElement.Id);
            }

            TransactionManager.Instance.ForceCloseTransaction();
            List<View> newDraftingViews =
                ViewUtils.DuplicateDraftingViews(sourceDbDoc, idCollection, doc);
            List<global::Revit.Elements.Element> vList = new List<global::Revit.Elements.Element>(newDraftingViews.Select(v => v.ToDSType(true)));

            return vList;
        }

        /// <summary>
        /// This node will save the Revit document to another path.
        /// </summary>
        /// <param name="document">A valid Revit Document.</param>
        /// <param name="filePath">The file path to save the document.</param>
        /// <param name="previewViewId">Optional - If you want to specify the preview view for the thumbnail.</param>
        /// <returns name="result">A string message whether the save as was successful or a failure.</returns>
        [NodeCategory("Action")]
        public static string SaveAs(object document, string filePath, int previewViewId = -1)
        {
            Document dbDoc = null;

            if (document is global::Revit.Application.Document dynamoDoc)
            {
                dbDoc = dynamoDoc.ToRevitType();
            }
            else
            {
                dbDoc = document as Document;
            }


            SaveAsOptions opts = new SaveAsOptions();

            if (previewViewId != -1)
            {
                try
                {
                    opts.PreviewViewId = new ElementId(previewViewId);
                }
                catch (Exception)
                {
                    //suppress
                }
            }

            try
            {
                opts.Compact = true;
                dbDoc.SaveAs(filePath, opts);
                return "Successful Save";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }

    #region EventHandlers
    /// <summary>
    /// A handler to accept duplicate types names created by the copy/paste operation.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    class HideAndAcceptDuplicateTypeNamesHandler : IDuplicateTypeNamesHandler
    {
        #region IDuplicateTypeNamesHandler Members

        /// <summary>
        /// Implementation of the IDuplicateTypeNameHandler
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public DuplicateTypeAction OnDuplicateTypeNamesFound(DuplicateTypeNamesHandlerArgs args)
        {
            // Always use duplicate destination types when asked
            return DuplicateTypeAction.UseDestinationTypes;
        }

        #endregion
    }
    /// <summary>
    /// A failure preprocessor to hide the warning about duplicate types being pasted.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    class HidePasteDuplicateTypesPreprocessor : IFailuresPreprocessor
    {
        #region IFailuresPreprocessor Members

        /// <summary>
        /// Implementation of the IFailuresPreprocessor.
        /// </summary>
        /// <param name="failuresAccessor"></param>
        /// <returns></returns>
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            foreach (FailureMessageAccessor failure in failuresAccessor.GetFailureMessages())
            {
                // Delete any "Can't paste duplicate types.  Only non duplicate types will be pasted." warnings
                if (failure.GetFailureDefinitionId() == BuiltInFailures.CopyPasteFailures.CannotCopyDuplicates)
                {
                    failuresAccessor.DeleteWarning(failure);
                }
            }

            // Handle any other errors interactively
            return FailureProcessingResult.Continue;
        }

        #endregion
    }
    #endregion
}

using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace Rhythm.Revit.Views
{
    /// <summary>
    /// Wrapper class for sections.
    /// </summary>
    public class ViewSection
    {
        private ViewSection()
        { }


        /// <summary>
        /// Creates a reference section.
        /// </summary>
        /// <param name="parentView"></param>
        /// <param name="viewToReference"></param>
        /// <param name="headPoint"></param>
        /// <param name="tailPoint"></param>
        public static void CreateReferenceSection(global::Revit.Elements.Element parentView,
            global::Revit.Elements.Element viewToReference, Point headPoint, Point tailPoint)
        {
            //the current document
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(doc);
            Autodesk.Revit.DB.ViewSection.CreateReferenceSection(doc,parentView.InternalElement.Id,viewToReference.InternalElement.Id,headPoint.ToRevitType(),tailPoint.ToRevitType());
            TransactionManager.Instance.TransactionTaskDone();
        }
    }
}


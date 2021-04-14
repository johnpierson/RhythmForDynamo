using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Revit.Elements;
using Revit.GeometryConversion;
using Curve = Autodesk.Revit.DB.Curve;
using SketchPlane = Autodesk.Revit.DB.SketchPlane;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for Parts.
    /// </summary>
    public class Parts
    {
        private Parts()
        { }
        /// <summary>
        /// This node will divide the given parts by reference planes.
        /// </summary>
        /// <param name="part">The part to divide.</param>
        /// <param name="referencePlane">The reference plane to use for the division.</param>
        /// <returns name="Parts">The created parts from the given element.</returns>
        /// <search>
        /// Parts.DivideParts, rhythm
        /// </search>
        [NodeCategory("Actions")]
        public static global::Revit.Elements.Element DivideParts(global::Revit.Elements.Element part, List<global::Revit.Elements.ReferencePlane> referencePlane)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.UI.UIDocument uidoc = new Autodesk.Revit.UI.UIDocument(doc);
            //create a list to hold the element ids and add them to it
            ICollection<Autodesk.Revit.DB.ElementId> partIds = new List<ElementId>();
            partIds.Add(part.InternalElement.Id);
            //create a list to hold the element ids and add them to it
            ICollection<Autodesk.Revit.DB.ElementId> intersectingElementIds = new List<ElementId>();

            foreach (var rPlane in referencePlane)
            {
             intersectingElementIds.Add(rPlane.InternalElement.Id);   
            }
            
            //curve list
            IList<Curve> curveList = new List<Curve>();         
            //start a transaction and divide the parts
            TransactionManager.Instance.EnsureInTransaction(doc);
            Autodesk.Revit.DB.SketchPlane sketchPlane = SketchPlane.Create(doc, Autodesk.DesignScript.Geometry.Plane.XY().ToPlane());
            PartUtils.DivideParts(doc, partIds, intersectingElementIds,curveList, sketchPlane.Id);
            TransactionManager.Instance.TransactionTaskDone();
            doc.ActiveView.PartsVisibility = PartsVisibility.ShowPartsOnly;
            //regeneration allows the parts to be selectable
            doc.Regenerate();

            return part;
        }

        /// <summary>
        /// Gets the collection of elements from which the parts were created.
        /// </summary>
        /// <param name="part">The part to get the element from.</param>
        /// <returns name="sourceElement">The created parts from the given element.</returns>
        /// <search>
        /// Parts.GetSourceElement, rhythm
        /// </search>
        [NodeCategory("Query")]
        public static global::Revit.Elements.Element GetSourceElement(global::Revit.Elements.Element part)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //converts the Revit family instance to internal.
            var internalPart = (Autodesk.Revit.DB.Part)part.InternalElement;
            global::Revit.Elements.Element sourceElement = null;
            sourceElement = doc.GetElement(internalPart.GetSourceElementIds().First().HostElementId).ToDSType(true);

            return sourceElement;
        }
    }
}

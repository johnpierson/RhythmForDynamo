using System.Collections.Generic;
using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Transactions;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for elevation markers.
    /// </summary>
    public class ElevationMarker
    {
        private ElevationMarker()
        {
        }

        /// <summary>
        /// This node will create an empty elevation marker at the given points. 
        /// </summary>
        /// <param name="location">The location of the view.</param>
        /// <param name="scaleFactor">The scale factor of the view in integer value. E.g. "96"</param>
        /// <param name="viewFamilyType">The view family type you wish to use.</param>
        /// <returns name="elevationMarker">The created elevation marker.</returns>
        /// <search>
        /// viewport, addview,rhythm
        /// </search>
        [NodeCategory("Create")]
        public static List<global::Revit.Elements.Element> CreateElevationMarker(List<Autodesk.DesignScript.Geometry.Point> location, int scaleFactor, global::Revit.Elements.Element viewFamilyType )
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //obtain the element id from the sheet
            ElementId viewFamilyTypeId = viewFamilyType.InternalElement.Id;
            List<global::Revit.Elements.Element> elevationMarker = new List<global::Revit.Elements.Element>();
            foreach (Autodesk.DesignScript.Geometry.Point loc in location)
            {
            TransactionManager.Instance.EnsureInTransaction(doc);
                elevationMarker.Add(Autodesk.Revit.DB.ElevationMarker.CreateElevationMarker(doc, viewFamilyTypeId, loc.ToXyz(), scaleFactor).ToDSType(false));
            TransactionManager.Instance.TransactionTaskDone();  
            }
            return elevationMarker;
        }

        /// <summary>
        /// This node will add elevations on each side of the marker chosen. Typically 0-3. 
        /// </summary>
        /// <param name="elevationMarker">The marker to host elevations on.</param>
        /// <param name="planView">Plan view to do this stuff to.</param>
        /// <param name="index">This is where the view appears.</param>
        /// <returns name="Result">The result</returns>
        /// <search>
        /// viewport, addview,rhythm
        /// </search>
        [NodeCategory("Create")]
        public static List<global::Revit.Elements.Element> CreateElevationByMarkerIndex(global::Revit.Elements.Element elevationMarker, global::Revit.Elements.Element planView, List<int> index)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //obtain the element id from the sheet
            ElementId planViewId = planView.InternalElement.Id;
            List<global::Revit.Elements.Element> result = new List<global::Revit.Elements.Element>();

            foreach (int i in index)
            {
                    Autodesk.Revit.DB.ElevationMarker internalMarker = (Autodesk.Revit.DB.ElevationMarker)elevationMarker.InternalElement;
                    TransactionManager.Instance.EnsureInTransaction(doc);
                    result.Add(internalMarker.CreateElevation(doc, planViewId, i).ToDSType(false));
                    TransactionManager.Instance.TransactionTaskDone();                    
            }
            return result;
        }
    }
}
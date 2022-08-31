using Autodesk.DesignScript.Runtime;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Transactions;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for curtain grids.
    /// </summary>
    public class CurtainGrid
    {
        private CurtainGrid()
        {
        }
        /// <summary>
        /// This node will retrieve the curtain grid and U/V Gridlines from the given wall
        /// </summary>
        /// <param name="curtainWall">The curtain wall to get data from.</param>
        /// <returns name="curtainGrid">The internal curtain grid.</returns>
        /// <returns name="uGrids">The grids in the U direction, (horizontal).</returns>
        /// <returns name="vGrids">The grids in the V direction, (vertical).</returns>
        /// <search>
        /// curtaingrid, rhythm
        /// </search>
        [MultiReturn(new[] { "curtainGrid" , "uGrids" , "vGrids" })]
        [NodeCategory("Create")]
        public static Dictionary<string, object> ByWallElement(global::Revit.Elements.Wall curtainWall)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.Wall internalWall = (Autodesk.Revit.DB.Wall)curtainWall.InternalElement;
            //obtains internal curtain grid
            Autodesk.Revit.DB.CurtainGrid internalCurtainGrid = internalWall.CurtainGrid;
            //gets U Grid Ids
            ICollection<Autodesk.Revit.DB.ElementId> uGridIds = internalCurtainGrid.GetUGridLineIds();
            //make new list for U grids
            List<global::Revit.Elements.Element> uGrids = new List<global::Revit.Elements.Element>(uGridIds.Select(id => doc.GetElement(id).ToDSType(true)).ToArray());
            //gets V Grid Ids
            ICollection<Autodesk.Revit.DB.ElementId> vGridIds = internalCurtainGrid.GetVGridLineIds();
            //make new list for V grids
            List<global::Revit.Elements.Element> vGrids = new List<global::Revit.Elements.Element>(vGridIds.Select(id => doc.GetElement(id).ToDSType(true)).ToArray());
            //returns the outputs
            var outInfo = new Dictionary<string, object>
                {
                    { "curtainGrid", internalCurtainGrid},
                    { "uGrids", uGrids},
                    { "vGrids", vGrids},
                };
            return outInfo;
        }
        /// <summary>
        /// This node will retrieve the curtain grid and U/V Gridlines from the given wall
        /// </summary>
        /// <param name="slopedGlazing">The sloped glazing to get data from.</param>
        /// <returns name="curtainGrid">The internal curtain grid.</returns>
        /// <returns name="uGrids">The grids in the U direction, (horizontal).</returns>
        /// <returns name="vGrids">The grids in the V direction, (vertical).</returns>
        /// <search>
        /// curtaingrid, rhythm
        /// </search>
        [MultiReturn(new[] { "curtainGrid", "uGrids", "vGrids" })]
        [NodeCategory("Create")]
        public static Dictionary<string, object> ByRoofElement(global::Revit.Elements.Roof slopedGlazing)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.FootPrintRoof internalRoof = (Autodesk.Revit.DB.FootPrintRoof)slopedGlazing.InternalElement;
            //obtains internal curtain grid
            Autodesk.Revit.DB.CurtainGrid internalCurtainGrid = null;
            foreach (var cg in internalRoof.CurtainGrids)
            {
                if (cg is Autodesk.Revit.DB.CurtainGrid)
                {
                    internalCurtainGrid = cg as Autodesk.Revit.DB.CurtainGrid;
                    break;
                }
            }

            if (internalCurtainGrid == null) return null;

            //gets U Grid Ids
            List<global::Revit.Elements.Element> uGrids = new List<Element>();
            if (internalCurtainGrid.NumULines > 0)
            {
                var uGridIds = internalCurtainGrid.GetUGridLineIds();
                uGrids = new List<global::Revit.Elements.Element>(uGridIds
                    .Select(id => doc.GetElement(id).ToDSType(true)).ToArray());
            }

            //gets V Grid Ids
            List<global::Revit.Elements.Element> vGrids = new List<Element>();
            if (internalCurtainGrid.NumVLines > 0)
            {
                var vGridIds = internalCurtainGrid.GetVGridLineIds();
                vGrids = new List<global::Revit.Elements.Element>(vGridIds
                    .Select(id => doc.GetElement(id).ToDSType(true)).ToArray());
            }

            //returns the outputs
            var outInfo = new Dictionary<string, object>
                {
                    { "curtainGrid", internalCurtainGrid},
                    { "uGrids", uGrids},
                    { "vGrids", vGrids},
                };
            return outInfo;
        }
        /// <summary>
        /// This node will retrieve the curtain grid per face from the curtain system.
        /// </summary>
        /// <param name="curtainSystem">The curtain system to get data from.</param>
        /// <returns name="curtainGrid">The internal curtain grid.</returns>
        /// <search>
        /// curtaingrid, rhythm
        /// </search>
        [NodeCategory("Create")]
        public static List<Autodesk.Revit.DB.CurtainGrid> ByCurtainSystem(global::Revit.Elements.CurtainSystem curtainSystem)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.CurtainSystem internalCurtainSystem = (Autodesk.Revit.DB.CurtainSystem)curtainSystem.InternalElement;
            //obtains internal curtain grid
            Autodesk.Revit.DB.CurtainGridSet internalCurtainGridSet = internalCurtainSystem.CurtainGrids;
            //the grid per face of system
            List<Autodesk.Revit.DB.CurtainGrid> internalCurtainGrid = new List<Autodesk.Revit.DB.CurtainGrid>();

            foreach (Autodesk.Revit.DB.CurtainGrid curtainGridSet in internalCurtainGridSet)
            {
                internalCurtainGrid.Add(curtainGridSet);
            }

            return internalCurtainGrid;
        }

        /// <summary>
        /// This node will retrieve the U gridlines from the curtain grid
        /// </summary>
        /// <param name="curtainGrid">The curtain grid to get data from.</param>
        /// <returns name="UGrids">The grids in the U direction, (horizontal).</returns>
        /// <search>
        /// curtaingrid, rhythm
        /// </search>
        [NodeCategory("Query")]
        public static List<global::Revit.Elements.Element> UGrids(Autodesk.Revit.DB.CurtainGrid curtainGrid)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //gets U Grid Ids
            ICollection<Autodesk.Revit.DB.ElementId> uGridIds = curtainGrid.GetUGridLineIds();
            //make new list for U grids
            List<global::Revit.Elements.Element> uGrids = new List<global::Revit.Elements.Element>(uGridIds.Select(id => doc.GetElement(id).ToDSType(true)).ToArray());           

            return uGrids;
        }
        /// <summary>
        /// This node will retrieve the V gridlines from the curtain grid
        /// </summary>
        /// <param name="curtainGrid">The curtain grid to get data from.</param>
        /// <returns name="VGrids">The grids in the V direction, (horizontal).</returns>
        /// <search>
        /// curtaingrid, rhythm
        /// </search>
        [NodeCategory("Query")]
        public static List<global::Revit.Elements.Element> VGrids(Autodesk.Revit.DB.CurtainGrid curtainGrid)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //gets U Grid Ids
            ICollection<Autodesk.Revit.DB.ElementId> vGridIds = curtainGrid.GetVGridLineIds();
            //make new list for U grids
            List<global::Revit.Elements.Element> vGrids = new List<global::Revit.Elements.Element>(vGridIds.Select(id => doc.GetElement(id).ToDSType(true)).ToArray());

            return vGrids;
        }

        /// <summary>
        /// This node will add a gridline at the specified place on the curtain wall grid.
        /// </summary>
        /// <param name="curtainGrid">The curtain grid to add a gridline to.</param>
        /// <param name="locationPoint">XYZ location to place grid</param>
        /// <param name="isUGridline">Is this gridline horizontal?</param>
        /// <returns name="curtainGridLine">The new gridline</returns>
        /// <search>
        /// curtainwall, rhythm
        /// </search>
        [NodeCategory("Actions")]
        public static List<global::Revit.Elements.Element> AddGridLineByPoint(Autodesk.Revit.DB.CurtainGrid curtainGrid, Autodesk.DesignScript.Geometry.Point locationPoint, bool isUGridline)
        {

            List<Autodesk.Revit.DB.XYZ> revitPoint = new List<Autodesk.Revit.DB.XYZ>();
            revitPoint.Add(locationPoint.ToRevitType());
            List<global::Revit.Elements.Element> newElements = new List<global::Revit.Elements.Element>();
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            TransactionManager.Instance.EnsureInTransaction(doc);
            foreach (var point in revitPoint)
            {
                newElements.Add(curtainGrid.AddGridLine(isUGridline, point, false).ToDSType(true));
            };
            TransactionManager.Instance.TransactionTaskDone();

            return newElements;
        }

        
    }
}

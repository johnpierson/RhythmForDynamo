using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using Revit.GeometryConversion;
using Surface = Autodesk.DesignScript.Geometry.Surface;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for host objects.
    /// </summary>
    public class HostObject
    {
        private HostObject()
        {
        }
        /// <summary>
        /// This node will return the exterior face or faces for the input host object. This particular method works for walls.
        /// </summary>
        /// <param name="hostObject">The host object to retrieve exterior faces for.</param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static IEnumerable<List<Surface>> ExteriorSurface(global::Revit.Elements.Element hostObject)
        {
            Autodesk.Revit.DB.HostObject internalHost = hostObject.InternalElement as Autodesk.Revit.DB.HostObject;

            IList<Reference> sideRefs = HostObjectUtils.GetSideFaces(internalHost, ShellLayerType.Exterior);

            List<Autodesk.Revit.DB.Face> exteriorGeometryObjects = new List<Autodesk.Revit.DB.Face>(sideRefs.Select(r => internalHost.GetGeometryObjectFromReference(r)).Cast<Autodesk.Revit.DB.Face>());

            return exteriorGeometryObjects.Select(g => g.ToProtoType(true).ToList());

        }
        /// <summary>
        /// This node will return the interior face or faces for the input host object. This particular method works for walls.
        /// </summary>
        /// <param name="hostObject">The host object to retrieve interior faces for.</param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static IEnumerable<List<Surface>> InteriorSurface(global::Revit.Elements.Element hostObject)
        {
            Autodesk.Revit.DB.HostObject internalHost = hostObject.InternalElement as Autodesk.Revit.DB.HostObject;

            IList<Reference> sideRefs = HostObjectUtils.GetSideFaces(internalHost, ShellLayerType.Interior);

            List<Autodesk.Revit.DB.Face> exteriorGeometryObjects = new List<Autodesk.Revit.DB.Face>(sideRefs.Select(r => internalHost.GetGeometryObjectFromReference(r)).Cast<Autodesk.Revit.DB.Face>());

            return exteriorGeometryObjects.Select(g => g.ToProtoType(true).ToList());

        }
        /// <summary>
        /// This node will return the bottom face or faces for the input host object. This particular method works for ceilings, roofs, or floors.
        /// </summary>
        /// <param name="hostObject">The host object to retrieve top faces for.</param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static IEnumerable<List<Surface>> TopSurface(global::Revit.Elements.Element hostObject)
        {
            Autodesk.Revit.DB.HostObject internalHost = hostObject.InternalElement as Autodesk.Revit.DB.HostObject;

            IList<Reference> sideRefs = HostObjectUtils.GetTopFaces(internalHost);

            List<Autodesk.Revit.DB.Face> exteriorGeometryObjects = new List<Autodesk.Revit.DB.Face>(sideRefs.Select(r => internalHost.GetGeometryObjectFromReference(r)).Cast<Autodesk.Revit.DB.Face>());

            return exteriorGeometryObjects.Select(g => g.ToProtoType(true).ToList());

        }
        /// <summary>
        /// This node will return the bottom face or faces for the input host object. This particular method works for ceilings, roofs, or floors.
        /// </summary>
        /// <param name="hostObject">The host object to retrieve bottom faces for.</param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static IEnumerable<List<Surface>> BottomSurface(global::Revit.Elements.Element hostObject)
        {
            Autodesk.Revit.DB.HostObject internalHost = hostObject.InternalElement as Autodesk.Revit.DB.HostObject;

            IList<Reference> sideRefs = HostObjectUtils.GetBottomFaces(internalHost);

            List<Autodesk.Revit.DB.Face> exteriorGeometryObjects = new List<Autodesk.Revit.DB.Face>(sideRefs.Select(r => internalHost.GetGeometryObjectFromReference(r)).Cast<Autodesk.Revit.DB.Face>());

            return exteriorGeometryObjects.Select(g => g.ToProtoType(true).ToList());

        }
    }
}

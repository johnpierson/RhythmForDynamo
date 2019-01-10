using System.Collections.Generic;
using System.Linq;
using Revit.Elements;
using RevitServices.Persistence;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for beam systems.
    /// </summary>
    public class BeamSystem
    {
        private BeamSystem()
        { }
        /// <summary>
        /// Obtains the individual beams within a beam system.
        /// </summary>
        /// <param name="BeamSystem">The beam system to get information from.</param>
        /// <returns name="members">The individual members.</returns>
        /// <search>
        /// structural, beamsystem
        /// </search>
        public static List<global::Revit.Elements.Element> Members(global::Revit.Elements.Element BeamSystem)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //convert input beam system to internal version
            Autodesk.Revit.DB.BeamSystem beamSystemInternal = (Autodesk.Revit.DB.BeamSystem) BeamSystem.InternalElement;
            //get beam ids from system
            var beamIdCollection = beamSystemInternal.GetBeamIds();
            //create list for the elements and add the beams to that list.
            List<global::Revit.Elements.Element> members = new List<global::Revit.Elements.Element>(beamIdCollection.Select(id => doc.GetElement(id).ToDSType(true)).ToArray());
            
            return members;
        }
        /// <summary>
        /// Drops the beam system.
        /// </summary>
        /// <param name="beamSystem">The beam system to drop.</param>
        /// <returns name="members">The individual members.</returns>
        /// <search>
        /// structural, beamsystem
        /// </search>
        public static List<global::Revit.Elements.Element> DropBeamSystem(global::Revit.Elements.Element beamSystem)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //convert input beam system to internal version
            Autodesk.Revit.DB.BeamSystem beamSystemInternal = (Autodesk.Revit.DB.BeamSystem)beamSystem.InternalElement;
            //get beam ids from system
            var beamIdCollection = beamSystemInternal.GetBeamIds();
            //create list for the elements and add the beams to that list.
            List<global::Revit.Elements.Element> members = new List<global::Revit.Elements.Element>(beamIdCollection.Select(id => doc.GetElement(id).ToDSType(true)).ToArray());
            //drop da beam system
            Autodesk.Revit.DB.BeamSystem.DropBeamSystem(beamSystemInternal);

            return members;
        }

    }
}

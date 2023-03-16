using RhythmRevit_Compatibility;

namespace RhythmRevit_22_23
{
    public class Viewport : IViewport
    {
        public int[] CompatibleVersions => new[]
        {
            2022,
            2023
        };

        public void SetViewTitleLineLength(object viewport, double length)
        {
            Autodesk.Revit.DB.Viewport internalViewport = viewport as Autodesk.Revit.DB.Viewport;

            internalViewport.LabelLineLength = length;
        }

        public void SetViewTitleLocation(object viewport, object location)
        {
            Autodesk.Revit.DB.Viewport internalViewport = viewport as Autodesk.Revit.DB.Viewport;
            Autodesk.Revit.DB.XYZ internalPoint = location as Autodesk.Revit.DB.XYZ;

            internalViewport.LabelOffset = internalPoint;
        }

        public double GetViewTitleLineLength(object viewport)
        {
            Autodesk.Revit.DB.Viewport internalViewport = viewport as Autodesk.Revit.DB.Viewport;

            return internalViewport.LabelLineLength;
        }

        public object GetViewTitleLocation(object viewport)
        {
            Autodesk.Revit.DB.Viewport internalViewport = viewport as Autodesk.Revit.DB.Viewport;

            return internalViewport.LabelOffset;
        }
    }
}

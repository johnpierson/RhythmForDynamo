using Revit.Elements;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for area tags
    /// </summary>
    public class AreaTag
    {
        private AreaTag()
        {
        }

        /// <summary>
        /// Retrieves the area that is tagged by the given area tag.
        /// </summary>
        /// <param name="areaTag"></param>
        /// <returns></returns>
        public static global::Revit.Elements.Element TaggedArea(global::Revit.Elements.Element areaTag)
        {
            Autodesk.Revit.DB.AreaTag areaTagInternal = areaTag.InternalElement as Autodesk.Revit.DB.AreaTag;
            return areaTagInternal.Area.ToDSType(true);
        }
    }
}

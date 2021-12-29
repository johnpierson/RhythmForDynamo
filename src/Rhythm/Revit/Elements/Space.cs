using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Revit.Elements;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for spaces
    /// </summary>
    public class Spaces
    {
        private Spaces()
        {
        }
        /// <summary>
        /// Return space tags for given space in given view
        /// </summary>
        /// <param name="space">The space to check.</param>
        /// <param name="view">The view to check in.</param>
        /// <returns name="spaceTags">We return a list here because a space can have more than one tag. Whether or not it should, is a different conversation.</returns>
        public static List<global::Revit.Elements.Element> SpaceTagsInView(global::Revit.Elements.Space space,
            global::Revit.Elements.Views.FloorPlanView view)
        {
            var internalSpace = space.InternalElement as Autodesk.Revit.DB.Mechanical.Space;
            var doc = internalSpace.Document;

            ElementCategoryFilter spaceTagFilter = new ElementCategoryFilter(BuiltInCategory.OST_MEPSpaceTags);
            var spaceTags = internalSpace.GetDependentElements(spaceTagFilter).Select(id => doc.GetElement(id)).ToList();

            return spaceTags.Cast<Autodesk.Revit.DB.Mechanical.SpaceTag>()
                .Where(st => st.View.Id.IntegerValue.Equals(view.Id)).Select(r => r.ToDSType(true)).ToList();
        }

    }
}

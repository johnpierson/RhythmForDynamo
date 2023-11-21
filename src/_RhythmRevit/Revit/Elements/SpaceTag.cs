using System;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Transactions;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for space tags
    /// </summary>
    public  class SpaceTag
    {
        private SpaceTag(){}
        /// <summary>
        /// Place or update an existing space tag.
        /// </summary>
        /// <param name="view">Tags are view specific. This is the specific view to use.</param>
        /// <param name="space">The space to be tagged.</param>
        /// <param name="location">The location to place the tag. If null, this will place on space origin.</param>
        /// <param name="tagType">The tag type to use. If null, the default one is used</param>
        /// <param name="tryUpdateExisting">Toggle to true to try and update existing space tags in the view.</param>
        /// <returns name="spaceTag">The new or updated space tag.</returns>
        [NodeCategory("Actions")]
        public static global::Revit.Elements.Element PlaceOrUpdate(global::Revit.Elements.Views.FloorPlanView view, global::Revit.Elements.Element space, [DefaultArgument("Rhythm.Utilities.MiscUtils.GetNull()")] Point location, [DefaultArgument("Rhythm.Utilities.MiscUtils.GetNull()")] global::Revit.Elements.FamilyType tagType, bool tryUpdateExisting = false)
        {
            var internalSpace = space.InternalElement as Autodesk.Revit.DB.Mechanical.Space;

            if (internalSpace.Area <= 0) return null;

            var doc = internalSpace.Document;

            var lp = internalSpace.Location as LocationPoint;

            Point locationPoint = location ?? lp.Point.ToPoint();

            Autodesk.Revit.DB.Mechanical.SpaceTag spaceTag;

            if (tryUpdateExisting && Spaces.SpaceTagsInView(space, view).Any())
            {
                var firstTag =
                    Spaces.SpaceTagsInView(space, view).First();

                //set the location if a tag is found and that option is selected
                TransactionManager.Instance.EnsureInTransaction(doc);
                firstTag.SetLocation(locationPoint);
                TransactionManager.Instance.TransactionTaskDone();
                spaceTag = firstTag.InternalElement as Autodesk.Revit.DB.Mechanical.SpaceTag;
            }
            else
            {
                var revitPoint = locationPoint.ToRevitType(true);
                Autodesk.Revit.DB.UV uv = new Autodesk.Revit.DB.UV(revitPoint.X, revitPoint.Y);

                //create a new tag if none are found
                TransactionManager.Instance.EnsureInTransaction(doc);
                spaceTag = doc.Create.NewSpaceTag(internalSpace, uv, (View)view.InternalElement);

                spaceTag.ToDSType(false).SetLocation(locationPoint);
                TransactionManager.Instance.TransactionTaskDone();
            }

            //if the given tag type is valid, set the tag to that type
            if (tagType == null) return spaceTag.ToDSType(false);

#if R20 || R21 || R22 || R23
            ElementId elementId = new ElementId(-2000485);
#endif

#if R24_OR_GREATER
            ElementId elementId = new ElementId(Convert.ToInt64(-2000485));
#endif

            if (tagType.InternalElement.Category.Id != elementId) return spaceTag.ToDSType(false);

            var spaceTagType = doc.GetElement(tagType.InternalElement.Id) as SpaceTagType;
            TransactionManager.Instance.EnsureInTransaction(doc);
            spaceTag.ChangeTypeId(spaceTagType.Id);
            TransactionManager.Instance.TransactionTaskDone();

            return spaceTag.ToDSType(false);
        }
    }
}

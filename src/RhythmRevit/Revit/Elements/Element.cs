using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Rhythm.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Line = Autodesk.DesignScript.Geometry.Line;
using Point = Autodesk.DesignScript.Geometry.Point;
using Solid = Autodesk.Revit.DB.Solid;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for Element.
    /// </summary>
    public class Elements
    {
        private Elements()
        {
        }

        /// <summary>
        /// Get Null
        /// </summary>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static object GetNull()
        {
            return null;
        }

        /// <summary>
        /// This node will convert the given elements to parts.
        /// </summary>
        /// <param name="element">The element to convert to parts.</param>
        /// <returns name="Parts">The created parts from the given element.</returns>
        /// <search>
        /// Element.CreateParts
        /// </search>
        [NodeCategory("Actions")]
        public static List<global::Revit.Elements.Element> CreateParts(global::Revit.Elements.Element element)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //create a list to hold the element ids and add them to it
            ICollection<Autodesk.Revit.DB.ElementId> elementIds = new List<ElementId>() { element.InternalElement.Id };
            //start a transaction and create the parts
            TransactionManager.Instance.EnsureInTransaction(doc);
            PartUtils.CreateParts(doc, elementIds);
            TransactionManager.Instance.TransactionTaskDone();
            doc.ActiveView.PartsVisibility = PartsVisibility.ShowPartsOnly;
            //regeneration allows the parts to be selectable
            doc.Regenerate();
            //collect the parts that have been created
            ICollection<ElementId> partIds = PartUtils.GetAssociatedParts(doc, element.InternalElement.Id, false, false);
            List<global::Revit.Elements.Element> partList = new List<global::Revit.Elements.Element>();
            //collect the newly created parts
            foreach (Autodesk.Revit.DB.ElementId id in partIds)
            {
                partList.Add(doc.GetElement(id).ToDSType(false));
            }

            return partList;
        }

        /// <summary>
        /// This node will get the parameter as instance or type.
        /// </summary>
        /// <param name="element">The element to get parameter from.</param>
        /// <param name="parameterName">The parameter name.</param>
        /// <returns name="value">The parameter value.</returns>
        /// <search>
        ///  TypeOrInstance
        /// </search>
        [NodeCategory("Actions")]
        public static object GetParameterValueByNameTypeOrInstance(global::Revit.Elements.Element element, string parameterName)
        {
            //create a list to hold the element ids and add them to it
            Autodesk.Revit.DB.Element internalElement = element.InternalElement;
            Autodesk.Revit.DB.Document doc = internalElement.Document;
            //declare variable to assign
            //looks up the parameter to see if it exists
            var result = internalElement.LookupParameter(parameterName);
            //if parameter exists as instance obtain it, otherwise try for type
            var paramValue = result != null ? element.GetParameterValueByName(parameterName) : doc.GetElement(internalElement.GetTypeId()).ToDSType(false).GetParameterValueByName(parameterName);

            return paramValue;
        }

        /// <summary>
        /// Set one of the element's parameters. Instance if it is instance or type if type.
        /// </summary>
        /// <param name="element">The element to set parameter to.</param>
        /// <param name="parameterName">The parameter name.</param>
        /// <param name="value">The value to set.</param>
        /// <returns name="element">The element.</returns>
        /// <search>
        /// TypeOrInstance
        /// </search>
        [NodeCategory("Actions")]
        public static global::Revit.Elements.Element SetParameterByNameTypeOrInstance(global::Revit.Elements.Element element,
            string parameterName, object value)
        {
            //create a list to hold the element ids and add them to it
            Autodesk.Revit.DB.Element internalElement = element.InternalElement;
            Autodesk.Revit.DB.Document doc = internalElement.Document;

            //looks up the parameter to see if it exists
            var result = internalElement.LookupParameter(parameterName);
            //if parameter exists as instance obtain it, otherwise try for type
            if (result != null)
            {
                element.SetParameterByName(parameterName, value);
            }
            else
            {
                doc.GetElement(internalElement.GetTypeId())
                    .ToDSType(false)
                    .SetParameterByName(parameterName, value);
            }

            return element;
        }

        /// <summary>
        /// This will take a given element and category and grab the intersecting elements of that category.
        /// </summary>
        /// <param name="element">The element to run intersections against.</param>
        /// <param name="category">The category to check.</param>
        /// <returns name="elements">The intersecting elements.</returns>
        /// <search>
        ///  IntersectingElements
        /// </search>
        [IsObsolete("Node removed, use the OOTB node, Element.GetIntersectingElementsOfCategory")]
        [NodeCategory("Query")]
        public static List<global::Revit.Elements.Element> GetIntersectingElementsOfCategory(global::Revit.Elements.Element element,
            global::Revit.Elements.Category category)
        {
            //the current document
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //get built in category from user viewable category
            BuiltInCategory myCatEnum = (BuiltInCategory)Enum.Parse(typeof(BuiltInCategory), category.Id.ToString());
            //the intersection filter
            ElementIntersectsElementFilter filter = new ElementIntersectsElementFilter(element.InternalElement);
            //build a collector
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            //collect the elements that fall in that category
            IList<Autodesk.Revit.DB.Element> intersectingElementsInternaList =
                collector.OfCategory(myCatEnum).WhereElementIsNotElementType().WherePasses(filter).ToElements();
            //build a new list for the intersecting elements to be added to
            List<global::Revit.Elements.Element> intersectingElements = new List<global::Revit.Elements.Element>();
            //add each user recognizable element to the list
            foreach (Autodesk.Revit.DB.Element internalElement in intersectingElementsInternaList)
            {
                intersectingElements.Add(internalElement.ToDSType(false));
            }

            return intersectingElements;
        }

        /// <summary>
        /// This node will get a parameter value by search string, regardless of case of the search string. Also accounts for misspellings.
        /// Note: If the parameter name appears multiple times it will retrieve the first one that it finds.
        /// </summary>
        /// <param name="element">The element to get parameter from.</param>
        /// <param name="parameterName">The parameter name.</param>
        /// <returns name="value">The parameter value.</returns>
        /// <search>
        /// CaseInsensitive
        /// </search>
        [NodeCategory("Actions")]
        public static object GetParameterValueByNameCaSeiNSeNSiTiVe(global::Revit.Elements.Element element,
            string parameterName)
        {
            //create a list to hold the element ids and add them to it
            Autodesk.Revit.DB.Element internalElement = element.InternalElement;

            global::Revit.Elements.Parameter[] elementParams = element.Parameters;

            string paramToSet;
            //score of each match list
            List<int> values = new List<int>();
            //score the match in the parameter list
            foreach (var param in elementParams)
            {
                values.Add(StringComparisonUtilities.Compute(parameterName, param.Name));
            }
            //get the closest matching parameter name
            int minIndex = values.IndexOf(values.Min());
            paramToSet = elementParams[minIndex].Name;
            //lookup and get the parameter value
            var result = internalElement.LookupParameter(paramToSet);
            var parameterValue = global::Revit.Elements.InternalUtilities.ElementUtils.GetParameterValue(result);

            return parameterValue;
        }

        /// <summary>
        /// This node will set a parameter value by search string, regardless of case of the search string. Also accounts for misspellings.
        /// Note: If the parameter name appears multiple times it will retrieve the first one that it finds.
        /// </summary>
        /// <param name="element">The element to get parameter from.</param>
        /// <param name="parameterName">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <returns name="element">The element that had parameters set.</returns>
        /// <search>
        ///  CaseInsensitive
        /// </search>
        [NodeCategory("Actions")]
        public static global::Revit.Elements.Element SetParameterValueByNameCaSeiNSeNSiTiVe(global::Revit.Elements.Element element,
            string parameterName, object value)
        {
            //check Rhythm version
            //Rhythm.Utilities.VersionChecker();
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //create a list to hold the element ids and add them to it
            global::Revit.Elements.Parameter[] elementParams = element.Parameters;

            string paramToSet;
            //score of each match list
            List<int> values = new List<int>();
            //score the match in the parameter list
            foreach (var param in elementParams)
            {
                values.Add(StringComparisonUtilities.Compute(parameterName, param.Name));
            }
            //get the closest matching parameter name
            int minIndex = values.IndexOf(values.Min());
            paramToSet = elementParams[minIndex].Name;

            //Convert to a usable value
            var dynval = value as dynamic;
            TransactionManager.Instance.EnsureInTransaction(doc);
            //set that thing!
            element.SetParameterByName(paramToSet, dynval);
            TransactionManager.Instance.TransactionTaskDone();

            return element;
        }

        /// <summary>
        /// This will take a given element and category and grab the intersecting elements of that category.
        /// </summary>
        /// <param name="element">The element to run intersections against.</param>
        /// <param name="category">The category to check.</param>
        /// <param name="buffer">Optional scale buffer in X, Y, Z directions. Defaults to 0.</param>
        /// <returns name="elements">The intersecting elements.</returns>
        /// <search>
        ///  IntersectingElements
        /// </search>
        [NodeCategory("Query")]
        public static List<global::Revit.Elements.Element> IntersectingElementsOfCategoryBuffered(
            global::Revit.Elements.Element element,
            global::Revit.Elements.Category category,
            [DefaultArgument("Rhythm.Utilities.MiscUtils.GetNull()")] Vector buffer)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            BuiltInCategory myCatEnum = (BuiltInCategory)Enum.Parse(typeof(BuiltInCategory), category.Id.ToString());

            var internalElement = element.InternalElement;
            List<Solid> solids = GetSolidsFromElement(internalElement);
            if (solids.Count == 0)
            {
                return new List<global::Revit.Elements.Element>();
            }

            Vector bufferVector = buffer ?? Vector.ByCoordinates(0, 0, 0);
            List<ElementFilter> solidFilters = new List<ElementFilter>();
            foreach (Solid solid in solids)
            {
                Solid scaledSolid = ApplyScaleBuffer(solid, bufferVector);
                solidFilters.Add(new ElementIntersectsSolidFilter(scaledSolid));
            }

            ElementFilter filter = solidFilters.Count == 1 ? solidFilters[0] : new LogicalOrFilter(solidFilters);
            IList<Autodesk.Revit.DB.Element> intersectingElementsInternaList = new FilteredElementCollector(doc)
                .OfCategory(myCatEnum)
                .WhereElementIsNotElementType()
                .WherePasses(filter)
                .ToElements();

            List<global::Revit.Elements.Element> intersectingElements = new List<global::Revit.Elements.Element>();
            foreach (Autodesk.Revit.DB.Element internalIntersectingElement in intersectingElementsInternaList)
            {
                if (!internalIntersectingElement.Id.Equals(internalElement.Id))
                {
                    intersectingElements.Add(internalIntersectingElement.ToDSType(false));
                }
            }

            return intersectingElements;
        }

        /// <summary>
        /// This will take a given element and category and grab the intersecting elements of that category.
        /// </summary>
        /// <param name="element">The element to run intersections against.</param>
        /// <param name="sourceInstance">Use this input if the elements are from a link! If they are not, leave it blank.</param>
        /// <param name="category">The category to check.</param>
        /// <returns name="elements">The intersecting elements.</returns>
        /// <search>
        ///  IntersectingElements
        /// </search>
        [NodeCategory("Query")]
        public static List<global::Revit.Elements.Element> GetIntersectingElementsOfCategoryLinkOption(global::Revit.Elements.Element element,
            global::Revit.Elements.Category category, [DefaultArgument("Rhythm.Utilities.MiscUtils.GetNull()")] global::Revit.Elements.Element sourceInstance)
        {
            //the current document
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //get built in category from user viewable category
            BuiltInCategory myCatEnum = (BuiltInCategory)Enum.Parse(typeof(BuiltInCategory), category.Id.ToString());
            //build a new list for the intersecting elements to be added to
            List<global::Revit.Elements.Element> intersectingElements = new List<global::Revit.Elements.Element>();
            if (sourceInstance != null)
            {
                Autodesk.Revit.DB.RevitLinkInstance internalInstance =
      (Autodesk.Revit.DB.RevitLinkInstance)sourceInstance.InternalElement;
                //get the element's geometry.
                GeometryElement geomElement = element.InternalElement.get_Geometry(new Options());
                //transform the solid to where the eff the link is.
                GeometryElement transformedElement = geomElement.GetTransformed(internalInstance.GetTransform());
                //make a solid filter.
                Solid solid = null;
                foreach (GeometryObject geomObj in transformedElement)
                {
                    solid = geomObj as Solid;
                    if (solid != null) break;
                }
                //the intersection filter
                ElementIntersectsSolidFilter filter = new ElementIntersectsSolidFilter(solid);
                //build a collector
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                //collect the elements that fall in that category
                IList<Autodesk.Revit.DB.Element> intersectingElementsInternaList =
                    collector.OfCategory(myCatEnum).WhereElementIsNotElementType().WherePasses(filter).ToElements();

                //add each user recognizable element to the list
                foreach (Autodesk.Revit.DB.Element internalElement in intersectingElementsInternaList)
                {
                    intersectingElements.Add(internalElement.ToDSType(false));
                }
            }
            else
            {
                //the intersection filter
                ElementIntersectsElementFilter filter = new ElementIntersectsElementFilter(element.InternalElement);
                //build a collector
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                //collect the elements that fall in that category
                IList<Autodesk.Revit.DB.Element> intersectingElementsInternaList =
                    collector.OfCategory(myCatEnum).WhereElementIsNotElementType().WherePasses(filter).ToElements();

                //add each user recognizable element to the list
                foreach (Autodesk.Revit.DB.Element internalElement in intersectingElementsInternaList)
                {
                    intersectingElements.Add(internalElement.ToDSType(false));
                }
            }

            return intersectingElements;
        }

        /// <summary>
        /// This node will report whether or not the given element is hidden in given views.
        /// </summary>
        /// <param name="element">The element to check.</param>
        /// <param name="view">The views to check in.</param>
        /// <returns name="bool">Is it hidden?</returns>
        /// <search>
        ///  IsVisible
        /// </search>
        [NodeCategory("Query")]
        public static List<bool> IsHiddenInView(List<global::Revit.Elements.Element> element,
            global::Revit.Elements.Views.View view)
        {
            List<bool> boolList = new List<bool>();

            foreach (global::Revit.Elements.Element e in element)
            {
                Autodesk.Revit.DB.Element internalElement = (Autodesk.Revit.DB.Element)e.InternalElement;
                Autodesk.Revit.DB.View internalView = (Autodesk.Revit.DB.View)view.InternalElement;

                boolList.Add(internalElement.IsHidden(internalView));
            }

            return boolList;
        }

        /// <summary>
        /// This node will report what elements depend on the input element. Useful for determining safe deletion.(Available Revit 2018.1+).
        /// </summary>
        /// <param name="element">The element to check.</param>
        /// <returns name="dependentElements">The dependent elements.</returns>
        /// <search>
        ///  DependentElements
        /// </search>
        [NodeCategory("Query")]
        public static List<global::Revit.Elements.Element> DependentElements(global::Revit.Elements.Element element)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //the element filter, this just filters out type elements.
            Autodesk.Revit.DB.ElementFilter elemFilter = new ElementIsElementTypeFilter(true);
            //the dependent element ids
            IList<ElementId> elemIds = element.InternalElement.GetDependentElements(elemFilter);
            //get the elements
            List<global::Revit.Elements.Element> elems = new List<global::Revit.Elements.Element>(elemIds.Select(e => doc.GetElement(e).ToDSType(false)));

            return elems;
        }

        /// <summary>
        /// This node will report what elements depend on the input element. Useful for determining safe deletion.(Available Revit 2018.1+).
        /// </summary>
        /// <param name="element">The element to check.</param>
        /// <param name="category">The category to use to filter the elements.</param>
        /// <returns name="dependentElements">The dependent elements.</returns>
        /// <search>
        ///  DependentElements
        /// </search>
        [NodeCategory("Query")]
        public static List<global::Revit.Elements.Element> DependentElementsOfCategory(global::Revit.Elements.Element element, global::Revit.Elements.Category category)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //obtain the builtin category from the id
            BuiltInCategory myCatEnum = (BuiltInCategory)Enum.Parse(typeof(BuiltInCategory), category.Id.ToString());
            //the filter to use
            Autodesk.Revit.DB.ElementFilter elemFilter = new ElementCategoryFilter(myCatEnum);
            //dependent element ids
            IList<ElementId> elemIds = element.InternalElement.GetDependentElements(elemFilter);
            //get the elements
            List<global::Revit.Elements.Element> elems = new List<global::Revit.Elements.Element>(elemIds.Select(e => doc.GetElement(e).ToDSType(false)));

            return elems;
        }

        /// <summary>
        /// This node will report what elements are joined to the input element.
        /// </summary>
        /// <param name="element">The element to check.</param>
        /// <returns name="joinedElements">The joined elements.</returns>
        /// <search>
        ///  Element.GetJoinedElements,JoinedElements
        /// </search>
        [IsObsolete("Node removed, use the OOTB node, Element.GetJoinedElements")]
        [NodeCategory("Query")]
        public static List<global::Revit.Elements.Element> JoinedElements(global::Revit.Elements.Element element)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //the element filter, this just filters out type elements.
            Autodesk.Revit.DB.ElementFilter elemFilter = new ElementIsElementTypeFilter(true);
            //the joined element ids
            ICollection<ElementId> elemIds = JoinGeometryUtils.GetJoinedElements(doc, element.InternalElement);
            //get the elements
            List<global::Revit.Elements.Element> elems = new List<global::Revit.Elements.Element>(elemIds.Select(e => doc.GetElement(e).ToDSType(false)));

            return elems;
        }

        /// <summary>
        /// This node will change the pinned status of an element.
        /// </summary>
        /// <param name="element">The element to change the status of.</param>
        /// <param name="status">The pinned status. True = pinned, False = not pinned.</param>
        /// <returns name="element">The elements.</returns>
        /// <search>
        ///  Element.SetPinnedStatus
        /// </search>
        /// works with background docs
        [IsObsolete("Node removed, use the OOTB node, Element.SetPinnedStatus")]
        [NodeCategory("Actions")]
        public static global::Revit.Elements.Element SetPinnedStatus(global::Revit.Elements.Element element, bool status)
        {
            Autodesk.Revit.DB.Document doc = element.InternalElement.Document;
            TransactionManager.Instance.EnsureInTransaction(doc);
            element.InternalElement.Pinned = status;
            TransactionManager.Instance.TransactionTaskDone();

            return element;
        }

        /// <summary>
        /// *BETA* - This node will retrieve the closest area that an element resides in.
        /// This uses bounding boxes which encompass the whole geometry, so we take the closest one.
        /// This means that there is potential that we grab the wrong one..
        /// </summary>
        /// <param name="element">The element to find the closest area location for.</param>
        /// <returns name="area">The closest area.</returns>
        /// <search>
        /// Element.AreaLocation
        /// </search>
        [NodeCategory("Actions")]
        public static global::Revit.Elements.Element AreaLocation(global::Revit.Elements.Element element)
        {
            //the current document
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //the internal element
            Autodesk.Revit.DB.Element internalElement = element.InternalElement;
            //list to append area locations to
            List<global::Revit.Elements.Element> areaLocations = new List<global::Revit.Elements.Element>();
            //ptlocation to append to
            Autodesk.DesignScript.Geometry.Point ptLocation = null;
            //figure out the location of the given element
            if (internalElement.Location is LocationPoint pt)
            {
                ptLocation = pt.Point.ToPoint(true);
            }
            else if (internalElement.Location is Autodesk.Revit.DB.LocationCurve)
            {
                Autodesk.Revit.DB.LocationCurve curve = internalElement.Location as Autodesk.Revit.DB.LocationCurve;
                ptLocation = curve.Curve.Evaluate(0.5, true).ToPoint(true);
            }
            else
            {
                throw new Exception(@"This element's location is not supported for this operation. ¯\_(ツ)_/¯ ");
            }

            var allViews = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                .OfClass(typeof(Autodesk.Revit.DB.View))
                .Cast<Autodesk.Revit.DB.View>()
                .Where(x => x.ViewType == ViewType.AreaPlan && !x.IsTemplate)
                .ToList();

            //collect the areas to do some cool stuff
            FilteredElementCollector areaColl = new FilteredElementCollector(doc);
            IList<Autodesk.Revit.DB.Element> areas = areaColl.OfCategory(BuiltInCategory.OST_Areas).ToElements();

            foreach (Autodesk.Revit.DB.Element area in areas)
            {

                View view = allViews.First(v => v.GenLevel.Id.Equals(area.LevelId));
                BoundingBox bBox = area.get_BoundingBox(view).ToProtoType();
                if (bBox.Contains(ptLocation) && area.LevelId.Equals(internalElement.LevelId))
                {
                    areaLocations.Add(area.ToDSType(false));
                }

            }

            global::Revit.Elements.Element elem = areaLocations.First(a => a.GetLocation().DistanceTo(ptLocation) == areaLocations.Min(e => e.GetLocation().DistanceTo(ptLocation)));

            return elem;
        }

        private static List<Solid> GetSolidsFromElement(Autodesk.Revit.DB.Element element)
        {
            GeometryElement geometryElement = element.get_Geometry(new Options() { ComputeReferences = true });
            List<Solid> solids = new List<Solid>();

            if (geometryElement == null)
            {
                return solids;
            }

            foreach (GeometryObject geometryObject in geometryElement)
            {
                if (geometryObject is Solid directSolid && directSolid.Volume > 0)
                {
                    solids.Add(directSolid);
                    continue;
                }

                GeometryInstance geometryInstance = geometryObject as GeometryInstance;
                if (geometryInstance == null)
                {
                    continue;
                }

                foreach (GeometryObject instanceObject in geometryInstance.GetInstanceGeometry())
                {
                    if (instanceObject is Solid instanceSolid && instanceSolid.Volume > 0)
                    {
                        solids.Add(instanceSolid);
                    }
                }
            }

            return solids;
        }

        private static Solid ApplyScaleBuffer(Solid solid, Vector buffer)
        {
            double scaleX = 1.0 + buffer.X;
            double scaleY = 1.0 + buffer.Y;
            double scaleZ = 1.0 + buffer.Z;

            if (scaleX <= 0 || scaleY <= 0 || scaleZ <= 0)
            {
                throw new Exception("Buffer values must be greater than -1 in each direction.");
            }

            if (scaleX.Equals(1.0) && scaleY.Equals(1.0) && scaleZ.Equals(1.0))
            {
                return solid;
            }

            XYZ centroid = solid.ComputeCentroid();
            Transform toOrigin = Transform.CreateTranslation(new XYZ(-centroid.X, -centroid.Y, -centroid.Z));
            Transform fromOrigin = Transform.CreateTranslation(centroid);

            Transform scaleTransform = Transform.Identity;
            scaleTransform.BasisX = new XYZ(scaleX, 0, 0);
            scaleTransform.BasisY = new XYZ(0, scaleY, 0);
            scaleTransform.BasisZ = new XYZ(0, 0, scaleZ);

            Transform finalTransform = fromOrigin.Multiply(scaleTransform).Multiply(toOrigin);
            return SolidUtils.CreateTransformed(solid, finalTransform);
        }

        /// <summary>
        /// This finds all the views an element appears in. Note: "Appears in" means that if it appears when you do a category collector, that counts.
        /// </summary>
        /// <param name="element">The element to analyze.</param>
        /// <returns name="views">The views this element appears in.</returns>
        [NodeCategory("Actions")]
        public static List<global::Revit.Elements.Element> ViewFinder(global::Revit.Elements.Element element)
        {
            List<global::Revit.Elements.Element> views = FindAllViewsWhereElementIsVisible(element.InternalElement).Select(v => v.ToDSType(false)).ToList();
            return views;
        }

        private static IEnumerable<Autodesk.Revit.DB.View> FindAllViewsThatCanDisplayElements(Autodesk.Revit.DB.Document doc)
        {
            ElementMulticlassFilter filter = new ElementMulticlassFilter(new List<Type> { typeof(View3D), typeof(ViewPlan), typeof(ViewSection) });

            return new FilteredElementCollector(doc).WherePasses(filter).Cast<View>().Where(v => !v.IsTemplate);
        }

        private static IEnumerable<Autodesk.Revit.DB.View> FindAllViewsWhereElementIsVisible(
            Autodesk.Revit.DB.Element element)
        {
            Document doc = element.Document;
            IEnumerable<View> relevantViewList = FindAllViewsThatCanDisplayElements(doc);

            ElementId idToCheck = element.Id;

            return (
                from v in relevantViewList
                let idList
                    = new FilteredElementCollector(doc, v.Id)
                        .WhereElementIsNotElementType()
                        .ToElementIds()
                where idList.Contains(idToCheck)
                select v);
        }
        /// <summary>
        /// Rotate an element in Revit given the angle and an optional rotation vector.
        /// </summary>
        /// <param name="element">The element to rotate</param>
        /// <param name="angle">How much to rotate?</param>
        /// <param name="vector">The vector to rotate about.</param>
        /// <returns name="Element">The rotated element</returns>
        public static global::Revit.Elements.Element SetRotation(global::Revit.Elements.Element element, double angle, [DefaultArgument("Rhythm.Utilities.MiscUtils.GetNull()")] Vector vector)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            var internalElement = element.InternalElement;

            Autodesk.Revit.DB.Line rotationLine = null;

            if (vector != null)
            {
                var vectorPoint = vector.ToXyz(true);

                var newPoint = new XYZ(vectorPoint.X, vectorPoint.Y, 0);

                rotationLine = Autodesk.Revit.DB.Line.CreateBound(newPoint, vector.ToXyz(true));
            }
            else
            {
                if (internalElement.Location is LocationPoint locationPoint)
                {
                    var secondPoint = locationPoint.Point;
                    var firstPoint = new XYZ(secondPoint.X, secondPoint.Y, secondPoint.Z - 100);

                    rotationLine = Autodesk.Revit.DB.Line.CreateBound(firstPoint, secondPoint);
                }
                else
                {
                    throw new Exception("Element is not point based. Cannot rotate.");
                }
            }


            TransactionManager.Instance.EnsureInTransaction(doc);
            ElementTransformUtils.RotateElement(doc, internalElement.Id, rotationLine, angle);
            TransactionManager.Instance.TransactionTaskDone();

            return element;
        }
    }
}
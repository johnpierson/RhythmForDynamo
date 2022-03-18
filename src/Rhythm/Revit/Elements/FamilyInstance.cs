using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using Point = Autodesk.DesignScript.Geometry.Point;
using Autodesk.Revit.UI;
using Dynamo.Graph.Nodes;
using RevitServices.Transactions;
using Document = Autodesk.Revit.DB.Document;
using ReferencePlane = Autodesk.Revit.DB.ReferencePlane;
using SketchPlane = Autodesk.Revit.DB.SketchPlane;
using StructuralType = Autodesk.Revit.DB.Structure.StructuralType;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for FamilyInstance.
    /// </summary>
    public class FamilyInstances
    {
        private FamilyInstances()
        {
        }
        /// <summary>
        /// This node will report the room the family instance resides in, (if available).
        /// </summary>
        /// <param name="instance">The family instance to obtain room info from.</param>
        /// <returns name="Room">The room in which the instance is located (during the last phase of the project).</returns>
        /// <search>
        /// room, rhythm,element.room
        /// </search>
        [NodeCategory("Query")]
        public static List<global::Revit.Elements.Element> Room(List<global::Revit.Elements.Element> instance)
        {

            //gets the room and converts to readable Revit element
            List<global::Revit.Elements.Element> room = new List<global::Revit.Elements.Element>();
            foreach (var i in instance)
            {
                var internalFamily = (Autodesk.Revit.DB.FamilyInstance)i.InternalElement;
                Document doc = internalFamily.Document;
                Phase phaseToUse = doc.GetElement(internalFamily.CreatedPhaseId) as Phase;
                try
                {
                    room.Add(internalFamily.Room.ToDSType(true));
                }
                catch
                {
                    try
                    {
                        var locationPoint = internalFamily.Location as LocationPoint;
                        room.Add(doc.GetRoomAtPoint(locationPoint.Point,phaseToUse).ToDSType(true));
                    }
                    catch (Exception)
                    {
                        room.Add(null);
                    }
                }
            }
            return room;
        }

        /// <summary>
        /// This node will report the room the family instance resides in, (if available).
        /// </summary>
        /// <param name="instance">The family instance to obtain room info from.</param>
        /// <param name="phase">The room to look in.</param>
        /// <returns name="Room">The room by phase in which the instance is located.</returns>
        /// <search>
        /// space, rhythm,element.space
        /// </search>
        [NodeCategory("Query")]
        public static global::Revit.Elements.Element RoomInPhase(global::Revit.Elements.Element instance, global::Revit.Elements.Element phase)
        {
            var internalFamily = (Autodesk.Revit.DB.FamilyInstance)instance.InternalElement;
            var internalPhase = phase.InternalElement as Autodesk.Revit.DB.Phase;
            try
            {
                return internalFamily.get_Room(internalPhase).ToDSType(true);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// This node will report the space the family instance resides in, (if available).
        /// </summary>
        /// <param name="instance">The family instance to obtain space info from.</param>
        /// <returns name="Space">The space in which the instance is located (during the last phase of the project).</returns>
        /// <search>
        /// space, rhythm,element.space
        /// </search>
        [NodeCategory("Query")]
        public static List<global::Revit.Elements.Element> Space(List<global::Revit.Elements.Element> instance)
        {
            //gets the space and converts to readable Revit element
            List<global::Revit.Elements.Element> space = new List<global::Revit.Elements.Element>();
            foreach (var i in instance)
            {
                var internalFamily = (Autodesk.Revit.DB.FamilyInstance)i.InternalElement;
                try
                {
                    space.Add(internalFamily.Space.ToDSType(true));
                }
                catch
                {
                    space.Add(null);
                }
            }
            return space;
        }


        /// <summary>
        /// This node will report the space the family instance resides in, (if available).
        /// </summary>
        /// <param name="instance">The family instance to obtain space info from.</param>
        /// <param name="phase">The phase to look in.</param>
        /// <returns name="Space">The space in which the instance is located (during the last phase of the project).</returns>
        /// <search>
        /// space, rhythm,element.space
        /// </search>
        [NodeCategory("Query")]
        public static global::Revit.Elements.Element SpaceInPhase(global::Revit.Elements.Element instance, global::Revit.Elements.Element phase)
        {
            var internalFamily = (Autodesk.Revit.DB.FamilyInstance)instance.InternalElement;
            var internalPhase = phase.InternalElement as Autodesk.Revit.DB.Phase;
            try
            {
                return internalFamily.get_Space(internalPhase).ToDSType(true);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// This node will find all deeply nested components in the given family instance.
        /// </summary>
        /// <param name="instance">The family instance to retrieve deep nested components from.</param>
        /// <returns name="nestedComponents">The nested components.</returns>
        /// <search>
        /// nested,
        /// </search>
        [NodeCategory("Query")]
        public static List<global::Revit.Elements.Element> RetrieveNestedComponents(global::Revit.Elements.FamilyInstance instance)
        {
            //the current document
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //the internal family list to loop through
            List<Autodesk.Revit.DB.FamilyInstance> internalFamily =
                new List<Autodesk.Revit.DB.FamilyInstance>
                {
                    (Autodesk.Revit.DB.FamilyInstance) instance.InternalElement
                };
            //output
            List<global::Revit.Elements.Element> nestedList = new List<global::Revit.Elements.Element>();

            //variable to step through
            int i = 0;
            //test case
            string s = "keepGoing";
            //loop da loop
            while (s == "keepGoing")
            {
                if (i != internalFamily.Count)
                {
                    foreach (ElementId subCom in internalFamily[i].GetSubComponentIds())
                    {
                        Autodesk.Revit.DB.FamilyInstance internalFam =
                            (Autodesk.Revit.DB.FamilyInstance)doc.GetElement(subCom);

                        internalFamily.Add(internalFam);
                        nestedList.Add(internalFam.ToDSType(true));
                    }
                    i++;
                    s = "keepGoing";
                }
                else
                {
                    s = "stop collaborate and listen";
                }
            }

            return nestedList;
        }

        /// <summary>
        /// This node will create and place a generic model family instance at all the room locations given the room element. This will close all family documents so keep that in mind!
        /// </summary>
        /// <param name="familyTemplatePath">The family template to use.</param>
        /// <param name="room">The room to convert to generic model.</param>
        /// <param name="materialName">The material to assign to the solid. *Note - The material has to exist in the family template to work. If it does not exist nothing will be assigned.</param>
        /// <param name="category">The category to assign to the family. *Note - this needs to be a category that works for families. (Doors, Generic Models, etc.)</param>
        /// <param name="subcategory">The subcategory to assign to the solid. *Note - this needs to exist in the family template, if it does not, nothing will be changed in this regard.</param>
        /// <returns name="familyInstance">The family instances that were placed.</returns>
        /// <search>
        /// space, rhythm,element.space
        /// </search>
        [NodeCategory("Create")]
        public static Dictionary<string, object> ByRoom(string familyTemplatePath, global::Revit.Elements.Room room, string materialName, global::Revit.Elements.Category category, string subcategory = "")
        {
            //variables
            global::Revit.Elements.Element famInstance = null;
            Autodesk.Revit.DB.FamilyInstance internalFam = null;
            bool fileFound = false;
            //the current document
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            UIApplication uiapp = new UIApplication(doc.Application);
            //convert the room to an Autodesk.Revit.DB representation
            Autodesk.Revit.DB.Architecture.Room internalRoom = (Autodesk.Revit.DB.Architecture.Room)room.InternalElement;
            string name = internalRoom.Name;
            //we close all of the families because we need to swap documents
            foreach (Document d in uiapp.Application.Documents)
            {
                if (d.IsFamilyDocument)
                {
                    d.Close(false);
                }
            }
            //create the new family document in the background and store it in memory
            Document familyDocument = uiapp.Application.NewFamilyDocument(familyTemplatePath);
            //instantiate a material element id and try to get the material that was specified
            ElementId material = null;
            FilteredElementCollector materialCollector = new FilteredElementCollector(familyDocument).OfClass(typeof(Autodesk.Revit.DB.Material));
            foreach (var m in materialCollector)
            {
                if (m.Name.ToLower().Replace(" ", "") == materialName.ToLower().Replace(" ", ""))
                {
                    material = m.Id;
                }
            }
            //close Dynamo's open transaction, we need to do this because we open a new Revit API transaction in the family document (This is document switching)
            TransactionManager.Instance.ForceCloseTransaction();
            //start creating the families.
            Transaction trans = new Transaction(familyDocument, "Generate Families Ya'll");
            trans.Start();
            //set the family category
            Autodesk.Revit.DB.Category familyCategory = familyDocument.Settings.Categories.get_Item(category.Name);
            familyDocument.OwnerFamily.FamilyCategory = familyCategory;
            //get the subcategory for the solids
            Autodesk.Revit.DB.Category subCategory = null;
            foreach (Autodesk.Revit.DB.Category c in familyCategory.SubCategories)
            {
                if (c.Name.ToLower() == subcategory.ToLower())
                {
                    subCategory = c;
                }
            }
            //get the height of the thing
            double height = room.Height;
            //get the curves
            IList<IList<BoundarySegment>> boundary = internalRoom.GetBoundarySegments(new SpatialElementBoundaryOptions());

            //generate a plane
            Autodesk.Revit.DB.Plane revitPlane = Autodesk.Revit.DB.Plane.CreateByNormalAndOrigin(Vector.ZAxis().ToXyz(), new XYZ(0, 0, 0));
            Autodesk.Revit.DB.SketchPlane sketchPlane = SketchPlane.Create(familyDocument, revitPlane);
            //the curve arrays to generate solids and voids in family
            CurveArray curveArray = new CurveArray();
            CurveArrArray curveArrArray = new CurveArrArray();
            CurveArray curveArrayVoid = new CurveArray();
            CurveArrArray curveArrArrayVoid = new CurveArrArray();
            //to perform  the cut action on the solid with the voids
            CombinableElementArray ceArray = new CombinableElementArray();
            //transform bizness
            Point roomCentroid = room.BoundingBox.ToCuboid().Centroid();
            Point locationPoint = Point.ByCoordinates(roomCentroid.X, roomCentroid.Y, 0);
            CoordinateSystem oldCS = CoordinateSystem.ByOrigin(locationPoint);
            CoordinateSystem newCS = CoordinateSystem.ByOrigin(0, 0, 0);

            //flag to step through the boundaries.
            int flag = 0;

            while (flag < boundary.Count)
            {
                //the first set of curves is the solid's boundary
                if (flag == 0)
                {
                    //generate the solid form which is the first item in the boundary segments.
                    foreach (BoundarySegment b in boundary[flag])
                    {
                        Autodesk.DesignScript.Geometry.Curve c = b.GetCurve().ToProtoType();
                        Autodesk.DesignScript.Geometry.Curve movedCurve = c.Transform(oldCS, newCS) as Autodesk.DesignScript.Geometry.Curve;
                        curveArray.Append(movedCurve.ToRevitType());
                    }
                    curveArrArray.Append(curveArray);
                    Extrusion solidExtrusion = familyDocument.FamilyCreate.NewExtrusion(true, curveArrArray, sketchPlane, height);
                    if (material != null)
                    {
                        //Set the material
                        Autodesk.Revit.DB.Parameter matParam = solidExtrusion.get_Parameter(BuiltInParameter.MATERIAL_ID_PARAM);
                        matParam.Set(material);
                    }
                    //try to set the subcategory
                    if (subCategory != null)
                    {
                        solidExtrusion.Subcategory = subCategory;
                    }
                }
                //subsequent lists of curves are representative of the voids
                else
                {
                    //clear the curves from the collection for all items after the second one. (index 2+)
                    if (!curveArrayVoid.IsEmpty)
                    {
                        curveArrayVoid.Clear();
                        curveArrArrayVoid.Clear();
                        ceArray.Clear();
                    }
                    //generate the void form
                    foreach (BoundarySegment b in boundary[flag])
                    {
                        Autodesk.DesignScript.Geometry.Curve c = b.GetCurve().ToProtoType();
                        Autodesk.DesignScript.Geometry.Curve movedCurve = c.Transform(oldCS, newCS) as Autodesk.DesignScript.Geometry.Curve;
                        curveArrayVoid.Append(movedCurve.ToRevitType());
                    }
                    curveArrArrayVoid.Append(curveArrayVoid);
                    Extrusion voidExtrusion = familyDocument.FamilyCreate.NewExtrusion(false, curveArrArrayVoid, sketchPlane, height);

                    //try to combine things
                    foreach (Extrusion genericForm in new FilteredElementCollector(familyDocument).OfClass(typeof(Extrusion))
                        .Cast<Extrusion>())
                    {
                        ceArray.Append(genericForm);
                    }
                    //to add the void to the solid
                    familyDocument.CombineElements(ceArray);
                }
                flag++;
            }
            familyDocument.Regenerate();
            trans.Commit();

            Autodesk.Revit.DB.Family fam = null;
            //build the temporary path
            string familyFilePath = Path.GetTempPath() + name + ".rfa";

            SaveAsOptions opt = new SaveAsOptions();
            opt.OverwriteExistingFile = true;
            familyDocument.SaveAs(familyFilePath, opt);
            familyDocument.Close(false);

            TransactionManager.Instance.ForceCloseTransaction();
            Transaction trans2 = new Transaction(doc, "Attempting to place or update Room family instances.");
            trans2.Start();
            IFamilyLoadOptions loadOptions = new FamilyImportOptions();
            bool variable = true;
            loadOptions.OnFamilyFound(true, out variable);
            doc.LoadFamily(familyFilePath, loadOptions, out fam);

            FamilySymbol familySymbol = (FamilySymbol)doc.GetElement(fam.GetFamilySymbolIds().First());
            //try to find if it is placed already
            FilteredElementCollector col = new FilteredElementCollector(doc);
            //get built in category from user viewable category
            BuiltInCategory myCatEnum =
                (BuiltInCategory)Enum.Parse(typeof(BuiltInCategory), category.Id.ToString());
            foreach (Autodesk.Revit.DB.Element e in col.WhereElementIsNotElementType().OfCategory(myCatEnum).ToElements())
            {
                Autodesk.Revit.DB.FamilySymbol type = (FamilySymbol)doc.GetElement(e.GetTypeId());
                if (type.FamilyName.Equals(name))
                {
                    fileFound = true;
                    internalFam = e as Autodesk.Revit.DB.FamilyInstance;
                }
            }
            //place families that are not placed
            if (fileFound == false)
            {
                if (!familySymbol.IsActive)
                {
                    familySymbol.Activate();
                }

                internalFam = doc.Create.NewFamilyInstance(new XYZ(roomCentroid.X, roomCentroid.Y, 0), familySymbol, internalRoom.Level,
                    StructuralType.NonStructural);
            }
            trans2.Commit();
            //delete the temp file
            File.Delete(familyFilePath);
            //cast to Dynamo type for output and location updating (if necessary)
            famInstance = internalFam.ToDSType(true);
            if (fileFound)
            {
                famInstance.SetLocation(locationPoint);
            }
            //returns the outputs
            var outInfo = new Dictionary<string, object>
            {
                { "familyInstance", famInstance}
            };
            return outInfo;
        }   
    }
}

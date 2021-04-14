using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Nuclex.Game.Packing;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Element = Revit.Elements.Element;

namespace Rhythm.Revit.Views
{
    /// <summary>
    /// A Revit ViewSheet
    /// </summary>
    public class Sheet
    {
        private Sheet()
        {
        }
        /// <summary>
        /// This node will attempt to pack the given views on a sheet. For views that do not fit it will output a list of them.
        /// </summary>
        /// <param name="sheet">The sheet to add views to.</param>
        /// <param name="views">The views to try to add.</param>
        /// <returns name="success">Views that were added.</returns>
        /// <returns name="fail">Views that did not fit within the outline of the sheet.</returns>
        /// <search>
        /// Sheet.AddViews,Pack
        /// </search>
        [MultiReturn(new[] { "viewsThatFit", "viewsThatDoNotFit" })]
        public static Dictionary<string, object> PackViews(global::Revit.Elements.Element sheet, IEnumerable<global::Revit.Elements.Views.View> views)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //success list
            List<global::Revit.Elements.Element> successKid = new List<Element>();
            //fail list
            List<global::Revit.Elements.Element> badLuckBrian = new List<Element>();

            Autodesk.Revit.DB.ViewSheet internalSheet = (Autodesk.Revit.DB.ViewSheet)sheet.InternalElement;

            TransactionManager.Instance.EnsureInTransaction(doc);
            //attempt to find the titleblock on the sheet
            Autodesk.Revit.DB.Element titleblock = new FilteredElementCollector(doc, internalSheet.Id).OfCategory(BuiltInCategory.OST_TitleBlocks).FirstElement();
            double width;
            double height;

            if (titleblock != null)
            {
                 width = titleblock.get_BoundingBox(internalSheet).Max.X -
             titleblock.get_BoundingBox(internalSheet).Min.X;
                 height = titleblock.get_BoundingBox(internalSheet).Max.Y -
                             titleblock.get_BoundingBox(internalSheet).Min.Y;
            }
            else
            {
                 width = internalSheet.Outline.Max.U - internalSheet.Outline.Min.U;
                 height = internalSheet.Outline.Max.V - internalSheet.Outline.Min.V;
            }




            var packer = new CygonRectanglePacker(width, height);
            int count = 0;

            foreach (var v in views)
            {
                Autodesk.Revit.DB.View view = (Autodesk.Revit.DB.View)v.InternalElement;

                var viewWidth = view.Outline.Max.U - view.Outline.Min.U;
                var viewHeight = view.Outline.Max.V - view.Outline.Min.V;

                Autodesk.Revit.DB.UV placement = null;
                if (packer.TryPack(viewWidth, viewHeight, out placement))
                {
                    XYZ bottomLeft = titleblock.get_BoundingBox(internalSheet).Min;
                    double bottomLeftX;
                    double bottomLeftY;

                    if (bottomLeft != null)
                    {
                        bottomLeftX = bottomLeft.X;
                        bottomLeftY = bottomLeft.Y;
                    }
                    else
                    {
                        bottomLeftX = 0;
                        bottomLeftY = 0;
                    }
                    

                    var dbViews = internalSheet.GetAllPlacedViews().Select(x => doc.GetElement(x)).
                        OfType<Autodesk.Revit.DB.View>();
                    if (dbViews.Contains(view))
                    {
                        //move the view
                        //find the corresponding viewport
                        var enumerable =
                            DocumentManager.Instance.ElementsOfType<Autodesk.Revit.DB.Viewport>()
                                .Where(x => x.SheetId == internalSheet.Id && x.ViewId == view.Id).ToArray();

                        if (!enumerable.Any())
                            continue;

                        var viewport = enumerable.First();
                        viewport.SetBoxCenter(new XYZ(placement.U + bottomLeftX + viewWidth / 2, placement.V + bottomLeftY + viewHeight / 2, 0));
                    }
                    else
                    {
                        //place the view on the sheet
                        if (Viewport.CanAddViewToSheet(doc, internalSheet.Id, view.Id))
                        {
                            var viewport = Viewport.Create(doc, internalSheet.Id, view.Id,
                                                           new XYZ(placement.U + bottomLeftX + viewWidth / 2, placement.V + bottomLeftY + viewHeight / 2, 0));
                        }
                        successKid.Add(v);
                    }
                }
                else
                {
                    //var viewport = Viewport.Create(doc, internalSheet.Id, view.Id,
                    //new XYZ(0,0,0));
                    badLuckBrian.Add(v);
                }

                count++;
            }

            TransactionManager.Instance.TransactionTaskDone();
            //returns the outputs
            var outInfo = new Dictionary<string, object>
            {
                {"viewsThatFit", successKid},
                { "viewsThatDoNotFit",badLuckBrian}
            };
            return outInfo;
        }
    }
}
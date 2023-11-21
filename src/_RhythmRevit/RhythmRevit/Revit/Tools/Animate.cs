using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitServices.Persistence;
using RevitServices.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using Color = DSCore.Color;
using FillPatternElement = Autodesk.Revit.DB.FillPatternElement;

namespace Rhythm.Revit.Tools
{
    /// <summary>
    /// Wrapper class for animators
    /// </summary>
    public class Element
    {
        private Element()
        { }
#if !R20
        /// <summary>
        /// Animate a numeric parameter of an element. This will export images of the parameter, then revert the element back to where it was. Also adds text to comments to prevent infinite loops.Clear this comment for subsequent runs.
        /// Inspired by the Bad Monkeys Team.
        /// </summary>
        /// <param name="element">The element to set parameter to.</param>
        /// <param name="parameterName">The parameter name.</param>
        /// <param name="startValue">The value to set.</param>
        /// <param name="endValue">The value to set.</param>
        /// <param name="iterations">The number of images.</param>
        /// <param name="directoryPath">Where to save the images.</param>
        /// <returns name="element">The element.</returns>
        /// <search>
        ///  rhythm
        /// </search>
        public static object AnimateNumericParameter(List<global::Revit.Elements.Element> element, string parameterName, double startValue, double endValue, int iterations, string directoryPath)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            UIDocument uiDocument = new UIDocument(doc);
            object runResult;
            //create a new form!
            DefaultProgressForm statusBar = new DefaultProgressForm("Exporting Images", "Exporting image {0} of " + iterations.ToString(), "Animate Numeric Parameter", iterations);

            //this finds the number to increment by
            double d = (endValue - startValue) / (iterations - 1);

            if (element.First().GetParameterValueByName("Comments").ToString() != "already animated, clear this to run again.")
            {
                //starts a transaction group so we can rollback the changes after
                using (TransactionGroup transactionGroup = new TransactionGroup(doc, "group"))
                {
                    TransactionManager.Instance.ForceCloseTransaction();
                    transactionGroup.Start();
                    using (Transaction t2 = new Transaction(doc, "Modify parameter"))
                    {
                        int num2 = 0;
                        while (startValue <= endValue)
                        {
                            statusBar.Activate();
                            t2.Start();
                            foreach (var e in element)
                            {
                                var parameter = e.InternalElement.LookupParameter(parameterName);

                                string paramType = string.Empty;

                                //autodesk.unit.unit:degrees-1.0.1
                                string versionNumber = DocumentManager.Instance.CurrentUIApplication.Application.VersionNumber;


                          paramType = parameter.GetUnitTypeId().TypeId;        



                                if (paramType.ToLower().Contains("degrees"))
                                {
                                    parameter.Set(startValue * System.Math.PI / 180.0);
                                }
                                else
                                {
                                    parameter.Set(startValue);
                                }
                            }

                            t2.Commit();
                            uiDocument.RefreshActiveView();
                            var exportOpts = new ImageExportOptions
                            {
                                FilePath = directoryPath + num2.ToString(),
                                FitDirection = FitDirectionType.Horizontal,
                                HLRandWFViewsFileType = ImageFileType.PNG,
                                ImageResolution = ImageResolution.DPI_300,
                                ShouldCreateWebSite = false
                            };
                            doc.ExportImage(exportOpts);
                            ++num2;
                            startValue += d;
                            statusBar.Increment();
                        }
                    }
                    transactionGroup.RollBack();

                }
            }
            runResult = "This element has already been animated, clear the comments from the element and run again.";
            foreach (var e in element)
            {
                e.SetParameterByName("Comments", "already animated, clear this to run again.");
            }
            statusBar.Close();

            return runResult;
        }
#endif
        /// <summary>
        /// Animate the color of an element. This will export images of the element, then revert the element back to where it was.
        /// Inspired by the Bad Monkeys team.
        /// </summary>
        /// <param name="element">The element to set color to.</param>
        /// <param name="startColor">The start color.</param>
        /// <param name="endColor">The end color.</param>
        /// <param name="iterations">Numnber of images.</param>
        /// <param name="directoryPath">Where to save the images.</param>
        /// <param name="view">View to export from.</param>
        /// <returns name="element">The element.</returns>
        /// <search>
        ///  rhythm
        /// </search>
        public static object AnimateColor(List<global::Revit.Elements.Element> element, Color startColor, Color endColor, int iterations, string directoryPath, global::Revit.Elements.Element view)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            UIDocument uiDocument = new UIDocument(doc);
            Autodesk.Revit.DB.View internalView = (Autodesk.Revit.DB.View)view.InternalElement;
            //create a new form!
            DefaultProgressForm statusBar = new DefaultProgressForm("Exporting Images", "Exporting image {0} of " + iterations.ToString(), "Animate Element Color", iterations + 1);
            //default indices for start and end color
            List<double> defaultIndices = new List<double>
            {
                0,
                1
            };
            //the color list generated from start and end color
            List<Color> colorList = new List<Color>
            {
                startColor,
                endColor
            };
            //where to start
            double startValue = 0;

            //starts a transaction group so we can roolback the changes after
            using (TransactionGroup transactionGroup = new TransactionGroup(doc, "group"))
            {
                TransactionManager.Instance.ForceCloseTransaction();
                transactionGroup.Start();
                using (Transaction t2 = new Transaction(doc, "Animate Color"))
                {
                    int num2 = 0;
                    while (startValue <= 1)
                    {
                        statusBar.Activate();
                        t2.Start();
                        //declare the graphic settings overrides
                        OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                        //generate color range
                        Color dscolor = DSCore.Color.BuildColorFrom1DRange(colorList, defaultIndices, startValue);
                        //convert to revit color
                        Autodesk.Revit.DB.Color revitColor = new Autodesk.Revit.DB.Color(dscolor.Red, dscolor.Green,
                            dscolor.Blue);
                        //solid fill id
                        FilteredElementCollector fillPatternCollector = new FilteredElementCollector(doc);
                        Autodesk.Revit.DB.Element solidFill = fillPatternCollector.OfClass(typeof(FillPatternElement)).ToElements().FirstOrDefault(x => x.Name.ToLower() == "solid fill");

#if R20 || R21 || R22 || R23
                        ElementId pattId = new ElementId(20);
#endif

#if R24_OR_GREATER
                        ElementId pattId = new ElementId(System.Convert.ToInt64(20));
#endif

                        //set the overrides to the graphic settings
                        ogs.SetSurfaceForegroundPatternColor(revitColor);
                        ogs.SetSurfaceForegroundPatternId(solidFill.Id);
                        foreach (var e in element)
                        {
                            //apply the changes to view
                            internalView.SetElementOverrides(e.InternalElement.Id, ogs);
                        }
                        t2.Commit();

                        uiDocument.RefreshActiveView();
                        var exportOpts = new ImageExportOptions
                        {
                            FilePath = directoryPath + num2.ToString(),
                            FitDirection = FitDirectionType.Horizontal,
                            HLRandWFViewsFileType = ImageFileType.PNG,
                            ImageResolution = ImageResolution.DPI_300,
                            ShouldCreateWebSite = false
                        };
                        doc.ExportImage(exportOpts);
                        ++num2;
                        startValue += (1.0 / iterations);
                        statusBar.Increment();
                    }
                }
                transactionGroup.RollBack();
            }
            statusBar.Close();

            return element;
        }

        /// <summary>
        /// Animate the transparency of an element. This will export images of the element, then revert the element back to where it was.
        /// Inspired by the Bad Monkeys team.
        /// </summary>
        /// <param name="element">The element to set transparency to.</param>
        /// <param name="startPercentage">The transparency start percent.</param>
        /// <param name="endPercentage">The transparency end percent.</param>
        /// <param name="iterations">Numnber of images.</param>
        /// <param name="directoryPath">Where to save the images.</param>
        /// <param name="view">View to export from.</param>
        /// <returns name="element">The element.</returns>
        /// <search>
        ///  rhythm
        /// </search>
        public static object AnimateTransparency(List<global::Revit.Elements.Element> element, int startPercentage, int endPercentage, int iterations, string directoryPath, global::Revit.Elements.Element view)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            UIDocument uiDocument = new UIDocument(doc);
            Autodesk.Revit.DB.View internalView = (Autodesk.Revit.DB.View)view.InternalElement;
            //create a new form!
            DefaultProgressForm statusBar = new DefaultProgressForm("Exporting Images", "Exporting image {0} of " + iterations.ToString(), "Animate Element Transparency", iterations + 1);
            double d = (endPercentage - startPercentage) / (iterations - 1.0);
            int incrementor = Convert.ToInt32(d);


            //starts a transaction group so we can roolback the changes after
            using (TransactionGroup transactionGroup = new TransactionGroup(doc, "group"))
            {
                TransactionManager.Instance.ForceCloseTransaction();
                transactionGroup.Start();
                using (Transaction t2 = new Transaction(doc, "Modify parameter"))
                {
                    int num2 = 0;
                    while (startPercentage <= endPercentage)
                    {
                        statusBar.Activate();
                        t2.Start();
                        //declare the graphic settings overrides
                        OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                        //solid fill id
#if R20 || R21 || R22 || R23
                        ElementId pattId = new ElementId(20);
#endif

#if R24_OR_GREATER
                        ElementId pattId = new ElementId(Convert.ToInt64(20));
#endif

                        //set the overrides to the graphic settings
                        ogs.SetSurfaceTransparency(startPercentage);
                        foreach (var e in element)
                        {
                            //apply the changes to view
                            internalView.SetElementOverrides(e.InternalElement.Id, ogs);
                        }
                        t2.Commit();

                        uiDocument.RefreshActiveView();
                        var exportOpts = new ImageExportOptions
                        {
                            FilePath = directoryPath + num2.ToString(),
                            FitDirection = FitDirectionType.Horizontal,
                            HLRandWFViewsFileType = ImageFileType.PNG,
                            ImageResolution = ImageResolution.DPI_300,
                            ShouldCreateWebSite = false
                        };
                        doc.ExportImage(exportOpts);
                        ++num2;
                        startPercentage += incrementor;
                        statusBar.Increment();
                    }
                }
                transactionGroup.RollBack();
            }
            statusBar.Close();

            return element;
        }
    }
}

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

        /// <summary>
        /// Finds the solid fill pattern by asking the patterns themselves, rather than by matching
        /// the English display name "solid fill" - which does not exist on a localised Revit, and
        /// which a user is free to rename.
        /// </summary>
        [Autodesk.DesignScript.Runtime.IsVisibleInDynamoLibrary(false)]
        private static ElementId GetSolidFillPatternId(Autodesk.Revit.DB.Document doc)
        {
            var solidFill = new FilteredElementCollector(doc)
                .OfClass(typeof(FillPatternElement))
                .Cast<FillPatternElement>()
                .FirstOrDefault(f => f.GetFillPattern().IsSolidFill);

            if (solidFill == null)
            {
                throw new InvalidOperationException(
                    "This document has no solid fill pattern, so the elements cannot be colour-filled.");
            }

            return solidFill.Id;
        }

        /// <summary>
        /// Builds the path for one exported frame.
        /// </summary>
        /// <remarks>
        /// The frame number used to be concatenated straight onto the directory path, so a
        /// directoryPath of "C:\Exports" wrote C:\Exports0.png, C:\Exports1.png ... beside the
        /// folder rather than inside it. Numbers are zero-padded so that frames sort in order
        /// lexicographically, which is what video assembly tools expect.
        /// </remarks>
        [Autodesk.DesignScript.Runtime.IsVisibleInDynamoLibrary(false)]
        private static string BuildFramePath(string directoryPath, int frameNumber)
        {
            System.IO.Directory.CreateDirectory(directoryPath);
            return System.IO.Path.Combine(directoryPath, frameNumber.ToString("D4"));
        }
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
                                FilePath = BuildFramePath(directoryPath, num2),
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

            //Resolve the solid fill pattern once, by asking each pattern whether it IS solid rather
            //than comparing its display name to the English "solid fill". The name is localised, so
            //on a German or Japanese Revit the old lookup returned null and the next line threw a
            //NullReferenceException inside an open transaction. It was also re-collected on every
            //frame of the animation.
            ElementId solidFillId = GetSolidFillPatternId(doc);

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
                        //set the overrides to the graphic settings
                        ogs.SetSurfaceForegroundPatternColor(revitColor);
                        ogs.SetSurfaceForegroundPatternId(solidFillId);
                        foreach (var e in element)
                        {
                            //apply the changes to view
                            internalView.SetElementOverrides(e.InternalElement.Id, ogs);
                        }
                        t2.Commit();

                        uiDocument.RefreshActiveView();
                        var exportOpts = new ImageExportOptions
                        {
                            FilePath = BuildFramePath(directoryPath, num2),
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
            if (iterations < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(iterations),
                    "Animating a transparency range needs at least two frames.");
            }

            DefaultProgressForm statusBar = new DefaultProgressForm("Exporting Images", "Exporting image {0} of " + iterations.ToString(), "Animate Element Transparency", iterations + 1);

            //Step as a double and drive the loop by frame index. The previous version rounded the
            //step to an int, so any range narrower than about half a percent per frame - 0 to 50%
            //over 101 frames, say - produced a step of 0. startPercentage then never advanced, the
            //loop exported the same 300 DPI image forever, and the only thing that stopped it was
            //the progress bar throwing once it was incremented past its own maximum.
            double step = (endPercentage - startPercentage) / (iterations - 1.0);

            //starts a transaction group so we can roolback the changes after
            using (TransactionGroup transactionGroup = new TransactionGroup(doc, "group"))
            {
                TransactionManager.Instance.ForceCloseTransaction();
                transactionGroup.Start();
                using (Transaction t2 = new Transaction(doc, "Modify parameter"))
                {
                    int num2 = 0;
                    for (int frame = 0; frame < iterations; frame++)
                    {
                        int currentPercentage = (int)System.Math.Round(startPercentage + (step * frame));
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
                        ogs.SetSurfaceTransparency(currentPercentage);
                        foreach (var e in element)
                        {
                            //apply the changes to view
                            internalView.SetElementOverrides(e.InternalElement.Id, ogs);
                        }
                        t2.Commit();

                        uiDocument.RefreshActiveView();
                        var exportOpts = new ImageExportOptions
                        {
                            FilePath = BuildFramePath(directoryPath, num2),
                            FitDirection = FitDirectionType.Horizontal,
                            HLRandWFViewsFileType = ImageFileType.PNG,
                            ImageResolution = ImageResolution.DPI_300,
                            ShouldCreateWebSite = false
                        };
                        doc.ExportImage(exportOpts);
                        ++num2;
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

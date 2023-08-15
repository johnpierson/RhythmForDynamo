using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Dynamo.Graph.Nodes;
using Dimension = Autodesk.Revit.DB.Dimension;
using Line = Autodesk.DesignScript.Geometry.Line;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace Rhythm.Revit.Elements
{
    // ReSharper disable PossibleInvalidOperationException
    /// <summary>
    /// Wrapper class for dimensions.
    /// </summary>
    public class Dimensions
    {
        private Dimensions()
        {
        }

        /// <summary>
        /// This node will get the dimension's line.
        /// </summary>
        /// <param name="dimension">The dimension to obtain the curve from.</param>
        /// <returns name="dimensionLine">The curve representing the dimension.</returns>
        /// <search>
        /// Dimension.GetCurve, rhythm
        /// </search>
        [NodeCategory("Query")]
        public static List<Autodesk.DesignScript.Geometry.Line> GetCurve(global::Revit.Elements.Dimension dimension)
        {
            //convert to internal dimension
            Autodesk.Revit.DB.Dimension internalDimension = (Autodesk.Revit.DB.Dimension) dimension.InternalElement;
            Document doc = internalDimension.Document;
            List<Autodesk.DesignScript.Geometry.Line> dimensionLine = new List<Line>();

            if (internalDimension.NumberOfSegments > 1)
            {
                List<Autodesk.Revit.DB.DimensionSegment> dimSegments = new List<Autodesk.Revit.DB.DimensionSegment>();
                foreach (Autodesk.Revit.DB.DimensionSegment seg in internalDimension.Segments)
                {
                    dimSegments.Add(seg);
                }

                foreach (Autodesk.Revit.DB.DimensionSegment s in dimSegments)
                {
                    var originPoint = s.Origin.ToPoint();
                    Autodesk.Revit.DB.Line dimCurve = (Autodesk.Revit.DB.Line) internalDimension.Curve;
                    var vector = dimCurve.Direction.ToVector();
                    var dimValue = s.Value.Value;
                    if (doc.DisplayUnitSystem.ToString() != "IMPERIAL")
                    {
                        dimValue = s.Value.Value * 304.8;
                    }

                    var startPoint =
                        (Autodesk.DesignScript.Geometry.Point) originPoint.Translate(vector.Reverse(), dimValue / 2);
                    var endPoint = (Autodesk.DesignScript.Geometry.Point) originPoint.Translate(vector, dimValue / 2);
                    dimensionLine.Add(Autodesk.DesignScript.Geometry.Line.ByStartPointEndPoint(startPoint, endPoint));
                }
            }
            else
            {
                var originPoint = internalDimension.Origin.ToPoint();
                Autodesk.Revit.DB.Line dimCurve = (Autodesk.Revit.DB.Line) internalDimension.Curve;
                var vector = dimCurve.Direction.ToVector();

                var dimValue = internalDimension.Value.Value;
                if (doc.DisplayUnitSystem.ToString() != "IMPERIAL")
                {
                    dimValue = internalDimension.Value.Value * 304.8;
                }

                var startPoint =
                    (Autodesk.DesignScript.Geometry.Point) originPoint.Translate(vector.Reverse(), dimValue / 2);
                var endPoint = (Autodesk.DesignScript.Geometry.Point) originPoint.Translate(vector, dimValue / 2);
                dimensionLine.Add(Autodesk.DesignScript.Geometry.Line.ByStartPointEndPoint(startPoint, endPoint));
            }

            return dimensionLine;
        }

        /// <summary>
        /// This node will return the segments comprising the multi segment dimension.
        /// </summary>
        /// <param name="dimension">Multi segment dimension.</param>
        /// <returns name="segmentArray">The individual members.</returns>
        /// <search>
        /// dimension.Segments
        /// </search>
        [NodeCategory("Query")]
        public static List<Autodesk.Revit.DB.DimensionSegment> Segments(global::Revit.Elements.Dimension dimension)
        {
            Autodesk.Revit.DB.Dimension internalDimension = dimension.InternalElement as Autodesk.Revit.DB.Dimension;

            List<Autodesk.Revit.DB.DimensionSegment> segments = new List<Autodesk.Revit.DB.DimensionSegment>();

            foreach (Autodesk.Revit.DB.DimensionSegment seg in internalDimension.Segments)
            {
                segments.Add(seg);
            }

            return segments;
        }

        /// <summary>
        /// This node will check if the dimension has any overrides in it of text.
        /// </summary>
        /// <param name="dimension"> The dimension to check</param>
        /// <returns name="result">True = overridden with text, False = not so overridden.</returns>
        /// <search>
        /// dimension.IsOverriden
        /// </search>
        [NodeCategory("Query")]
        public static bool IsOverriden(global::Revit.Elements.Dimension dimension)
        {
            List<bool> listReturn = new List<bool>();
            foreach (var v in dimension.ValueOverride)
            {
                try
                {
                    listReturn.Add(!v.Equals(""));
                }
                catch (Exception)
                {
                    listReturn.Add(false);
                }
            }

            return listReturn.Contains(true);
        }

#if R20
#else
/// <summary>
        /// This node will return the display unit type for the given dimension.
        /// </summary>
        /// <param name="dimension">The dimension.</param>
        /// <returns name="displayUnits">The display unit type.</returns>
        /// <search>
        /// dimension.Properties, Dimension.DisplayUnits
        /// </search>
        [NodeCategory("Query")]
        public static string DisplayUnits(global::Revit.Elements.Dimension dimension)
        {
            string units = string.Empty;

            Dimension dim = dimension.InternalElement as Dimension;



            return dim.DimensionType.GetUnitsFormatOptions().GetUnitTypeId().TypeId;

        }
#endif


        /// <summary>
        /// This node will return the accuracy for the given dimension.
        /// </summary>
        /// <param name="dimension">The dimension.</param>
        /// <returns name="accuracy">The accuracy.</returns>
        /// <search>
        /// dimension.Properties, Dimension.DisplayUnits
        /// </search>
        [NodeCategory("Query")]
        public static object Accuracy(global::Revit.Elements.Dimension dimension)
        {
            Dimension dim = dimension.InternalElement as Dimension;
            try
            {
                return dim.DimensionType.GetUnitsFormatOptions().Accuracy;
            }
            catch (Exception)
            {
                return "UseDefault";
            }
        }

        /// <summary>
        /// This node will return the color for the given dimension.
        /// </summary>
        /// <param name="dimension">The dimension.</param>
        /// <returns name="color">The color.</returns>
        /// <search>
        /// dimension.Properties, Dimension.Color
        /// </search>
        [NodeCategory("Query")]
        public static DSCore.Color Color(global::Revit.Elements.Dimension dimension)
        {
            Dimension dim = dimension.InternalElement as Dimension;
            //get the integer value of the color
            int colorInt = dim.DimensionType.get_Parameter(BuiltInParameter.LINE_COLOR).AsInteger();
            //windows color from int
            global::System.Drawing.Color winColor = ColorTranslator.FromOle(colorInt);
            //return dynamo color
            return DSCore.Color.ByARGB(winColor.A, winColor.R, winColor.G, winColor.B);
        }

        /// <summary>
        /// This node will try to set the above value for the dimensions. This will work for either single segment dimensions or all segments of a multi-segment dimension.
        /// </summary>
        /// <param name="dimension">Dimension to set</param>
        /// <param name="aboveValue">Value to set.</param>
        /// <returns name="set">Successfully set.</returns>
        /// <returns name="notSet">Not so successfully set.</returns>
        /// <search>
        /// dimension.SetAboveValue
        /// </search>
        [MultiReturn(new[] {"set", "notSet"})]
        [NodeCategory("Actions")]
        public static Dictionary<string, object> SetAboveValue(global::Revit.Elements.Dimension dimension,
            List<string> aboveValue)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //convert to internal dimension
            Autodesk.Revit.DB.Dimension internalDimension = (Autodesk.Revit.DB.Dimension) dimension.InternalElement;
            List<Autodesk.Revit.DB.Dimension> internalDimensionList =
                new List<Autodesk.Revit.DB.Dimension> {internalDimension};
            //lists to output success or not success
            List<global::Revit.Elements.Element> set = new List<global::Revit.Elements.Element>();
            List<global::Revit.Elements.Element> notSet = new List<global::Revit.Elements.Element>();

            try
            {
                if (internalDimension.NumberOfSegments > 1) //set the multi segment ones
                {
                    List<Autodesk.Revit.DB.DimensionSegment> dimSegments =
                        new List<Autodesk.Revit.DB.DimensionSegment>();
                    foreach (Autodesk.Revit.DB.DimensionSegment seg in internalDimension.Segments)
                    {
                        dimSegments.Add(seg);
                    }

                    //check if lists are even
                    bool cond = aboveValue.Count < 2 && dimSegments.Count > 1;
                    IEnumerable<string> repeatedSequence = Enumerable.Repeat(aboveValue.First(), dimSegments.Count);

                    if (cond)
                    {
                        int count = 0;
                        TransactionManager.Instance.EnsureInTransaction(doc);
                        while (count < dimSegments.Count)
                        {
                            dimSegments[count].Above = repeatedSequence.ElementAt(count);
                            count++;
                        }

                        TransactionManager.Instance.TransactionTaskDone();
                        set.Add(dimension);
                    }
                    else
                    {
                        int count = 0;
                        TransactionManager.Instance.EnsureInTransaction(doc);
                        while (count < dimSegments.Count)
                        {
                            dimSegments[count].Above = aboveValue[count];
                            count++;
                        }

                        TransactionManager.Instance.TransactionTaskDone();
                        set.Add(dimension);
                    }
                }
                else // set the single ones
                {
                    //check if lists are even
                    bool cond = aboveValue.Count < 2 && internalDimensionList.Count > 1;
                    IEnumerable<string> repeatedSequence =
                        Enumerable.Repeat(aboveValue.First(), internalDimensionList.Count);

                    if (cond)
                    {
                        int count = 0;
                        TransactionManager.Instance.EnsureInTransaction(doc);
                        while (count < internalDimensionList.Count)
                        {
                            internalDimensionList[count].Above = repeatedSequence.ElementAt(count);
                            count++;
                        }

                        TransactionManager.Instance.TransactionTaskDone();
                        set.Add(dimension);
                    }
                    else
                    {
                        int count = 0;
                        TransactionManager.Instance.EnsureInTransaction(doc);
                        while (count < internalDimensionList.Count)
                        {
                            internalDimensionList[count].Above = aboveValue[count];
                            count++;
                        }

                        TransactionManager.Instance.TransactionTaskDone();
                        set.Add(dimension);
                    }
                }
            }
            catch (Exception)
            {
                notSet.Add(dimension);
            }

            //returns the outputs
            var outInfo = new Dictionary<string, object>
            {
                {"set", set},
                {"notSet", notSet}
            };
            return outInfo;
        }

        /// <summary>
        /// This node will try to set the below value for the dimensions. This will work for either single segment dimensions or all segments of a multi-segment dimension.
        /// </summary>
        /// <param name="dimension">Dimension to set</param>
        /// <param name="belowValue">Value to set.</param>
        /// <returns name="set">Successfully set.</returns>
        /// <returns name="notSet">Not so successfully set.</returns>
        /// <search>
        /// dimension.SetBelowValue
        /// </search>
        [MultiReturn(new[] {"set", "notSet"})]
        [NodeCategory("Actions")]
        public static Dictionary<string, object> SetBelowValue(global::Revit.Elements.Dimension dimension,
            List<string> belowValue)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //convert to internal dimension
            Autodesk.Revit.DB.Dimension internalDimension = (Autodesk.Revit.DB.Dimension) dimension.InternalElement;
            List<Autodesk.Revit.DB.Dimension> internalDimensionList =
                new List<Autodesk.Revit.DB.Dimension> {internalDimension};
            //lists to output success or not success
            List<global::Revit.Elements.Element> set = new List<global::Revit.Elements.Element>();
            List<global::Revit.Elements.Element> notSet = new List<global::Revit.Elements.Element>();

            try
            {
                if (internalDimension.NumberOfSegments > 1) //set the multi segment ones
                {
                    List<Autodesk.Revit.DB.DimensionSegment> dimSegments =
                        new List<Autodesk.Revit.DB.DimensionSegment>();
                    foreach (Autodesk.Revit.DB.DimensionSegment seg in internalDimension.Segments)
                    {
                        dimSegments.Add(seg);
                    }

                    //check if lists are even
                    bool cond = belowValue.Count < 2 && dimSegments.Count > 1;
                    IEnumerable<string> repeatedSequence = Enumerable.Repeat(belowValue.First(), dimSegments.Count);
                    if (cond)
                    {
                        int count = 0;
                        TransactionManager.Instance.EnsureInTransaction(doc);
                        while (count < dimSegments.Count)
                        {
                            dimSegments[count].Below = repeatedSequence.ElementAt(count);
                            count++;
                        }

                        TransactionManager.Instance.TransactionTaskDone();
                        set.Add(dimension);
                    }
                    else
                    {
                        int count = 0;
                        TransactionManager.Instance.EnsureInTransaction(doc);
                        while (count < dimSegments.Count)
                        {
                            dimSegments[count].Below = belowValue[count];
                            count++;
                        }

                        TransactionManager.Instance.TransactionTaskDone();
                        set.Add(dimension);
                    }
                }
                else // set the single ones
                {
                    //check if lists are even
                    bool cond = belowValue.Count < 2 && internalDimensionList.Count > 1;
                    IEnumerable<string> repeatedSequence =
                        Enumerable.Repeat(belowValue.First(), internalDimensionList.Count);

                    if (cond)
                    {
                        int count = 0;
                        TransactionManager.Instance.EnsureInTransaction(doc);
                        while (count < internalDimensionList.Count)
                        {
                            internalDimensionList[count].Below = repeatedSequence.ElementAt(count);
                            count++;
                        }

                        TransactionManager.Instance.TransactionTaskDone();
                        set.Add(dimension);
                    }
                    else
                    {
                        int count = 0;
                        TransactionManager.Instance.EnsureInTransaction(doc);
                        while (count < internalDimensionList.Count)
                        {
                            internalDimensionList[count].Below = belowValue[count];
                            count++;
                        }

                        TransactionManager.Instance.TransactionTaskDone();
                        set.Add(dimension);
                    }
                }
            }
            catch (Exception)
            {
                notSet.Add(dimension);
            }

            //returns the outputs
            var outInfo = new Dictionary<string, object>
            {
                {"set", set},
                {"notSet", notSet}
            };
            return outInfo;
        }

        /// <summary>
        /// This node will retrieve the reference elements of the dimension. Will not work with multi segment dimensions.
        /// </summary>
        /// <param name="dimension">Dimension to get elements from.</param>
        /// <returns name="referenceElements">The reference elements.</returns>
        /// <search>
        /// dimension.GetReferenceElements
        /// </search>
        [NodeCategory("Query")]
        public static List<global::Revit.Elements.Element> GetReferenceElements(
            global::Revit.Elements.Dimension dimension)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //convert to internal dimension
            Autodesk.Revit.DB.Dimension internalDimension = (Autodesk.Revit.DB.Dimension) dimension.InternalElement;
            //lists to output success or not success
            List<global::Revit.Elements.Element> referenceElements = new List<global::Revit.Elements.Element>();
            try
            {
                ReferenceArray refs = new ReferenceArray();
                refs = internalDimension.References;
                foreach (Reference r in refs)
                {
                    referenceElements.Add(doc.GetElement(r.ElementId).ToDSType(false));
                }
            }
            catch (Exception)
            {
                referenceElements.Add(null);
            }

            return referenceElements;
        }

        /// <summary>
        /// *BETA* This node will center the dimension's text on the line.
        /// </summary>
        /// <param name="dimension">The dimension to center text on line for.</param>
        /// <returns name="dimension">The dimension.</returns>
        /// <search>
        /// Dimension.CenterTextOnLine
        /// </search>
        [NodeCategory("Actions")]
        public static void CenterTextOnLine(global::Revit.Elements.Dimension dimension)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            Autodesk.Revit.DB.Dimension internalDimension = (Autodesk.Revit.DB.Dimension) dimension.InternalElement;
            Autodesk.Revit.DB.View ownerView = (Autodesk.Revit.DB.View) doc.GetElement(internalDimension.OwnerViewId);
            //calculate offset distance from the view scale and the text size.
            int viewScaleFactor = ownerView.Scale;
            double textHeight =
                (double) internalDimension.DimensionType.get_Parameter(BuiltInParameter.TEXT_SIZE).AsDouble();
            double offsetAmount = ((textHeight + 0.002604) / 2) * viewScaleFactor;
            List<Line> dimCurve = GetCurve(dimension);

            if (internalDimension.NumberOfSegments > 1)
            {
                for (int i = 0; i < internalDimension.NumberOfSegments; i++)
                {
                    Autodesk.Revit.DB.Line dimLine = dimCurve[i].ToRevitType() as Autodesk.Revit.DB.Line;
                    var currentSegment = internalDimension.Segments.get_Item(i);

                    XYZ point = dimLine.Evaluate(0.5, true);

                    XYZ normal = dimLine.Direction.Normalize();
                    XYZ dir = new XYZ(0, 0, 1);
                    XYZ cross = normal.CrossProduct(dir);
                    if (cross.Y > 0)
                    {
                        offsetAmount = -offsetAmount;
                    }

                    XYZ pointEnd = point + (offsetAmount) * cross;

                    TransactionManager.Instance.EnsureInTransaction(doc);
                    currentSegment.TextPosition = pointEnd;
                    TransactionManager.Instance.TransactionTaskDone();
                }
            }
            else
            {
                Autodesk.Revit.DB.Line singleLine = dimCurve.First().ToRevitType() as Autodesk.Revit.DB.Line;
                XYZ point = singleLine.Evaluate(0.5, true);

                XYZ normal = singleLine.Direction.Normalize();
                XYZ dir = new XYZ(0, 0, 1);
                XYZ cross = normal.CrossProduct(dir);
                if (cross.Y > 0)
                {
                    offsetAmount = -offsetAmount;
                }

                XYZ pointEnd = point + (offsetAmount) * cross;

                TransactionManager.Instance.EnsureInTransaction(doc);
                internalDimension.TextPosition = pointEnd;
                TransactionManager.Instance.TransactionTaskDone();
            }
        }

        /// <summary>
        /// This node will try to set the text location for the given dimensions. This will work for either single segment dimensions or all segments of a multi-segment dimension.
        /// </summary>
        /// <param name="dimension">Dimension to set</param>
        /// <param name="locationPoint">Value to set.</param>
        /// <search>
        /// dimension.SetTextLocation
        /// </search>
        [NodeCategory("Actions")]
        public static void SetTextLocation(global::Revit.Elements.Dimension dimension,
            List<Autodesk.DesignScript.Geometry.Point> locationPoint)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //convert to internal dimension
            Autodesk.Revit.DB.Dimension internalDimension = (Autodesk.Revit.DB.Dimension) dimension.InternalElement;
            List<Autodesk.Revit.DB.Dimension> internalDimensionList =
                new List<Autodesk.Revit.DB.Dimension> {internalDimension};

            if (internalDimension.NumberOfSegments > 1) //set the multi segment ones
            {
                List<Autodesk.Revit.DB.DimensionSegment> dimSegments = new List<Autodesk.Revit.DB.DimensionSegment>();
                foreach (Autodesk.Revit.DB.DimensionSegment seg in internalDimension.Segments)
                {
                    dimSegments.Add(seg);
                }

                //check if lists are even
                bool cond = locationPoint.Count < 2 && dimSegments.Count > 1;
                IEnumerable<Autodesk.DesignScript.Geometry.Point> repeatedSequence =
                    Enumerable.Repeat(locationPoint.First(), dimSegments.Count);
                if (cond)
                {
                    int count = 0;
                    TransactionManager.Instance.EnsureInTransaction(doc);
                    while (count < dimSegments.Count)
                    {
                        dimSegments[count].TextPosition = repeatedSequence.ElementAt(count).ToXyz();
                        count++;
                    }

                    TransactionManager.Instance.TransactionTaskDone();
                }
                else
                {
                    int count = 0;
                    TransactionManager.Instance.EnsureInTransaction(doc);
                    while (count < dimSegments.Count)
                    {
                        dimSegments[count].TextPosition = locationPoint[count].ToXyz();
                        count++;
                    }

                    TransactionManager.Instance.TransactionTaskDone();
                }
            }
            else // set the single ones
            {
                //check if lists are even
                bool cond = locationPoint.Count < 2 && internalDimensionList.Count > 1;
                IEnumerable<Autodesk.DesignScript.Geometry.Point> repeatedSequence =
                    Enumerable.Repeat(locationPoint.First(), internalDimensionList.Count);

                if (cond)
                {
                    int count = 0;
                    TransactionManager.Instance.EnsureInTransaction(doc);
                    while (count < internalDimensionList.Count)
                    {
                        internalDimensionList[count].TextPosition = repeatedSequence.ElementAt(count).ToXyz();
                        count++;
                    }

                    TransactionManager.Instance.TransactionTaskDone();
                }
                else
                {
                    int count = 0;
                    TransactionManager.Instance.EnsureInTransaction(doc);
                    while (count < internalDimensionList.Count)
                    {
                        internalDimensionList[count].TextPosition = locationPoint[count].ToXyz();
                        count++;
                    }

                    TransactionManager.Instance.TransactionTaskDone();
                }
            }
        }

        /// <summary>
        /// This node will return the number of segments comprising the multi segment dimension.
        /// </summary>
        /// <param name="dimension">Multi segment dimension.</param>
        /// <returns name="numberOfSegments">The amount of segments.</returns>
        /// <search>
        /// dimension.Segments
        /// </search>
        [NodeCategory("Query")]
        public static List<int> NumberOfSegments(List<global::Revit.Elements.Dimension> dimension)
        {
            List<int> numberOfSegments = new List<int>(dimension
                .Select(d => ((Autodesk.Revit.DB.Dimension) d.InternalElement).NumberOfSegments).ToArray());

            return numberOfSegments;
        }

        /// <summary>
        /// This node will return the origin of the dimension. If it is a multi-segment dimension it will output all of the pieces.
        /// </summary>
        /// <param name="dimension">The dimension.</param>
        /// <returns name="origin">The dimension origin.</returns>
        /// <search>
        /// dimension.Origin
        /// </search>
        [NodeCategory("Query")]
        public static List<Point> Origin(global::Revit.Elements.Dimension dimension)
        {
            //List<Point> origin = new List<Point>(dimension.Select(d => ((Autodesk.Revit.DB.Dimension)d.InternalElement).Origin.ToPoint()).ToArray());

            Autodesk.Revit.DB.Dimension internalDimension = dimension.InternalElement as Autodesk.Revit.DB.Dimension;
            List<Point> values = new List<Point>();

            if (internalDimension.NumberOfSegments == 0)
            {
                values.Add(internalDimension.Origin.ToPoint(true));
            }
            else
            {
                foreach (Autodesk.Revit.DB.DimensionSegment segment in internalDimension.Segments)
                {
                    values.Add(segment.Origin.ToPoint(true));
                }
            }

            return values;
        }

        /// <summary>
        /// This node will return the text position of the dimension. If it is a multi-segment dimension it will output all of the pieces.
        /// </summary>
        /// <param name="dimension">The dimension.</param>
        /// <returns name="texPosition">The dimension text position.</returns>
        /// <search>
        /// dimension.TextPosition
        /// </search>
        [NodeCategory("Query")]
        public static List<Point> TextPosition(global::Revit.Elements.Dimension dimension)
        {
            //List<Point> textPosition = new List<Point>(dimension.Select(d => ((Autodesk.Revit.DB.Dimension)d.InternalElement).TextPosition.ToPoint()).ToArray());
            Autodesk.Revit.DB.Dimension internalDimension = dimension.InternalElement as Autodesk.Revit.DB.Dimension;
            List<Point> values = new List<Point>();

            if (internalDimension.NumberOfSegments == 0)
            {
                values.Add(internalDimension.TextPosition.ToPoint(true));
            }
            else
            {
                foreach (Autodesk.Revit.DB.DimensionSegment segment in internalDimension.Segments)
                {
                    values.Add(segment.TextPosition.ToPoint(true));
                }
            }

            return values;
        }

        /// <summary>
        /// This node will return the value (string) of the dimension. If the dimension is a multi-segment dimension, this will find all of the above values.
        /// This method returns what the dimension would be in it's non-rounded form. If you want the actual displayed string use Dimension.DisplayValueString in Rhythm.
        /// </summary>
        /// <param name="dimension">The dimension to obtain value from.</param>
        /// <returns name="valueString">The dimension value as a string.</returns>
        /// <search>
        /// dimension.TextPosition
        /// </search>
        [NodeCategory("Query")]
        public static List<string> ValueString(global::Revit.Elements.Dimension dimension)
        {
            Autodesk.Revit.DB.Dimension internalDimension = dimension.InternalElement as Autodesk.Revit.DB.Dimension;
            List<string> values = new List<string>();
            //List<string> valueString = new List<string>(dimension.Select(d => ((Autodesk.Revit.DB.Dimension)d.InternalElement).ValueString).ToArray());

            if (internalDimension.NumberOfSegments == 0)
            {
                values.Add(internalDimension.ValueString);
            }
            else
            {
                foreach (Autodesk.Revit.DB.DimensionSegment segment in internalDimension.Segments)
                {
                    values.Add(segment.ValueString);
                }
            }

            return values;
        }

        /// <summary>
        /// Retrieve the dimension above value. If the dimension is a multi-segment dimension, this will find all of the above values.
        /// </summary>
        /// <param name="dimension">The dimension to retrieve values from.</param>
        /// <returns name="aboveValue">The above value for the dimension.</returns>
        [NodeCategory("Query")]
        public static List<string> AboveValue(global::Revit.Elements.Dimension dimension)
        {
            Autodesk.Revit.DB.Dimension internalDimension = dimension.InternalElement as Autodesk.Revit.DB.Dimension;
            List<string> values = new List<string>();

            if (internalDimension.NumberOfSegments == 0)
            {
                values.Add(internalDimension.Above);
            }
            else
            {
                foreach (Autodesk.Revit.DB.DimensionSegment segment in internalDimension.Segments)
                {
                    values.Add(segment.Above);
                }
            }

            return values;
        }

        /// <summary>
        /// Retrieve the dimension below value. If the dimension is a multi-segment dimension, this will find all of the below values.
        /// </summary>
        /// <param name="dimension">The dimension to retrieve values from.</param>
        /// <returns name="belowValue">The below value for the dimension.</returns>
        [NodeCategory("Query")]
        public static List<string> BelowValue(global::Revit.Elements.Dimension dimension)
        {
            Autodesk.Revit.DB.Dimension internalDimension = dimension.InternalElement as Autodesk.Revit.DB.Dimension;
            List<string> values = new List<string>();

            if (internalDimension.NumberOfSegments == 0)
            {
                values.Add(internalDimension.Below);
            }
            else
            {
                foreach (Autodesk.Revit.DB.DimensionSegment segment in internalDimension.Segments)
                {
                    values.Add(segment.Below);
                }
            }

            return values;
        }


        

#if R20
#else
/// <summary>
        /// Retrieve the actual dimension display value. The built in RevitAPI method returns the string per the project setting. This returns it per the dimension setting.
        /// </summary>
        /// <param name="dimension">The dimension to retrieve values from.</param>
        /// <returns name="realValueString">The real string value for the dimension.</returns>
        [NodeCategory("Query")]
        public static List<string> DisplayValueString(global::Revit.Elements.Dimension dimension)
        {
            //the current document
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //list to append values to
            List<string> values = new List<string>();
            //convert the dimension to the Revit DB Version
            Autodesk.Revit.DB.Dimension internalDimension = dimension.InternalElement as Autodesk.Revit.DB.Dimension;

            if (internalDimension.DimensionType.GetUnitsFormatOptions().UseDefault)
            {
                if (internalDimension.NumberOfSegments == 0)
                {
                    values.Add(internalDimension.ValueString);
                }
                else
                {
                    foreach (Autodesk.Revit.DB.DimensionSegment segment in internalDimension.Segments)
                    {
                        values.Add(segment.ValueString);
                    }
                }
            }
            else
            {
                TransactionManager.Instance.EnsureInTransaction(doc);
                Units ogUnits = doc.GetUnits();
                Units newUnits = new Units(UnitSystem.Imperial);


                SetFormat(dimension, newUnits);
                //set the document units and commit the change
                doc.SetUnits(newUnits);

                //if else statement to work for both single segment or multi segment dimensions.
                if (internalDimension.NumberOfSegments == 0)
                {
                    values.Add(internalDimension.ValueString);
                }
                else
                {
                    foreach (Autodesk.Revit.DB.DimensionSegment segment in internalDimension.Segments)
                    {
                        values.Add(segment.ValueString);
                    }
                }

                doc.SetUnits(ogUnits);
                TransactionManager.Instance.TransactionTaskDone();
            }

            //return the values you retrieved.
            return values;
        }
        public static void SetFormat(global::Revit.Elements.Dimension dimension, Units units)
        {
            Dimension internalDimension = dimension.InternalElement as Dimension;


            ForgeTypeId typeId = new ForgeTypeId("autodesk.spec.aec:length-2.0.0");

            units.SetFormatOptions(typeId,
                internalDimension.DimensionType.GetUnitsFormatOptions());
        }

#endif

        //private static void SetFormatInternal(Units units, Autodesk.Revit.DB.Dimension internalDimension)
        //{
        //    units.SetFormatOptions(internalDimension.DimensionType.UnitType,
        //        internalDimension.DimensionType.GetUnitsFormatOptions());
        //}
    }
}
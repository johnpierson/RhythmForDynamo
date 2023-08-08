using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Curve = Autodesk.DesignScript.Geometry.Curve;
using FilledRegion = Autodesk.Revit.DB.FilledRegion;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for filled regions
    /// </summary>
    public class FilledRegions
    {
        private FilledRegions() { }

        /// <summary>
        /// This will create a filled region with multiple loops. Based on code from https://forum.dynamobim.com/t/filled-region-with-hole-in-the-middle-like-a-donut/22838/3
        /// </summary>
        /// <param name="filledRegionType"></param>
        /// <param name="view"></param>
        /// <param name="curvesToUse"></param>
        /// <returns></returns>
        [NodeCategory("Create")]
        public static global::Revit.Elements.Element ByMultipleLoops(global::Revit.Elements.FilledRegionType filledRegionType, global::Revit.Elements.Views.View view, List<List<Curve>> curvesToUse)
        {
            //the current document
            Document doc = DocumentManager.Instance.CurrentDBDocument;

            //boundary loop to append nested loops to
            List<CurveLoop> boundariesLoop = new List<CurveLoop>();
            
            //iterate through the loops and create curve loops
            foreach (var curveList in curvesToUse)
            {
                SortCurves(curveList);
                List<Autodesk.Revit.DB.Curve> curves = new List<Autodesk.Revit.DB.Curve>();
                foreach (var curve in curveList)
                {
                    curves.Add(curve.ToRevitType(true));
                }
                boundariesLoop.Add(CurveLoop.Create(curves));
            }

            //build the filled region
            TransactionManager.Instance.EnsureInTransaction(doc);
            var filledRegion = FilledRegion.Create(doc, filledRegionType.InternalElement.Id, view.InternalElement.Id, boundariesLoop);
            TransactionManager.Instance.TransactionTaskDone();

            //return the newly created revit element
            return filledRegion.ToDSType(false);
        }
        // this sort curves algorithm is from https://thebuildingcoder.typepad.com/blog/2013/03/sort-and-orient-curves-to-form-a-contiguous-loop.html
        private static void SortCurves(List<Curve> curves)
        {
            double _inch = 1.0 / 12.0;
            double sixteenth = _inch / 16.0;

            int n = curves.Count;

            // Walk through each curve (after the first) 
            // to match up the curves in order

            for (int i = 0; i < n; ++i)
            {
                Curve curve = curves[i];
                XYZ endPoint = curve.EndPoint.ToXyz();

                XYZ p;

                // Find curve with start point = end point

                for (int j = i + 1; j < n; ++j)
                {
                    p = curves[j].StartPoint.ToXyz();

                    // If there is a match end->start, 
                    // this is the next curve

                    if (sixteenth > p.DistanceTo(endPoint))
                    {
                        if (i + 1 != j)
                        {
                            (curves[i + 1], curves[j]) = (curves[j], curves[i + 1]);
                        }

                        break;
                    }

                    p = curves[j].EndPoint.ToXyz();

                    // If there is a match end->end, 
                    // reverse the next curve

                    if (sixteenth > p.DistanceTo(endPoint))
                    {
                        if (i + 1 == j)
                        {


                            curves[i + 1] = CreateReversed(curves[j]);
                        }
                        else
                        {
                            Curve tmp = curves[i + 1];
                            curves[i + 1] = CreateReversed(curves[j]);
                            curves[j] = tmp;
                        }

                        break;
                    }
                }

            }
        }

        private static Curve CreateReversed(Curve curve)
        {
            Autodesk.Revit.DB.Curve orig = curve.ToRevitType();
            if (orig is Line)
            {
                var line = Line.CreateBound(
                    orig.GetEndPoint(1),
                    orig.GetEndPoint(0));
                return line.ToProtoType();
            }
            else if (orig is Arc)
            {
                var arc = Arc.Create(orig.GetEndPoint(1),
                    orig.GetEndPoint(0),
                    orig.Evaluate(0.5, true));
                return arc.ToProtoType();
            }
            else
            {
                throw new Exception(
                    "CreateReversedCurve - Unreachable");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Nuclex.Game.Packing;

namespace Rhythm.GenerativeDesign
{
    /// <summary>
    /// Wrapper class for generative design
    /// </summary>
    public class GenerativeDesign
    {
        private GenerativeDesign()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <param name="viewportRectangles"></param>
        /// <param name="viewportIds"></param>
        /// <returns name="viewportsThatFit">The viewports that fit in the titleblock container</returns>
        /// <returns name="proposedLocations">The proposed locations</returns>
        /// <returns name="viewportRectangles">The rectangles</returns>
        [MultiReturn(new[] { "viewportsThatFit", "proposedLocations", "viewportRectangles" })]
        public static Dictionary<string, object> PackViewports(Rectangle container, List<Rectangle> viewportRectangles, List<int> viewportIds)
        {
            List<int> viewportsThatFit = new List<int>();
            List<Point> points = new List<Point>();
            List<Rectangle> rects = new List<Rectangle>();

            var containerBbox = container.BoundingBox;
            var containerWidth = containerBbox.MaxPoint.X - containerBbox.MinPoint.X;
            var containerHeight = containerBbox.MaxPoint.X - containerBbox.MinPoint.X;

            CygonRectanglePacker packer = new CygonRectanglePacker(containerWidth, containerHeight);

            UV placement = null;
            for (int i = 0; i < viewportRectangles.Count; i++)
            {
                var current = viewportRectangles[i];


                List<Point> geoPoints = new List<Point>();
                
                var bBox = BoundingBox.ByGeometry(current);

                var height = bBox.MaxPoint.Y - bBox.MinPoint.Y;
                var width = bBox.MaxPoint.X - bBox.MinPoint.X;

                if (packer.TryPack(width, height, out placement))
                {
                    var bottomLeft = containerBbox.MinPoint;
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
                    viewportsThatFit.Add(viewportIds[i]);
                    rects.Add(current);

                    points.Add(Point.ByCoordinates(placement.U + bottomLeftX + width / 2, placement.V + bottomLeftY + height / 2, 0));
                }
            }
            //returns the outputs
            var outInfo = new Dictionary<string, object>
            {
                {"viewportsThatFit", viewportsThatFit},
                { "proposedLocations",points},
                { "viewportRectangles",rects}
            };
            return outInfo;
        }
    }
}

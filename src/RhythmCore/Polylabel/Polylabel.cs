using System;
using System.Collections.Generic;
using System.Linq;

namespace Rhythm.Polylabel
{
    //made possible thanks to https://github.com/eqmiller/polylabel-csharp
    public class Polylabel
    {
        private Polylabel()
        {
        }

        /// <summary>
        /// A fast algorithm for finding polygon pole of inaccessibility, the most distant
        /// internal point from the polygon outline (not to be confused with centroid).
        /// Useful for optimal placement of a text label on a polygon.
        /// </summary>
        /// <param name="polygon">GeoJson like format</param>
        /// <param name="precision"></param>
        /// <param name="debug"></param>
        /// <returns></returns>
        internal static double[] GetPolylabel(double[][][] polygon, double precision = 1.0, bool debug = false)
        {
            //get bounding box of the outer ring
            var minX = polygon[0].Min(x => x[0]);
            var minY = polygon[0].Min(x => x[1]);
            var maxX = polygon[0].Max(x => x[0]);
            var maxY = polygon[0].Max(x => x[1]);

            var width = maxX - minX;
            var height = maxY - minY;
            var cellSize = global::System.Math.Min(width, height);
            var h = cellSize / 2;

            if (cellSize == 0) return new double[] { minX, minY };

            //a priority queue of cells in order of their "potential" (max distance to polygon)
            var cellQueue = new Queue<Cell>();

            //cover polygon with initial cells
            for (var x = minX; x < maxX; x += cellSize)
            {
                for (var y = minY; y < maxY; y += cellSize)
                {
                    cellQueue.Enqueue(new Cell(x + h, y + h, h, polygon));
                }
            }

            //take centroid as the first best guess
            var bestCell = GetCentroidCell(polygon);

            //special case for rectangular polygons
            var bboxCell = new Cell(minX + width / 2, minY + height / 2, 0, polygon);
            if (bboxCell.D > bestCell.D) bestCell = bboxCell;

            var numProbes = cellQueue.Count;

            while (cellQueue.Count > 0)
            {
                //pick the most promising cell from the queue
                var cell = cellQueue.Dequeue();

                //update the best cell if we found a better one
                if (cell.D > bestCell.D)
                {
                    bestCell = cell;
                    //if (debug) Console.WriteLine($"found best {global::System.Math.Round(1e4 * cell.D) / 1e4} after {numProbes}");
                }

                //do not drill down further if there's no chance of a better solution
                if (cell.Max - bestCell.D <= precision) continue;

                //split the cell into four cells
                h = cell.H / 2;
                cellQueue.Enqueue(new Cell(cell.X - h, cell.Y - h, h, polygon));
                cellQueue.Enqueue(new Cell(cell.X + h, cell.Y - h, h, polygon));
                cellQueue.Enqueue(new Cell(cell.X - h, cell.Y + h, h, polygon));
                cellQueue.Enqueue(new Cell(cell.X + h, cell.Y + h, h, polygon));
                numProbes += 4;
            }

            if (debug)
            {
                Console.WriteLine($"Number probes: {numProbes}");
                Console.WriteLine($"Best distance: {bestCell.D}");
            }

            return new double[] { bestCell.X, bestCell.Y };
        }

        internal static Cell GetCentroidCell(double[][][] polygon)
        {
            var area = 0.0;
            var x = 0.0;
            var y = 0.0;
            var points = polygon[0];

            var len = points.Length;
            var j = len - 1;
            for (var i = 0; i < len; j = i++)
            {
                var a = points[i];
                var b = points[j];
                var f = a[0] * b[1] - b[0] * a[1];
                x += (a[0] + b[0]) * f;
                y += (a[1] + b[1]) * f;
                area += f * 3;
            }
            if (area == 0) return new Cell(points[0][0], points[0][1], 0, polygon);
            return new Cell(x / area, y / area, 0, polygon);
        }
    }
}
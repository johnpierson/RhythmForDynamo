using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using SysBitmap = global::System.Drawing.Bitmap;
using SysImageLockMode = global::System.Drawing.Imaging.ImageLockMode;
using SysPixelFormat = global::System.Drawing.Imaging.PixelFormat;
using SysRectangle = global::System.Drawing.Rectangle;
using ImagePoint = global::System.Drawing.Point;
using SMath = global::System.Math;

namespace Rhythm.Geometry
{
    /// <summary>
    /// Provides floor plan detection from raster images using image processing techniques.
    /// </summary>
    public class FloorPlan
    {
        private FloorPlan() { }

        /// <summary>
        /// Detects architectural floor plan geometry from a JPG image.
        /// Converts raster floor plan images into Dynamo-native geometry including
        /// lines, room boundaries, and opening locations.
        /// </summary>
        /// <param name="imagePath">Path to the JPG image file.</param>
        /// <param name="scale">Pixel-to-model scale factor (default: 1.0).</param>
        /// <param name="threshold">Threshold value for binarization, -1 for automatic (default: -1).</param>
        /// <param name="adaptiveThreshold">Use adaptive thresholding (default: true).</param>
        /// <param name="minLineLength">Minimum detectable line length in pixels (default: 25).</param>
        /// <param name="wallThicknessRange">Optional expected wall thickness range [min, max] in pixels.</param>
        /// <param name="detectRooms">Attempt room/space contour detection (default: true).</param>
        /// <param name="detectOpenings">Attempt opening detection (default: true).</param>
        /// <param name="debugOutput">Return additional debug metadata (default: false).</param>
        /// <returns name="lines">Detected linework as Dynamo Line geometry.</returns>
        /// <returns name="roomBoundaries">Detected room boundary candidates as PolyCurve geometry.</returns>
        /// <returns name="openingLocations">Detected opening locations as Point geometry.</returns>
        /// <returns name="report">Processing report with metadata, warnings, and statistics.</returns>
        [MultiReturn(new[] { "lines", "roomBoundaries", "openingLocations", "report" })]
        public static Dictionary<string, object> DetectFloorPlanFromImage(
            string imagePath,
            double scale = 1.0,
            int threshold = -1,
            bool adaptiveThreshold = true,
            double minLineLength = 25,
            double[] wallThicknessRange = null,
            bool detectRooms = true,
            bool detectOpenings = true,
            bool debugOutput = false)
        {
            var stopwatch = Stopwatch.StartNew();
            var warnings = new List<string>();
            var lines = new List<Autodesk.DesignScript.Geometry.Line>();
            var roomBoundaries = new List<Autodesk.DesignScript.Geometry.PolyCurve>();
            var openingLocations = new List<Autodesk.DesignScript.Geometry.Point>();
            var report = new Dictionary<string, object>();

            int actualThreshold = threshold;
            int width = 0;
            int height = 0;

            try
            {
                // --- Validate inputs ---
                if (string.IsNullOrEmpty(imagePath))
                    throw new ArgumentNullException(nameof(imagePath), "Image path must not be null or empty.");

                if (!File.Exists(imagePath))
                    throw new FileNotFoundException($"Image file not found: {imagePath}", imagePath);

                string ext = Path.GetExtension(imagePath).ToLowerInvariant();
                if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".bmp" && ext != ".tiff" && ext != ".tif")
                    warnings.Add($"Unsupported image format '{ext}'. Best results are obtained with JPG/PNG images.");

                if (scale <= 0)
                {
                    warnings.Add("Scale must be positive. Defaulting to 1.0.");
                    scale = 1.0;
                }

                // --- Load image ---
                SysBitmap bitmap;
                try
                {
                    bitmap = new SysBitmap(imagePath);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Cannot decode image '{imagePath}': {ex.Message}");
                }

                width = bitmap.Width;
                height = bitmap.Height;

                if (width < 50 || height < 50)
                {
                    bitmap.Dispose();
                    warnings.Add($"Image resolution ({width}x{height}) is too small for reliable detection.");
                    stopwatch.Stop();
                    report["imageWidth"] = width;
                    report["imageHeight"] = height;
                    report["processingTimeMs"] = stopwatch.ElapsedMilliseconds;
                    report["warnings"] = warnings;
                    return new Dictionary<string, object>
                    {
                        { "lines", lines },
                        { "roomBoundaries", roomBoundaries },
                        { "openingLocations", openingLocations },
                        { "report", report }
                    };
                }

                if (width < 200 || height < 200)
                    warnings.Add($"Image resolution ({width}x{height}) is low. Detection quality may be reduced.");

                // --- Extract pixel data ---
                byte[] argbPixels = ExtractArgbPixels(bitmap);
                bitmap.Dispose();

                // --- Convert to grayscale ---
                byte[] gray = ConvertToGrayscale(argbPixels, width, height);

                // --- Apply Gaussian blur (denoising) ---
                byte[] blurred = ApplyGaussianBlur(gray, width, height);

                // --- Apply thresholding ---
                byte[] binary;
                if (threshold == -1 && adaptiveThreshold)
                {
                    binary = ApplyAdaptiveThreshold(blurred, width, height);
                    actualThreshold = -1;
                }
                else
                {
                    if (threshold == -1)
                        actualThreshold = ComputeOtsuThreshold(blurred);
                    binary = ApplyThreshold(blurred, actualThreshold);
                }

                // --- Morphological cleanup ---
                byte[] cleaned = ApplyMorphologyCleanup(binary, width, height);

                // --- Edge detection ---
                byte[] edges = DetectEdges(cleaned, width, height);

                // --- Line detection via Hough transform ---
                var detectedSegments = DetectLineSegments(edges, cleaned, width, height, minLineLength);

                if (detectedSegments.Count == 0)
                    warnings.Add("No significant linework detected. Check image quality and threshold settings.");
                else if (detectedSegments.Count < 5)
                    warnings.Add("Very few lines detected. This image may not be a floor plan.");

                // --- Convert segments to Dynamo Line geometry ---
                foreach (var seg in detectedSegments)
                {
                    var startPt = Autodesk.DesignScript.Geometry.Point.ByCoordinates(seg.X1 * scale, -seg.Y1 * scale);
                    var endPt = Autodesk.DesignScript.Geometry.Point.ByCoordinates(seg.X2 * scale, -seg.Y2 * scale);
                    try
                    {
                        if (startPt.DistanceTo(endPt) > 1e-6)
                        {
                            var line = Autodesk.DesignScript.Geometry.Line.ByStartPointEndPoint(startPt, endPt);
                            lines.Add(line);
                        }
                    }
                    catch
                    {
                        // Skip degenerate lines
                    }
                    finally
                    {
                        startPt.Dispose();
                        endPt.Dispose();
                    }
                }

                // --- Room / contour detection ---
                if (detectRooms)
                {
                    var contours = DetectRoomContours(cleaned, width, height);
                    foreach (var contour in contours)
                    {
                        if (contour.Count < 3) continue;
                        var pts = contour
                            .Select(p => Autodesk.DesignScript.Geometry.Point.ByCoordinates(p.X * scale, -p.Y * scale))
                            .ToList();
                        try
                        {
                            var polyCurve = Autodesk.DesignScript.Geometry.PolyCurve.ByPoints(pts, true);
                            roomBoundaries.Add(polyCurve);
                        }
                        catch
                        {
                            // Skip malformed contours
                        }
                        finally
                        {
                            foreach (var pt in pts) pt.Dispose();
                        }
                    }
                }

                // --- Opening detection ---
                if (detectOpenings)
                {
                    var openings = FindOpenings(cleaned, width, height);
                    foreach (var op in openings)
                    {
                        openingLocations.Add(
                            Autodesk.DesignScript.Geometry.Point.ByCoordinates(op.X * scale, -op.Y * scale));
                    }
                }

                if (lines.Count == 0 && roomBoundaries.Count == 0)
                    warnings.Add("No geometry was detected. The image may not contain recognizable floor plan linework.");

                stopwatch.Stop();

                // --- Build report ---
                report["imageWidth"] = width;
                report["imageHeight"] = height;
                report["scale"] = scale;
                report["thresholdUsed"] = actualThreshold == -1 ? "adaptive" : (object)actualThreshold;
                report["adaptiveThreshold"] = adaptiveThreshold;
                report["minLineLength"] = minLineLength;
                report["linesDetected"] = detectedSegments.Count;
                report["roomsDetected"] = roomBoundaries.Count;
                report["openingsDetected"] = openingLocations.Count;
                report["processingTimeMs"] = stopwatch.ElapsedMilliseconds;
                report["warnings"] = warnings;

                if (debugOutput)
                {
                    report["debugEdgePixelCount"] = edges.Count(b => b > 50);
                    report["debugBinaryDarkPixelCount"] = cleaned.Count(b => b <= 128);
                    report["debugImagePixelTotal"] = width * height;
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                warnings.Add($"Error processing image: {ex.Message}");
                report["imageWidth"] = width;
                report["imageHeight"] = height;
                report["processingTimeMs"] = stopwatch.ElapsedMilliseconds;
                report["warnings"] = warnings;
            }

            return new Dictionary<string, object>
            {
                { "lines", lines },
                { "roomBoundaries", roomBoundaries },
                { "openingLocations", openingLocations },
                { "report", report }
            };
        }

        // -----------------------------------------------------------------------
        // Image loading helpers
        // -----------------------------------------------------------------------

        private static byte[] ExtractArgbPixels(SysBitmap bitmap)
        {
            var bmpData = bitmap.LockBits(
                new SysRectangle(0, 0, bitmap.Width, bitmap.Height),
                SysImageLockMode.ReadOnly,
                SysPixelFormat.Format32bppArgb);
            int size = SMath.Abs(bmpData.Stride) * bitmap.Height;
            byte[] data = new byte[size];
            Marshal.Copy(bmpData.Scan0, data, 0, size);
            bitmap.UnlockBits(bmpData);
            return data;
        }

        // -----------------------------------------------------------------------
        // Grayscale conversion
        // -----------------------------------------------------------------------

        private static byte[] ConvertToGrayscale(byte[] argbPixels, int width, int height)
        {
            byte[] gray = new byte[width * height];
            // LockBits with Format32bppArgb gives BGRA byte order in memory.
            // Use integer arithmetic (fixed-point) for performance: coefficients are
            // r*77 + g*150 + b*29, then >> 8 (equivalent to /256 ≈ BT.601 luminance).
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = (y * width + x) * 4;
                    int b = argbPixels[i];
                    int g = argbPixels[i + 1];
                    int r = argbPixels[i + 2];
                    gray[y * width + x] = (byte)((77 * r + 150 * g + 29 * b) >> 8);
                }
            }
            return gray;
        }

        // -----------------------------------------------------------------------
        // Gaussian blur (5x5, sigma ~1.4)
        // -----------------------------------------------------------------------

        private static readonly int[,] GaussKernel5x5 =
        {
            { 2,  4,  5,  4, 2 },
            { 4,  9, 12,  9, 4 },
            { 5, 12, 15, 12, 5 },
            { 4,  9, 12,  9, 4 },
            { 2,  4,  5,  4, 2 }
        };

        private const int GaussKernelSum = 159;

        private static byte[] ApplyGaussianBlur(byte[] gray, int width, int height)
        {
            byte[] result = new byte[width * height];
            for (int y = 2; y < height - 2; y++)
            {
                for (int x = 2; x < width - 2; x++)
                {
                    int sum = 0;
                    for (int ky = -2; ky <= 2; ky++)
                        for (int kx = -2; kx <= 2; kx++)
                            sum += gray[(y + ky) * width + (x + kx)] * GaussKernel5x5[ky + 2, kx + 2];
                    result[y * width + x] = (byte)(sum / GaussKernelSum);
                }
            }
            // Copy border pixels unchanged
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (y < 2 || y >= height - 2 || x < 2 || x >= width - 2)
                        result[y * width + x] = gray[y * width + x];
                }
            }
            return result;
        }

        // -----------------------------------------------------------------------
        // Otsu's thresholding
        // -----------------------------------------------------------------------

        private static int ComputeOtsuThreshold(byte[] gray)
        {
            int[] hist = new int[256];
            foreach (byte b in gray) hist[b]++;

            int n = gray.Length;
            double sum = 0;
            for (int i = 0; i < 256; i++) sum += i * hist[i];

            double sumB = 0;
            int wB = 0;
            double maxVar = 0;
            int best = 128;
            for (int t = 0; t < 256; t++)
            {
                wB += hist[t];
                if (wB == 0) continue;
                int wF = n - wB;
                if (wF == 0) break;
                sumB += t * hist[t];
                double mB = sumB / wB;
                double mF = (sum - sumB) / wF;
                double var = (double)wB * wF * (mB - mF) * (mB - mF);
                if (var > maxVar) { maxVar = var; best = t; }
            }
            return best;
        }

        private static byte[] ApplyThreshold(byte[] gray, int thresh)
        {
            byte[] binary = new byte[gray.Length];
            for (int i = 0; i < gray.Length; i++)
                binary[i] = gray[i] < thresh ? (byte)0 : (byte)255;
            return binary;
        }

        // -----------------------------------------------------------------------
        // Adaptive threshold (mean-based with integral image)
        // -----------------------------------------------------------------------

        private static byte[] ApplyAdaptiveThreshold(byte[] gray, int width, int height)
        {
            // Block size: at least 11 pixels, scaled to ~1/20th of the shorter image dimension.
            // Using an odd block size is required by the mean-threshold formula.
            const int MinAdaptiveBlockSize = 11;
            const int BlockSizeDivisor = 20;
            int blockSize = SMath.Max(MinAdaptiveBlockSize, (SMath.Min(width, height) / BlockSizeDivisor) | 1);
            if (blockSize % 2 == 0) blockSize++;

            // Bias subtracted from the local mean before comparing.
            // A small positive value (5) keeps faint lines detectable while suppressing noise.
            const int AdaptiveThresholdBias = 5;
            int half = blockSize / 2;

            // Build integral image
            long[] integral = new long[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    long above = y > 0 ? integral[(y - 1) * width + x] : 0L;
                    long left = x > 0 ? integral[y * width + x - 1] : 0L;
                    long diagLeft = (y > 0 && x > 0) ? integral[(y - 1) * width + x - 1] : 0L;
                    integral[y * width + x] = gray[y * width + x] + above + left - diagLeft;
                }
            }

            byte[] result = new byte[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int x1 = SMath.Max(0, x - half);
                    int y1 = SMath.Max(0, y - half);
                    int x2 = SMath.Min(width - 1, x + half);
                    int y2 = SMath.Min(height - 1, y + half);
                    int count = (x2 - x1 + 1) * (y2 - y1 + 1);
                    long s = integral[y2 * width + x2];
                    if (x1 > 0) s -= integral[y2 * width + x1 - 1];
                    if (y1 > 0) s -= integral[(y1 - 1) * width + x2];
                    if (x1 > 0 && y1 > 0) s += integral[(y1 - 1) * width + x1 - 1];
                    double mean = (double)s / count;
                    result[y * width + x] = gray[y * width + x] < (mean - AdaptiveThresholdBias) ? (byte)0 : (byte)255;
                }
            }
            return result;
        }

        // -----------------------------------------------------------------------
        // Morphological operations (cleanup)
        // -----------------------------------------------------------------------

        private static byte[] ApplyMorphologyCleanup(byte[] binary, int width, int height)
        {
            // Opening: erode then dilate removes small noise speckles
            byte[] eroded = MorphErode(binary, width, height, 1);
            byte[] dilated = MorphDilate(eroded, width, height, 1);
            return dilated;
        }

        private static byte[] MorphErode(byte[] binary, int width, int height, int radius)
        {
            byte[] result = new byte[width * height];
            // Default to white (255); pixels that fail erosion stay 255
            for (int i = 0; i < result.Length; i++) result[i] = 255;

            for (int y = radius; y < height - radius; y++)
            {
                for (int x = radius; x < width - radius; x++)
                {
                    bool allDark = true;
                    for (int dy = -radius; dy <= radius && allDark; dy++)
                        for (int dx = -radius; dx <= radius && allDark; dx++)
                            if (binary[(y + dy) * width + (x + dx)] > 128)
                                allDark = false;
                    result[y * width + x] = allDark ? (byte)0 : (byte)255;
                }
            }
            return result;
        }

        private static byte[] MorphDilate(byte[] binary, int width, int height, int radius)
        {
            byte[] result = new byte[width * height];
            for (int i = 0; i < result.Length; i++) result[i] = 255;

            for (int y = radius; y < height - radius; y++)
            {
                for (int x = radius; x < width - radius; x++)
                {
                    bool anyDark = false;
                    for (int dy = -radius; dy <= radius && !anyDark; dy++)
                        for (int dx = -radius; dx <= radius && !anyDark; dx++)
                            if (binary[(y + dy) * width + (x + dx)] <= 128)
                                anyDark = true;
                    result[y * width + x] = anyDark ? (byte)0 : (byte)255;
                }
            }
            return result;
        }

        // -----------------------------------------------------------------------
        // Edge detection (Sobel)
        // -----------------------------------------------------------------------

        private static byte[] DetectEdges(byte[] binary, int width, int height)
        {
            byte[] edges = new byte[width * height];
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int gx =
                        -binary[(y - 1) * width + x - 1] + binary[(y - 1) * width + x + 1]
                        - 2 * binary[y * width + x - 1] + 2 * binary[y * width + x + 1]
                        - binary[(y + 1) * width + x - 1] + binary[(y + 1) * width + x + 1];

                    int gy =
                        binary[(y - 1) * width + x - 1] + 2 * binary[(y - 1) * width + x] + binary[(y - 1) * width + x + 1]
                        - binary[(y + 1) * width + x - 1] - 2 * binary[(y + 1) * width + x] - binary[(y + 1) * width + x + 1];

                    int mag = (int)SMath.Sqrt(gx * gx + gy * gy);
                    edges[y * width + x] = (byte)SMath.Min(255, mag / 2);
                }
            }
            return edges;
        }

        // -----------------------------------------------------------------------
        // Line segment detection (Hough transform + segment extraction)
        // -----------------------------------------------------------------------

        private struct LineSegment
        {
            public int X1, Y1, X2, Y2;
            public LineSegment(int x1, int y1, int x2, int y2) { X1 = x1; Y1 = y1; X2 = x2; Y2 = y2; }
        }

        private static List<LineSegment> DetectLineSegments(
            byte[] edges, byte[] binary, int width, int height, double minLineLength)
        {
            var result = new List<LineSegment>();

            // Downsample large images for the Hough pass to keep it fast
            int factor = 1;
            if (width > 1500 || height > 1500) factor = 2;
            if (width > 3000 || height > 3000) factor = 4;

            int rw = width / factor;
            int rh = height / factor;
            byte[] redEdges = factor == 1 ? edges : DownsampleMax(edges, width, height, factor);

            // Standard Hough accumulator
            int numAngles = 180;
            int diagLen = (int)SMath.Ceiling(SMath.Sqrt(rw * rw + rh * rh)) + 1;
            int numRho = 2 * diagLen + 1;

            int[] accumulator = new int[numAngles * numRho];
            double[] cosT = new double[numAngles];
            double[] sinT = new double[numAngles];
            for (int i = 0; i < numAngles; i++)
            {
                double angle = i * SMath.PI / numAngles;
                cosT[i] = SMath.Cos(angle);
                sinT[i] = SMath.Sin(angle);
            }

            // Vote
            // An edge pixel contributes to an accumulator cell only if its Sobel magnitude
            // exceeds this threshold, filtering low-gradient noise.
            const int EdgeVoteThreshold = 50;
            for (int y = 0; y < rh; y++)
            {
                for (int x = 0; x < rw; x++)
                {
                    if (redEdges[y * rw + x] <= EdgeVoteThreshold) continue;
                    for (int t = 0; t < numAngles; t++)
                    {
                        int rho = (int)(x * cosT[t] + y * sinT[t]) + diagLen;
                        if (rho >= 0 && rho < numRho)
                            accumulator[t * numRho + rho]++;
                    }
                }
            }

            // Find peaks with non-maximum suppression.
            // Minimum vote count is at least 15, or 60% of minLineLength (after downsampling).
            // Peaks with fewer votes than this are too weak to represent real lines.
            const int MinHoughVotes = 15;
            const double VoteRatioOfMinLength = 0.6;
            int voteThresh = SMath.Max(MinHoughVotes, (int)(minLineLength / factor * VoteRatioOfMinLength));
            var peaks = new List<(int t, int rho, int votes)>();
            for (int t = 0; t < numAngles; t++)
            {
                for (int r = 1; r < numRho - 1; r++)
                {
                    int v = accumulator[t * numRho + r];
                    if (v < voteThresh) continue;
                    bool isMax = true;
                    for (int dt = -2; dt <= 2 && isMax; dt++)
                    {
                        for (int dr = -3; dr <= 3 && isMax; dr++)
                        {
                            if (dt == 0 && dr == 0) continue;
                            int nt = (t + dt + numAngles) % numAngles;
                            int nr = r + dr;
                            if (nr >= 0 && nr < numRho && accumulator[nt * numRho + nr] > v)
                                isMax = false;
                        }
                    }
                    if (isMax) peaks.Add((t, r - diagLen, v));
                }
            }

            // Extract line segments from each detected line
            int maxGap = SMath.Max(5, (int)(minLineLength / 5));
            double minLenReduced = minLineLength / factor;

            foreach (var (t, rho, votes) in peaks.OrderByDescending(p => p.votes).Take(300))
            {
                double cosA = cosT[t];
                double sinA = sinT[t];

                // Scan along the line direction
                int lineSpan = (int)SMath.Sqrt(rw * rw + rh * rh) + 2;
                bool inSeg = false;
                int segX0 = 0, segY0 = 0, lastX = 0, lastY = 0;
                int segLen = 0, gapLen = 0;

                for (int s = -lineSpan; s <= lineSpan; s++)
                {
                    int px = (int)SMath.Round(rho * cosA - s * sinA);
                    int py = (int)SMath.Round(rho * sinA + s * cosA);
                    if (px < 0 || px >= rw || py < 0 || py >= rh) continue;

                    bool isEdge = redEdges[py * rw + px] > EdgeVoteThreshold;
                    if (isEdge)
                    {
                        if (!inSeg) { inSeg = true; segX0 = px; segY0 = py; segLen = 0; gapLen = 0; }
                        segLen++;
                        gapLen = 0;
                        lastX = px;
                        lastY = py;
                    }
                    else if (inSeg)
                    {
                        gapLen++;
                        if (gapLen > maxGap)
                        {
                            if (segLen >= minLenReduced)
                                result.Add(new LineSegment(segX0 * factor, segY0 * factor, lastX * factor, lastY * factor));
                            inSeg = false;
                            segLen = 0;
                            gapLen = 0;
                        }
                    }
                }
                if (inSeg && segLen >= minLenReduced)
                    result.Add(new LineSegment(segX0 * factor, segY0 * factor, lastX * factor, lastY * factor));
            }

            return result;
        }

        private static byte[] DownsampleMax(byte[] img, int width, int height, int factor)
        {
            int rw = width / factor;
            int rh = height / factor;
            byte[] result = new byte[rw * rh];
            for (int y = 0; y < rh; y++)
            {
                for (int x = 0; x < rw; x++)
                {
                    int maxVal = 0;
                    for (int dy = 0; dy < factor; dy++)
                    {
                        for (int dx = 0; dx < factor; dx++)
                        {
                            int px = x * factor + dx;
                            int py = y * factor + dy;
                            if (px < width && py < height)
                                maxVal = SMath.Max(maxVal, img[py * width + px]);
                        }
                    }
                    result[y * rw + x] = (byte)maxVal;
                }
            }
            return result;
        }

        // -----------------------------------------------------------------------
        // Room contour detection
        // -----------------------------------------------------------------------

        private static List<List<ImagePoint>> DetectRoomContours(byte[] binary, int width, int height)
        {
            var contours = new List<List<ImagePoint>>();
            bool[] visited = new bool[width * height];

            int minArea = SMath.Max(100, width * height / 1000);
            int maxArea = (int)(width * height * 0.85);

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int idx = y * width + x;
                    if (visited[idx] || binary[idx] <= 128) continue;

                    var region = FloodFillRegion(binary, visited, x, y, width, height, maxArea);
                    if (region == null || region.Count < minArea) continue;

                    var boundary = ExtractBoundary(region, binary, width, height);
                    if (boundary.Count < 3) continue;

                    // RDP epsilon of 5 pixels: merges boundary micro-steps from pixel rasterization
                    // while preserving actual corners of architectural walls and rooms.
                    const double RdpSimplificationTolerance = 5.0;
                    var simplified = RdpSimplify(boundary, RdpSimplificationTolerance);
                    if (simplified.Count >= 3)
                        contours.Add(simplified);
                }
            }
            return contours;
        }

        private static List<ImagePoint> FloodFillRegion(
            byte[] binary, bool[] visited, int startX, int startY, int width, int height, int maxSize)
        {
            var region = new List<ImagePoint>();
            var queue = new Queue<ImagePoint>();
            queue.Enqueue(new ImagePoint(startX, startY));
            visited[startY * width + startX] = true;

            while (queue.Count > 0)
            {
                var p = queue.Dequeue();
                region.Add(p);
                if (region.Count > maxSize) return null; // too large

                int[] dxArr = { 1, -1, 0, 0 };
                int[] dyArr = { 0, 0, 1, -1 };
                for (int d = 0; d < 4; d++)
                {
                    int nx = p.X + dxArr[d];
                    int ny = p.Y + dyArr[d];
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                    int ni = ny * width + nx;
                    if (visited[ni] || binary[ni] <= 128) continue;
                    visited[ni] = true;
                    queue.Enqueue(new ImagePoint(nx, ny));
                }
            }
            return region;
        }

        private static List<ImagePoint> ExtractBoundary(
            List<ImagePoint> region, byte[] binary, int width, int height)
        {
            var regionSet = new HashSet<int>();
            foreach (var p in region) regionSet.Add(p.Y * width + p.X);

            var boundary = new List<ImagePoint>();
            int[] dxArr = { 1, -1, 0, 0, 1, -1, 1, -1 };
            int[] dyArr = { 0, 0, 1, -1, 1, -1, -1, 1 };

            foreach (var p in region)
            {
                bool isBoundary = false;
                for (int d = 0; d < 8 && !isBoundary; d++)
                {
                    int nx = p.X + dxArr[d];
                    int ny = p.Y + dyArr[d];
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height || !regionSet.Contains(ny * width + nx))
                        isBoundary = true;
                }
                if (isBoundary) boundary.Add(p);
            }
            return boundary;
        }

        // Ramer-Douglas-Peucker simplification
        private static List<ImagePoint> RdpSimplify(List<ImagePoint> points, double epsilon)
        {
            if (points.Count <= 2) return new List<ImagePoint>(points);
            return RdpRecursive(points, 0, points.Count - 1, epsilon);
        }

        private static List<ImagePoint> RdpRecursive(
            List<ImagePoint> pts, int start, int end, double epsilon)
        {
            if (start >= end) return new List<ImagePoint> { pts[start] };

            double dx = pts[end].X - pts[start].X;
            double dy = pts[end].Y - pts[start].Y;
            double len = SMath.Sqrt(dx * dx + dy * dy);

            double maxDist = 0;
            int maxIdx = start;
            for (int i = start + 1; i < end; i++)
            {
                double dist = len < 1e-10
                    ? SMath.Sqrt(SMath.Pow(pts[i].X - pts[start].X, 2) + SMath.Pow(pts[i].Y - pts[start].Y, 2))
                    : SMath.Abs(dy * pts[i].X - dx * pts[i].Y + pts[end].X * pts[start].Y - pts[end].Y * pts[start].X) / len;
                if (dist > maxDist) { maxDist = dist; maxIdx = i; }
            }

            if (maxDist > epsilon)
            {
                var left = RdpRecursive(pts, start, maxIdx, epsilon);
                var right = RdpRecursive(pts, maxIdx, end, epsilon);
                var combined = new List<ImagePoint>(left);
                combined.AddRange(right.Skip(1));
                return combined;
            }
            return new List<ImagePoint> { pts[start], pts[end] };
        }

        // -----------------------------------------------------------------------
        // Opening detection (gap analysis in wall runs)
        // -----------------------------------------------------------------------

        private static List<ImagePoint> FindOpenings(byte[] binary, int width, int height)
        {
            var openings = new List<ImagePoint>();
            // Openings (doors/windows) in architectural plans typically span 8-80 pixels
            // at common floor-plan scan resolutions.
            const int MinOpeningGapPixels = 8;
            const int MaxOpeningGapPixels = 80;

            // Scan horizontal runs for gaps in walls
            for (int y = 1; y < height - 1; y++)
            {
                bool wasWall = binary[y * width] <= 128;
                int gapStart = -1;

                for (int x = 1; x < width; x++)
                {
                    bool isWall = binary[y * width + x] <= 128;

                    if (wasWall && !isWall)
                    {
                        gapStart = x;
                    }
                    else if (!isWall && gapStart >= 0)
                    {
                        // still in gap
                    }
                    else if (!wasWall && isWall && gapStart >= 0)
                    {
                        int gapLen = x - gapStart;
                        if (gapLen >= MinOpeningGapPixels && gapLen <= MaxOpeningGapPixels)
                        {
                            // Verify walls exist above or below
                            int cx = (gapStart + x) / 2;
                            bool adjacentWalls =
                                (y > 0 && binary[(y - 1) * width + cx] <= 128) ||
                                (y < height - 1 && binary[(y + 1) * width + cx] <= 128);
                            if (adjacentWalls)
                                openings.Add(new ImagePoint(cx, y));
                        }
                        gapStart = -1;
                    }

                    wasWall = isWall;
                }
            }

            // Scan vertical runs for gaps in walls
            for (int x = 1; x < width - 1; x++)
            {
                bool wasWall = binary[x] <= 128;
                int gapStart = -1;

                for (int y = 1; y < height; y++)
                {
                    bool isWall = binary[y * width + x] <= 128;

                    if (wasWall && !isWall) { gapStart = y; }
                    else if (!wasWall && isWall && gapStart >= 0)
                    {
                        int gapLen = y - gapStart;
                        if (gapLen >= MinOpeningGapPixels && gapLen <= MaxOpeningGapPixels)
                        {
                            int cy = (gapStart + y) / 2;
                            bool adjacentWalls =
                                (x > 0 && binary[cy * width + x - 1] <= 128) ||
                                (x < width - 1 && binary[cy * width + x + 1] <= 128);
                            if (adjacentWalls)
                                openings.Add(new ImagePoint(x, cy));
                        }
                        gapStart = -1;
                    }
                    wasWall = isWall;
                }
            }

            // Merge opening candidates within 25 pixels to avoid duplicate detections
            // at the same physical door/window opening.
            const double OpeningMergeDistance = 25.0;
            return DeduplicatePoints(openings, OpeningMergeDistance);
        }

        private static List<ImagePoint> DeduplicatePoints(
            List<ImagePoint> points, double minDist)
        {
            var result = new List<ImagePoint>();
            double minDistSq = minDist * minDist;
            foreach (var p in points)
            {
                bool tooClose = false;
                foreach (var r in result)
                {
                    double dx = p.X - r.X;
                    double dy = p.Y - r.Y;
                    if (dx * dx + dy * dy < minDistSq) { tooClose = true; break; }
                }
                if (!tooClose) result.Add(p);
            }
            return result;
        }
    }
}

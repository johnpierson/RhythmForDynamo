using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using SystemMath = System.Math;

namespace Rhythm.GenerativeDesign
{
    /// <summary>
    /// Wrapper class for circle packing operations using a particle-spring simulation system.
    /// </summary>
    public class CirclePacking
    {
        private CirclePacking()
        {
        }

        // Internal particle representation for the spring simulation
        private class Particle
        {
            public double X;
            public double Y;
            public double Radius;

            public Particle(double x, double y, double radius)
            {
                X = x;
                Y = y;
                Radius = radius;
            }
        }

        // Point-in-polygon test using ray casting algorithm
        private static bool IsPointInPolygon(double px, double py, double[] polyX, double[] polyY)
        {
            int n = polyX.Length;
            bool inside = false;
            int j = n - 1;
            for (int i = 0; i < n; i++)
            {
                if (((polyY[i] > py) != (polyY[j] > py)) &&
                    (px < (polyX[j] - polyX[i]) * (py - polyY[i]) / (polyY[j] - polyY[i]) + polyX[i]))
                {
                    inside = !inside;
                }
                j = i;
            }
            return inside;
        }

        // Apply one step of particle-spring separation to a list of particles
        private static void RunSimulationStep(List<Particle> particles)
        {
            int count = particles.Count;
            double[] dx = new double[count];
            double[] dy = new double[count];

            for (int i = 0; i < count; i++)
            {
                for (int j = i + 1; j < count; j++)
                {
                    double ddx = particles[j].X - particles[i].X;
                    double ddy = particles[j].Y - particles[i].Y;
                    double dist = SystemMath.Sqrt(ddx * ddx + ddy * ddy);
                    double minDist = particles[i].Radius + particles[j].Radius;

                    if (dist < minDist && dist > 1e-10)
                    {
                        double overlap = minDist - dist;
                        double nx = ddx / dist;
                        double ny = ddy / dist;

                        // Push each circle half the overlap distance apart
                        dx[i] -= nx * overlap * 0.5;
                        dy[i] -= ny * overlap * 0.5;
                        dx[j] += nx * overlap * 0.5;
                        dy[j] += ny * overlap * 0.5;
                    }
                }
            }

            for (int i = 0; i < count; i++)
            {
                particles[i].X += dx[i];
                particles[i].Y += dy[i];
            }
        }

        /// <summary>
        /// Packs circles within a boundary polygon using a particle-spring simulation system.
        /// Circles repel each other to prevent overlapping while remaining constrained within the boundary.
        /// </summary>
        /// <param name="boundary">The boundary polygon to pack circles within</param>
        /// <param name="count">Number of circles to pack (default: 10)</param>
        /// <param name="minRadius">Minimum circle radius (default: 1.0)</param>
        /// <param name="maxRadius">Maximum circle radius (default: 5.0)</param>
        /// <param name="iterations">Number of simulation iterations — more iterations produce better packing (default: 100)</param>
        /// <param name="seed">Random seed for reproducible results, use -1 for a random seed (default: -1)</param>
        /// <returns name="circles">The packed circles</returns>
        /// <returns name="centers">The center points of the packed circles</returns>
        /// <returns name="radii">The radii of the packed circles</returns>
        /// <search>circle packing, particle spring, generative design, circles, packing</search>
        [MultiReturn(new[] { "circles", "centers", "radii" })]
        public static Dictionary<string, object> ByBoundary(
            Autodesk.DesignScript.Geometry.Polygon boundary,
            int count = 10,
            double minRadius = 1.0,
            double maxRadius = 5.0,
            int iterations = 100,
            int seed = -1)
        {
#pragma warning disable CS0618
            var boundaryPoints = boundary.Points;
#pragma warning restore CS0618
            int numVertices = boundaryPoints.Length;
            double[] polyX = new double[numVertices];
            double[] polyY = new double[numVertices];

            for (int i = 0; i < numVertices; i++)
            {
                polyX[i] = boundaryPoints[i].X;
                polyY[i] = boundaryPoints[i].Y;
            }

            double centroidX = polyX.Average();
            double centroidY = polyY.Average();

            double bboxMinX = polyX.Min();
            double bboxMaxX = polyX.Max();
            double bboxMinY = polyY.Min();
            double bboxMaxY = polyY.Max();

            Random random = seed == -1 ? new Random() : new Random(seed);
            List<Particle> particles = new List<Particle>();

            int maxAttempts = count * 200;
            int attempts = 0;

            while (particles.Count < count && attempts < maxAttempts)
            {
                attempts++;
                double x = bboxMinX + random.NextDouble() * (bboxMaxX - bboxMinX);
                double y = bboxMinY + random.NextDouble() * (bboxMaxY - bboxMinY);
                double r = minRadius + random.NextDouble() * (maxRadius - minRadius);

                if (IsPointInPolygon(x, y, polyX, polyY))
                {
                    particles.Add(new Particle(x, y, r));
                }
            }

            for (int iter = 0; iter < iterations; iter++)
            {
                // Separate overlapping circles using the spring simulation
                RunSimulationStep(particles);

                // Enforce boundary: steer any circle that left the polygon back toward the centroid
                for (int i = 0; i < particles.Count; i++)
                {
                    if (!IsPointInPolygon(particles[i].X, particles[i].Y, polyX, polyY))
                    {
                        double toCentroidX = centroidX - particles[i].X;
                        double toCentroidY = centroidY - particles[i].Y;
                        double toCentroidDist = SystemMath.Sqrt(toCentroidX * toCentroidX + toCentroidY * toCentroidY);

                        if (toCentroidDist > 1e-10)
                        {
                            particles[i].X += toCentroidX / toCentroidDist * 0.5;
                            particles[i].Y += toCentroidY / toCentroidDist * 0.5;
                        }
                    }
                }
            }

            List<Circle> circles = new List<Circle>();
            List<Autodesk.DesignScript.Geometry.Point> centers = new List<Autodesk.DesignScript.Geometry.Point>();
            List<double> radii = new List<double>();

            foreach (var p in particles)
            {
                var center = Autodesk.DesignScript.Geometry.Point.ByCoordinates(p.X, p.Y, 0);
                centers.Add(center);
                radii.Add(p.Radius);
                circles.Add(Circle.ByCenterPointRadius(center, p.Radius));
            }

            return new Dictionary<string, object>
            {
                { "circles", circles },
                { "centers", centers },
                { "radii", radii }
            };
        }

        /// <summary>
        /// Packs circles from given starting positions using a particle-spring simulation system.
        /// Circles repel each other to eliminate overlaps while maintaining their original radii.
        /// </summary>
        /// <param name="centers">Initial center points of the circles</param>
        /// <param name="radii">Radii of the circles. If fewer radii than centers are provided, the last radius is reused.</param>
        /// <param name="iterations">Number of simulation iterations — more iterations produce better separation (default: 100)</param>
        /// <returns name="circles">The packed circles</returns>
        /// <returns name="centers">The center points of the packed circles</returns>
        /// <returns name="radii">The radii of the packed circles</returns>
        /// <search>circle packing, particle spring, generative design, circles, packing</search>
        [MultiReturn(new[] { "circles", "centers", "radii" })]
        public static Dictionary<string, object> ByCentersAndRadii(
            List<Autodesk.DesignScript.Geometry.Point> centers,
            List<double> radii,
            int iterations = 100)
        {
            if (centers == null || centers.Count == 0)
                throw new ArgumentException("centers list must not be empty.", nameof(centers));
            if (radii == null || radii.Count == 0)
                throw new ArgumentException("radii list must not be empty.", nameof(radii));

            int count = centers.Count;
            List<Particle> particles = new List<Particle>(count);

            for (int i = 0; i < count; i++)
            {
                double r = i < radii.Count ? radii[i] : radii[radii.Count - 1];
                particles.Add(new Particle(centers[i].X, centers[i].Y, r));
            }

            for (int iter = 0; iter < iterations; iter++)
            {
                RunSimulationStep(particles);
            }

            List<Circle> outCircles = new List<Circle>(count);
            List<Autodesk.DesignScript.Geometry.Point> outCenters = new List<Autodesk.DesignScript.Geometry.Point>(count);
            List<double> outRadii = new List<double>(count);

            foreach (var p in particles)
            {
                var center = Autodesk.DesignScript.Geometry.Point.ByCoordinates(p.X, p.Y, 0);
                outCenters.Add(center);
                outRadii.Add(p.Radius);
                outCircles.Add(Circle.ByCenterPointRadius(center, p.Radius));
            }

            return new Dictionary<string, object>
            {
                { "circles", outCircles },
                { "centers", outCenters },
                { "radii", outRadii }
            };
        }
    }
}

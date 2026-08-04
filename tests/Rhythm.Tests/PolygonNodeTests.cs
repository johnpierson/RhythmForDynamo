using System;
using NUnit.Framework;

namespace Rhythm.Tests
{
    /// <summary>
    /// Regression tests for Rhythm.Geometry.Polygon.MinimumCircle.
    ///
    /// The node passed the computed radius as the point's Z coordinate and supplied no radius
    /// argument at all, so every call returned a circle of the DesignScript default radius (1)
    /// floating at an elevation equal to the radius it should have used.
    ///
    /// These exercise ComputeInscribedCircle, which is the whole of the fix bar the final
    /// Circle.ByCenterPointRadius call. Testing that instead of the node itself is what lets these
    /// run on a CI agent with no ProtoGeometry/ASM present - a test that cannot run protects
    /// nothing.
    /// </summary>
    [TestFixture]
    public class ComputeInscribedCircleTests
    {
        /// <summary>Wraps a ring of (x, y) pairs in the nested array shape polylabel expects.</summary>
        private static double[][][] Ring(params double[][] points)
        {
            return new[] { points };
        }

        [Test]
        public void SquareGivesItsCentreAndTheDistanceToTheEdge()
        {
            var square = Ring(
                new[] { 0.0, 0.0 },
                new[] { 10.0, 0.0 },
                new[] { 10.0, 10.0 },
                new[] { 0.0, 10.0 });

            var result = global::Rhythm.Geometry.Polygon.ComputeInscribedCircle(square);

            Assert.That(result[0], Is.EqualTo(5).Within(1e-9), "centre x");
            Assert.That(result[1], Is.EqualTo(5).Within(1e-9), "centre y");
            Assert.That(result[2], Is.EqualTo(5).Within(1e-9),
                "radius - this used to be written into Z, leaving the radius at the default 1");
        }

        [Test]
        public void RadiusIsNeverTheDefaultOne()
        {
            // A 40x40 square: the correct radius is 20. The bug returned 1 regardless of size, so
            // any polygon whose true radius is not 1 catches it.
            var square = Ring(
                new[] { 0.0, 0.0 },
                new[] { 40.0, 0.0 },
                new[] { 40.0, 40.0 },
                new[] { 0.0, 40.0 });

            var result = global::Rhythm.Geometry.Polygon.ComputeInscribedCircle(square);

            Assert.That(result[2], Is.EqualTo(20).Within(1e-9));
        }

        /// <summary>
        /// A U shape. Its centroid lands at roughly (5, 4.4), which is in the gap between the two
        /// legs and therefore outside the polygon, so GetCentroidCell reports a negative distance.
        /// This is valid input and must not be rejected: the routine falls back to the pole of
        /// inaccessibility, which is interior by construction.
        /// </summary>
        [Test]
        public void ConcavePolygonWhoseCentroidFallsOutsideStillGetsACircle()
        {
            var horseshoe = Ring(
                new[] { 0.0, 0.0 },
                new[] { 10.0, 0.0 },
                new[] { 10.0, 10.0 },
                new[] { 7.0, 10.0 },
                new[] { 7.0, 3.0 },
                new[] { 3.0, 3.0 },
                new[] { 3.0, 10.0 },
                new[] { 0.0, 10.0 });

            double[] result = null;
            Assert.DoesNotThrow(() => result = global::Rhythm.Geometry.Polygon.ComputeInscribedCircle(horseshoe),
                "a concave polygon is valid input and must not be rejected outright");

            TestContext.WriteLine($"centre = ({result[0]:F3}, {result[1]:F3}), radius = {result[2]:F3}");

            Assert.That(result[2], Is.GreaterThan(0), "the radius must be positive");

            // The centre has to be inside one of the three arms, never in the gap between the legs.
            bool inGap = result[0] > 3.0 && result[0] < 7.0 && result[1] > 3.0;
            Assert.That(inGap, Is.False,
                $"centre ({result[0]:F2}, {result[1]:F2}) landed in the U's opening, outside the polygon");

            // Pin down that the fallback is what produced this, rather than the test passing for
            // the wrong reason: the centroid of this outline is (5, 4.4167), so returning the
            // centroid would have put the centre in the gap the assertion above rejects. Asserting
            // it directly means this test still means something if that geometry ever changes.
            // global:: because RhythmCore declares a Rhythm.System namespace, which shadows the BCL
            // root from inside Rhythm.Tests.
            bool isCentroid = global::System.Math.Abs(result[0] - 5.0) < 0.01
                              && global::System.Math.Abs(result[1] - 4.4167) < 0.01;
            Assert.That(isCentroid, Is.False,
                "the centroid was returned, so the pole-of-inaccessibility fallback did not fire");
        }

        [Test]
        public void DegenerateZeroAreaPolygonIsRejectedWithAReadableMessage()
        {
            // All points collinear: there is no interior, so no circle can be placed.
            var line = Ring(
                new[] { 0.0, 0.0 },
                new[] { 10.0, 0.0 },
                new[] { 20.0, 0.0 });

            var ex = Assert.Throws<InvalidOperationException>(
                () => global::Rhythm.Geometry.Polygon.ComputeInscribedCircle(line));

            Assert.That(ex.Message, Does.Contain("no circle"));
        }
    }
}

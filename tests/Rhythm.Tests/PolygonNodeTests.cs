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
    /// These tests exercise the geometry through ProtoGeometry, so they need the Dynamo geometry
    /// libraries beside the test assembly. They are marked Explicit rather than silently skipped:
    /// a test that quietly does not run is worse than one that is honestly not run by default.
    /// </summary>
    [TestFixture]
    [Explicit("Requires ProtoGeometry (ASM) to be loadable; run from a Dynamo-enabled environment.")]
    public class MinimumCircleTests
    {
        [Test]
        public void ReturnsACircleInThePlaneOfThePolygon()
        {
            var square = Autodesk.DesignScript.Geometry.Polygon.ByPoints(new[]
            {
                Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 0, 0),
                Autodesk.DesignScript.Geometry.Point.ByCoordinates(10, 0, 0),
                Autodesk.DesignScript.Geometry.Point.ByCoordinates(10, 10, 0),
                Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 10, 0)
            });

            var circle = global::Rhythm.Geometry.Polygon.MinimumCircle(square);

            Assert.That(circle.CenterPoint.Z, Is.EqualTo(0).Within(1e-9),
                "the radius used to be written into the Z coordinate");
            Assert.That(circle.Radius, Is.EqualTo(5).Within(1e-6),
                "a 10x10 square's centroid is 5 from each edge; the radius used to always be 1");
        }
    }
}

using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace HoloGraffiti
{
    // EditMode tests for TubeMeshGenerator.CreateTube.
    public class TubeMeshGeneratorTests
    {
        private GameObject tube;

        [TearDown]
        public void TearDown()
        {
            if (tube == null)
                return;

            Object.DestroyImmediate(tube.GetComponent<MeshFilter>().sharedMesh);
            Object.DestroyImmediate(tube);
        }

        [Test]
        public void CreateTube_OnePoint_ReturnsNull()
        {
            tube = TubeMeshGenerator.CreateTube(new List<Vector3> { Vector3.zero }, null);

            Assert.IsNull(tube);
        }

        [Test]
        public void CreateTube_PointsCloserThanTheTubeWidth_ReturnsNull()
        {
            // A quick tap: the hand moves a few millimeters, less than the
            // 1 cm (radius * 2) needed to keep a second point.
            var points = new List<Vector3>
            {
                Vector3.zero,
                new Vector3(0.003f, 0f, 0f),
                new Vector3(0.006f, 0f, 0f),
            };

            tube = TubeMeshGenerator.CreateTube(points, null);

            Assert.IsNull(tube);
        }

        [Test]
        public void CreateTube_TwoPointsFarApart_BuildsOneRingPerPoint()
        {
            var points = new List<Vector3> { Vector3.zero, new Vector3(0f, 0f, 0.1f) };

            tube = TubeMeshGenerator.CreateTube(points, null, radius: 0.005f, sides: 12);

            Assert.IsNotNull(tube);
            Mesh mesh = tube.GetComponent<MeshFilter>().sharedMesh;
            Assert.AreEqual(2 * 12, mesh.vertexCount);
            // One quad (2 triangles, 6 indices) per side between the two rings
            Assert.AreEqual(12 * 6, mesh.triangles.Length);
        }
    }
}

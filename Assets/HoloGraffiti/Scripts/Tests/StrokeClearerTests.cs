using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace HoloGraffiti
{
    // EditMode tests for StrokeClearer.
    public class StrokeClearerTests
    {
        private readonly List<Object> created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object obj in created)
            {
                if (obj != null)
                    Object.DestroyImmediate(obj);
            }
            created.Clear();
        }

        [Test]
        public void Clear_DestroysStrokesAndTheirMeshes()
        {
            GameObject first = CreateTube(Vector3.zero);
            GameObject second = CreateTube(Vector3.right);
            Mesh firstMesh = first.GetComponent<MeshFilter>().sharedMesh;

            int cleared = StrokeClearer.Clear(new[] { first, second });

            Assert.AreEqual(2, cleared);
            // Unity objects compare equal to null once they are destroyed
            Assert.IsTrue(first == null);
            Assert.IsTrue(second == null);
            Assert.IsTrue(firstMesh == null);
        }

        [Test]
        public void Clear_EmptyArray_ReturnsZero()
        {
            Assert.AreEqual(0, StrokeClearer.Clear(new GameObject[0]));
        }

        [Test]
        public void Clear_SkipsAlreadyDestroyedStrokes()
        {
            GameObject stroke = CreateTube(Vector3.zero);
            StrokeClearer.Clear(new[] { stroke });

            Assert.AreEqual(0, StrokeClearer.Clear(new[] { stroke }));
        }

        private GameObject CreateTube(Vector3 start)
        {
            var points = new List<Vector3> { start, start + new Vector3(0f, 0f, 0.1f) };
            GameObject tube = TubeMeshGenerator.CreateTube(points, null);
            created.Add(tube);
            created.Add(tube.GetComponent<MeshFilter>().sharedMesh);
            return tube;
        }
    }
}

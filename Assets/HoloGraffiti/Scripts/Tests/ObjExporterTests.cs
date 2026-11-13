using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HoloGraffiti
{
    // EditMode tests for ObjExporter.
    // EditMode tests run inside the Unity Editor without entering Play mode,
    // so they are fast but cannot test runtime-only behaviour like physics.
    public class ObjExporterTests
    {
        // Everything a test creates is tracked here and removed in TearDown,
        // so one test can't leave objects or files behind for the next.
        private readonly List<Object> created = new List<Object>();
        private string exportDirectory;

        [SetUp]
        public void SetUp()
        {
            exportDirectory = Path.Combine(
                Path.GetTempPath(),
                "HoloGraffitiTests_" + Guid.NewGuid().ToString("N")
            );
        }

        [TearDown]
        public void TearDown()
        {
            // DestroyImmediate is the edit-mode equivalent of Destroy.
            foreach (Object obj in created)
                Object.DestroyImmediate(obj);
            created.Clear();

            if (Directory.Exists(exportDirectory))
                Directory.Delete(exportDirectory, true);
        }

        [Test]
        public void ExportStrokes_EmptyArray_DoesNotThrow()
        {
            // Arrange: create an exporter with no strokes to export.
            ObjExporter exporter = CreateExporter();

            // Act + Assert: passing an empty array should log a message and return cleanly.
            string path = null;
            Assert.DoesNotThrow(() =>
                path = exporter.ExportStrokes(new GameObject[0], exportDirectory)
            );
            Assert.IsNull(path);
        }

        [Test]
        public void ExportStrokes_OneStroke_WritesObjFile()
        {
            // Arrange: one stroke GameObject with a simple triangle mesh.
            GameObject stroke = CreateStroke("Stroke", BuildTriangleMesh());
            ObjExporter exporter = CreateExporter();

            // Act
            string path = exporter.ExportStrokes(new[] { stroke }, exportDirectory);

            // Assert: a timestamped .obj file exists and holds the same text BuildObj returns.
            Assert.IsTrue(File.Exists(path));
            StringAssert.EndsWith(".obj", path);
            Assert.AreEqual(ObjExporter.BuildObj(new[] { stroke }), File.ReadAllText(path));
        }

        [Test]
        public void BuildObj_OneStroke_WritesObjectVerticesNormalsAndFace()
        {
            GameObject stroke = CreateStroke("Stroke", BuildTriangleMesh());

            string[] lines = Lines(ObjExporter.BuildObj(new[] { stroke }));

            Assert.Contains("o Stroke", lines);
            Assert.AreEqual(3, lines.Count(l => l.StartsWith("v ")));
            Assert.AreEqual(3, lines.Count(l => l.StartsWith("vn ")));
            Assert.AreEqual(1, lines.Count(l => l.StartsWith("f ")));
        }

        [Test]
        public void BuildObj_NegatesZAndReversesWinding()
        {
            // Unity is left-handed and OBJ is right-handed. Flipping Z converts
            // between them, and reversing each triangle keeps its front face outward.
            // Without both, the model opens mirrored or inside-out.
            GameObject stroke = CreateStroke("Stroke", BuildTriangleMesh(z: 2f));

            string[] lines = Lines(ObjExporter.BuildObj(new[] { stroke }));

            Assert.Contains("v 0 0 -2", lines);
            Assert.Contains("vn 0 0 -1", lines); // the triangle's normal is (0, 0, 1) in Unity
            Assert.Contains("f 1//1 3//3 2//2", lines);
        }

        [Test]
        public void BuildObj_TwoStrokes_OffsetsSecondStrokesFaceIndices()
        {
            // OBJ numbers vertices across the whole file, so the second stroke's
            // face must point at vertices 4-6, not 1-3.
            GameObject first = CreateStroke("First", BuildTriangleMesh());
            GameObject second = CreateStroke("Second", BuildTriangleMesh());

            string[] lines = Lines(ObjExporter.BuildObj(new[] { first, second }));

            Assert.Contains("f 1//1 3//3 2//2", lines);
            Assert.Contains("f 4//4 6//6 5//5", lines);
        }

        [Test]
        public void BuildObj_WritesWorldSpacePositions()
        {
            GameObject stroke = CreateStroke("Stroke", BuildTriangleMesh());
            stroke.transform.position = new Vector3(10f, 0f, 0f);

            string[] lines = Lines(ObjExporter.BuildObj(new[] { stroke }));

            Assert.Contains("v 10 0 0", lines);
            Assert.Contains("v 11 0 0", lines);
        }

        [Test]
        public void BuildObj_UsesDecimalPointInAnyLocale()
        {
            // German writes one half as "0,5". An OBJ file must always use "0.5".
            GameObject stroke = CreateStroke("Stroke", BuildTriangleMesh(z: 0.5f));
            CultureInfo original = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            string obj;
            try
            {
                obj = ObjExporter.BuildObj(new[] { stroke });
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }

            StringAssert.Contains("v 0 0 -0.5", obj);
            StringAssert.DoesNotContain(",", obj);
        }

        [Test]
        public void BuildObj_MeshWithoutNormals_WritesFacesWithoutNormalIndices()
        {
            Mesh mesh = BuildTriangleMesh();
            mesh.normals = null;
            GameObject stroke = CreateStroke("Stroke", mesh);

            string[] lines = Lines(ObjExporter.BuildObj(new[] { stroke }));

            Assert.AreEqual(0, lines.Count(l => l.StartsWith("vn ")));
            Assert.Contains("f 1 3 2", lines);
        }

        [Test]
        public void BuildObj_SkipsObjectsWithoutAMesh()
        {
            var empty = new GameObject("Empty");
            created.Add(empty);
            GameObject stroke = CreateStroke("Stroke", BuildTriangleMesh());

            string[] lines = Lines(ObjExporter.BuildObj(new[] { empty, stroke }));

            Assert.AreEqual(1, lines.Count(l => l.StartsWith("o ")));
            Assert.Contains("f 1//1 3//3 2//2", lines);
        }

        private ObjExporter CreateExporter()
        {
            var exporterObject = new GameObject("ObjExporterTest");
            created.Add(exporterObject);
            return exporterObject.AddComponent<ObjExporter>();
        }

        private GameObject CreateStroke(string name, Mesh mesh)
        {
            var stroke = new GameObject(name);
            created.Add(stroke);
            stroke.AddComponent<MeshFilter>().sharedMesh = mesh;
            return stroke;
        }

        // Builds a minimal valid mesh (one triangle) for testing purposes.
        private Mesh BuildTriangleMesh(float z = 0f)
        {
            var mesh = new Mesh();
            created.Add(mesh);
            mesh.vertices = new Vector3[]
            {
                new Vector3(0, 0, z),
                new Vector3(1, 0, z),
                new Vector3(0, 1, z),
            };
            mesh.triangles = new int[] { 0, 1, 2 };
            mesh.RecalculateNormals();
            return mesh;
        }

        private static string[] Lines(string text)
        {
            return text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HoloGraffiti
{
    // EditMode tests for GlbExporter.
    // These tests pass strokes in directly to avoid needing the "TubeStroke"
    // tag registered in the project settings.
    public class GlbExporterTests
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
            GlbExporter exporter = CreateExporter();

            // Act + Assert: passing an empty array should log a message and return cleanly.
            Assert.DoesNotThrow(() => exporter.ExportStrokes(new GameObject[0]));
        }

        [Test]
        public async Task ExportStrokesAsync_EmptyArray_ReturnsNull()
        {
            GlbExporter exporter = CreateExporter();

            string path = await exporter.ExportStrokesAsync(new GameObject[0], exportDirectory);

            Assert.IsNull(path);
        }

        [Test]
        public async Task ExportStrokesAsync_OneStroke_WritesGlbFile()
        {
            // Arrange: one stroke with a mesh and a renderer.
            // MeshRenderer is required by glTFast - it reads the material from it.
            var stroke = new GameObject("Stroke");
            created.Add(stroke);
            stroke.AddComponent<MeshFilter>().sharedMesh = BuildTriangleMesh();
            stroke.AddComponent<MeshRenderer>();
            GlbExporter exporter = CreateExporter();

            // Act: awaiting the task means the test waits for the file to be written.
            string path = await exporter.ExportStrokesAsync(new[] { stroke }, exportDirectory);

            // Assert: every GLB file starts with the four ASCII letters "glTF".
            Assert.IsTrue(File.Exists(path));
            StringAssert.EndsWith(".glb", path);
            byte[] bytes = File.ReadAllBytes(path);
            Assert.GreaterOrEqual(bytes.Length, 4);
            Assert.AreEqual("glTF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
        }

        private GlbExporter CreateExporter()
        {
            var exporterObject = new GameObject("GlbExporterTest");
            created.Add(exporterObject);
            return exporterObject.AddComponent<GlbExporter>();
        }

        // Builds a minimal valid mesh (one triangle) for testing purposes.
        private Mesh BuildTriangleMesh()
        {
            var mesh = new Mesh();
            created.Add(mesh);
            mesh.vertices = new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(0, 1, 0),
            };
            mesh.triangles = new int[] { 0, 1, 2 };
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}

using NUnit.Framework;
using UnityEngine;

namespace HoloGraffiti
{
    // EditMode tests for GlbExporter.
    // These tests call ExportStrokes directly to avoid needing the "TubeStroke"
    // tag registered in the project settings.
    public class GlbExporterTests
    {
        [Test]
        public void ExportStrokes_EmptyArray_DoesNotThrow()
        {
            // Arrange: create an exporter with no strokes to export.
            var exporterObject = new GameObject("GlbExporterTest");
            var exporter = exporterObject.AddComponent<GlbExporter>();

            // Act + Assert: passing an empty array should log a message and return cleanly.
            Assert.DoesNotThrow(() => exporter.ExportStrokes(new GameObject[0]));

            Object.DestroyImmediate(exporterObject);
        }

        [Test]
        public void ExportStrokes_OneStroke_DoesNotThrow()
        {
            // Arrange: one stroke with a mesh and a renderer.
            // MeshRenderer is required by glTFast - it reads the material from it.
            var stroke = new GameObject("Stroke");
            stroke.AddComponent<MeshFilter>().sharedMesh = BuildTriangleMesh();
            stroke.AddComponent<MeshRenderer>();

            var exporterObject = new GameObject("GlbExporterTest");
            var exporter = exporterObject.AddComponent<GlbExporter>();

            // Act + Assert: should kick off the async GLB export without throwing.
            Assert.DoesNotThrow(() => exporter.ExportStrokes(new GameObject[] { stroke }));

            Object.DestroyImmediate(stroke);
            Object.DestroyImmediate(exporterObject);
        }

        // Builds a minimal valid mesh (one triangle) for testing purposes.
        private static Mesh BuildTriangleMesh()
        {
            var mesh = new Mesh();
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
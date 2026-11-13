using NUnit.Framework;
using UnityEngine;

namespace HoloGraffiti
{
    // EditMode tests for ObjExporter.
    // EditMode tests run inside the Unity Editor without entering Play mode,
    // so they are fast but cannot test runtime-only behaviour like physics.
    public class ObjExporterTests
    {
        [Test]
        public void ExportStrokes_EmptyArray_DoesNotThrow()
        {
            // Arrange: create an exporter with no strokes to export.
            var exporterObject = new GameObject("ObjExporterTest");
            var exporter = exporterObject.AddComponent<ObjExporter>();

            // Act + Assert: passing an empty array should log a message and return cleanly.
            Assert.DoesNotThrow(() => exporter.ExportStrokes(new GameObject[0]));

            // Cleanup: DestroyImmediate is the edit-mode equivalent of Destroy.
            Object.DestroyImmediate(exporterObject);
        }

        [Test]
        public void ExportStrokes_OneStroke_DoesNotThrow()
        {
            // Arrange: one stroke GameObject with a simple triangle mesh.
            var stroke = new GameObject("Stroke");
            stroke.AddComponent<MeshFilter>().sharedMesh = BuildTriangleMesh();

            var exporterObject = new GameObject("ObjExporterTest");
            var exporter = exporterObject.AddComponent<ObjExporter>();

            // Act + Assert: should write an OBJ file without throwing.
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
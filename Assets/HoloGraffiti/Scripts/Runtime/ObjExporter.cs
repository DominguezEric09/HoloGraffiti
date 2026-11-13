using UnityEngine;
using System.IO;
using System.Text;
using System.Globalization;

namespace HoloGraffiti
{
    // Exports all tagged stroke GameObjects in the scene to a single OBJ file.
    public class ObjExporter : MonoBehaviour
    {
        // Called at runtime (e.g. from a UI button) to gather all strokes in the
        // scene by their "TubeStroke" tag and export them.
        public void ExportAllStrokes()
        {
            GameObject[] strokes = GameObject.FindGameObjectsWithTag("TubeStroke");
            ExportStrokes(strokes);
        }

        /// Exports the given array of GameObjects to an OBJ file.
        /// Separated from ExportAllStrokes so tests can pass strokes in directly
        /// without needing the "TubeStroke" tag registered in the project.
        public void ExportStrokes(GameObject[] strokes)
        {
            if (strokes.Length == 0)
            {
                Debug.Log("No strokes to export.");
                return;
            }

            // Build a timestamped filename so each export is unique.
            // InvariantCulture ensures the date format is the same on every machine.
            string timestamp = System.DateTime.Now.ToString(
                "yyyy-MM-dd_HH-mm-ss",
                CultureInfo.InvariantCulture
            );
            // persistentDataPath is the one folder Android always lets us write to.
            string path = Path.Combine(
                Application.persistentDataPath,
                $"drawing_{timestamp}.obj"
            );

            var sb = new StringBuilder();
            
            // OBJ vertex indices start at 1, not 0.
            // vertexOffset tracks where the next object's vertices begin.
            int vertexOffset = 1;

            foreach (GameObject obj in strokes)
            {
                MeshFilter meshFilter = obj.GetComponent<MeshFilter>();

                // sharedMesh is the asset mesh (safe in both edit and play mode).
                // mesh would create a copy at runtime and leak in edit mode tests. 
                if (meshFilter == null || meshFilter.sharedMesh == null)
                    continue;

                Mesh mesh = meshFilter.sharedMesh;
                Transform t = obj.transform;

                // Write each vertex position in world space.
                // Unity is left-handed (Z forward); OBJ is right-handed, so we negate Z.
                foreach (Vector3 v in mesh.vertices)
                {
                    Vector3 world = t.TransformPoint(v);
                    sb.AppendLine(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "v {0} {1} {2}",
                            world.x,
                            world.y,
                            -world.z
                        )
                    );
                }

                // Write each vertex normal in world space (also negating Z).
                foreach (Vector3 n in mesh.normals)
                {
                    Vector3 worldNormal = t.TransformDirection(n);
                    sb.AppendLine(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "vn {0} {1} {2}",
                            worldNormal.x,
                            worldNormal.y,
                            -worldNormal.z
                        )
                    );
                }
                
                // Write each triangle face.
                // Unity winding is clockwise; OBJ is counter-clockwise, so we swap b and c.
                int[] triangles = mesh.triangles;
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int a = triangles[i] + vertexOffset;
                    int b = triangles[i + 1] + vertexOffset;
                    int c = triangles[i + 2] + vertexOffset;
                    sb.AppendLine($"f {a}//{a} {c}//{c} {b}//{b}");
                }

                vertexOffset += mesh.vertices.Length;
            }

            File.WriteAllText(path, sb.ToString());
            Debug.Log($"OBJ exported to: {path}");
        }
    }
}
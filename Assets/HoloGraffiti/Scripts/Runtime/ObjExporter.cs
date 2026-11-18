using UnityEngine;
using System.IO;
using System.Text;
using System.Globalization;

namespace HoloGraffiti
{
    /// <summary>
    /// Exports all tagged stroke GameObjects in the scene to a single OBJ file.
    /// OBJ is a simple text format that stores vertex positions, normals and faces.
    ///
    /// ExportAllStrokes() finds strokes by the TubeStroke tag (must be registered
    /// in Project Settings → Tags and Layers).
    /// ExportStrokes(GameObject[] strokes) is the testable version — pass strokes
    /// directly so tests do not need the tag registered.
    /// </summary>
    public class ObjExporter : MonoBehaviour
    {
        /// <summary>
        /// Finds all GameObjects tagged TubeStroke and exports them to an OBJ file.
        /// Called by the Export button on the hand menu.
        /// </summary>
        public void ExportAllStrokes()
        {
            GameObject[] strokes = GameObject.FindGameObjectsWithTag("TubeStroke");
            ExportStrokes(strokes);
        }

        /// <summary>
        /// Exports the given array of GameObjects to a timestamped OBJ file in
        /// the app's persistent data folder (accessible via adb pull on Quest).
        /// </summary>
        public void ExportStrokes(GameObject[] strokes)
        {
            if (strokes.Length == 0)
            {
                Debug.Log("No strokes to export.");
                return;
            }
            string timestamp = System.DateTime.Now.ToString(
                "yyyy-MM-dd_HH-mm-ss",
                CultureInfo.InvariantCulture
            );
            string path = Path.Combine(
                Application.persistentDataPath,
                $"drawing_{timestamp}.obj"
            );
            var sb = new StringBuilder();
            int vertexOffset = 1;
            foreach (GameObject obj in strokes)
            {
                MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
                if (meshFilter == null || meshFilter.sharedMesh == null)
                    continue;
                Mesh mesh = meshFilter.sharedMesh;
                Transform t = obj.transform;
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
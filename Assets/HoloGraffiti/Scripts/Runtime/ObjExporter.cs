using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

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

        /// Exports the given array of GameObjects to an OBJ file and returns the
        /// file's path, or null if there was nothing to export.
        /// Separated from ExportAllStrokes so tests can pass strokes in directly
        /// without needing the "TubeStroke" tag registered in the project.
        /// directory is where the file goes; leave it out to use persistentDataPath,
        /// the one folder Android always lets us write to.
        public string ExportStrokes(GameObject[] strokes, string directory = null)
        {
            if (strokes == null || strokes.Length == 0)
            {
                Debug.Log("No strokes to export.");
                return null;
            }

            string path = ExportFile.NewPath(directory, "obj");
            File.WriteAllText(path, BuildObj(strokes));
            Debug.Log($"OBJ exported to: {path}");
            return path;
        }

        /// Builds the text of an OBJ file from the given GameObjects.
        /// Kept separate from the file writing so tests can check the text directly.
        public static string BuildObj(GameObject[] strokes)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Exported from HoloGraffiti");

            // OBJ vertex indices start at 1, not 0.
            // vertexOffset tracks where the next object's vertices begin.
            int vertexOffset = 1;

            foreach (GameObject obj in strokes)
            {
                if (obj == null)
                    continue;

                MeshFilter meshFilter = obj.GetComponent<MeshFilter>();

                // sharedMesh is the asset mesh (safe in both edit and play mode).
                // mesh would create a copy at runtime and leak in edit mode tests.
                if (meshFilter == null || meshFilter.sharedMesh == null)
                    continue;

                Mesh mesh = meshFilter.sharedMesh;
                Transform t = obj.transform;

                // Each of these properties copies the whole array every time it is
                // read, so read each one once.
                Vector3[] vertices = mesh.vertices;
                Vector3[] normals = mesh.normals;
                int[] triangles = mesh.triangles;

                // A face can only point at normals if the mesh has one per vertex.
                bool hasNormals = normals.Length == vertices.Length;

                // "o" starts a new named object, so each stroke stays separate
                // when the file is opened in Unity or Blender.
                sb.Append("o ").AppendLine(obj.name);

                // Write each vertex position in world space.
                // Unity is left-handed (Z forward); OBJ is right-handed, so we negate Z.
                foreach (Vector3 v in vertices)
                {
                    Vector3 world = t.TransformPoint(v);
                    AppendVector(sb, "v", world.x, world.y, -world.z);
                }

                // Write each vertex normal in world space (also negating Z).
                if (hasNormals)
                {
                    foreach (Vector3 n in normals)
                    {
                        Vector3 worldNormal = t.TransformDirection(n);
                        AppendVector(sb, "vn", worldNormal.x, worldNormal.y, -worldNormal.z);
                    }
                }

                // Write each triangle face.
                // Unity winding is clockwise; OBJ is counter-clockwise, so we swap b and c.
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int a = triangles[i] + vertexOffset;
                    int b = triangles[i + 1] + vertexOffset;
                    int c = triangles[i + 2] + vertexOffset;
                    if (hasNormals)
                        sb.AppendLine($"f {a}//{a} {c}//{c} {b}//{b}");
                    else
                        sb.AppendLine($"f {a} {c} {b}");
                }

                vertexOffset += vertices.Length;
            }

            return sb.ToString();
        }

        // InvariantCulture always writes a decimal point. Without it, a device set
        // to German or French would write "0,5" and break the file.
        static void AppendVector(StringBuilder sb, string prefix, float x, float y, float z)
        {
            // Negating zero gives "negative zero", which some platforms print as "-0".
            // -0 == 0 is true, so this swaps it for a plain 0.
            if (x == 0f)
                x = 0f;
            if (y == 0f)
                y = 0f;
            if (z == 0f)
                z = 0f;

            sb.Append(prefix)
                .Append(' ')
                .Append(x.ToString(CultureInfo.InvariantCulture))
                .Append(' ')
                .Append(y.ToString(CultureInfo.InvariantCulture))
                .Append(' ')
                .AppendLine(z.ToString(CultureInfo.InvariantCulture));
        }
    }
}

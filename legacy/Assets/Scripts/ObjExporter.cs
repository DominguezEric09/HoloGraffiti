using UnityEngine; // Basic Unity functions — GameObject, MeshFilter, Debug, etc.
using System.Collections.Generic; // Gives us access to List<> (not directly used here but good practice)
using System.IO; // Gives us File and Path — needed to write and save the OBJ file
using System.Text; // Gives us StringBuilder — an efficient way to build large strings

public class OBJExporter : MonoBehaviour
{
    // EXPORT ALL STROKES
    public void ExportAllStrokes()
    // This is the method that gets called when the Export button is pressed
    // It finds all tube strokes in the scene and writes them to an OBJ file
    {
        // StringBuilder is like a notepad we write the OBJ file into
        // More efficient than using regular string concatenation for large files
        StringBuilder sb = new StringBuilder();

        // OBJ files number vertices starting at 1, not 0
        // This offset tracks the running total of vertices across all strokes
        // so each stroke's face indices are correct relative to the whole file
        int vertexOffset = 1;

        // FIND ALL TUBE STROKES IN THE SCENE
        // FindGameObjectsWithTag searches the entire scene for objects
        // tagged "TubeStroke" — the tag we assigned in TubeMeshGenerator
        // Returns an array of all matching GameObjects
        GameObject[] strokes = GameObject.FindGameObjectsWithTag("TubeStroke");

        // If there are no strokes in the scene, nothing to export
        // Log a message and exit early
        if (strokes.Length == 0)
        {
            Debug.Log("No strokes to export.");
            return;
        }

        // Write a comment at the top of the OBJ file
        // Lines starting with # are comments in OBJ format — ignored by importers
        sb.AppendLine("# Exported from HoloGraffiti");

        // LOOP THROUGH EVERY STROKE
        foreach (GameObject stroke in strokes)
        // For each TubeStroke object found in the scene...
        {
            // Get the MeshFilter component — this holds the geometry data
            // (vertices, triangles) that TubeMeshGenerator created
            MeshFilter mf = stroke.GetComponent<MeshFilter>();

            // If this object somehow doesn't have a MeshFilter, skip it
            if (mf == null) continue;

            // Get the actual mesh data from the MeshFilter
            Mesh mesh = mf.mesh;

            // Get the array of vertex positions (x, y, z coordinates)
            Vector3[] vertices = mesh.vertices;

            // Get the array of triangle indices
            // Every 3 numbers in this array = one triangle
            // The numbers refer to positions in the vertices array
            int[] triangles = mesh.triangles;

            // Write the object name to the OBJ file
            // "o" in OBJ format declares a new named object
            sb.AppendLine($"o {stroke.name}");

            // WRITE VERTICES
            // "v" in OBJ format declares a vertex position
            // Format: v x y z
            foreach (Vector3 v in vertices)
            {
                sb.AppendLine($"v {v.x} {v.y} {v.z}");
                // Each line writes one vertex's x, y, z position
                // These are in Unity's world space coordinates (meters)
            }

            // WRITE FACES
            // "f" in OBJ format declares a face (triangle)
            // Format: f index1 index2 index3
            // Indices refer to the vertex positions we wrote above
            for (int i = 0; i < triangles.Length; i += 3)
            // Step by 3 because every 3 triangle indices = one triangle
            {
                // Add the vertexOffset so indices are correct
                // relative to ALL vertices in the file, not just this stroke
                // Example: if stroke 1 had 100 vertices, stroke 2's
                // vertices start at index 101, so we add 100 to all its face indices
                int a = triangles[i] + vertexOffset;
                int b = triangles[i + 1] + vertexOffset;
                int c = triangles[i + 2] + vertexOffset;
                sb.AppendLine($"f {a} {b} {c}");
            }

            // After writing this stroke, advance the vertex offset
            // by the number of vertices in this stroke
            // so the next stroke's face indices start from the right number
            vertexOffset += vertices.Length;
        }

        // SAVE THE FILE 
        // Application.persistentDataPath is a folder on the device
        // that the app has permission to write to
        // On HoloLens this is in the app's internal storage
        // On PC (Unity editor) this is usually in AppData/LocalLow
        string path = Path.Combine(Application.persistentDataPath, "drawing.obj");

        // Write the entire StringBuilder contents to the file
        // If the file already exists it gets overwritten
        File.WriteAllText(path, sb.ToString());

        // Log the file path to the console so we know where it was saved
        Debug.Log($"OBJ exported to: {path}");
    }
}
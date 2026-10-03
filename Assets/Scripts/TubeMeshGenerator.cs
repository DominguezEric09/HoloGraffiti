using UnityEngine;
using System.Collections.Generic;

public class TubeMeshGenerator : MonoBehaviour
{
    // CreateTube is called by DrawingManager when a stroke ends
    // It takes the list of recorded points and builds a 3D tube around them
    public static GameObject CreateTube(List<Vector3> points, float radius = 0.005f, int sides = 12)
    // radius = how thick the tube is (0.005 = 5mm)
    // sides = how many flat faces make up the circle (12 = smoother than 8, cheaper than 16)
    {
        // Need at least 2 points to draw a line between them
        if (points.Count < 2) return null;

        // Create a new empty object in the scene to hold the tube
        GameObject tubeObject = new GameObject("TubeStroke");

        // Tag it so OBJExporter can find it later when exporting
        tubeObject.tag = "TubeStroke";

        // MeshFilter holds the geometry data (vertices and triangles)
        MeshFilter mf = tubeObject.AddComponent<MeshFilter>();

        // MeshRenderer makes it visible in the scene
        MeshRenderer mr = tubeObject.AddComponent<MeshRenderer>();

        // Try to use the MRTK shader first (optimized for HoloLens)
        // Fall back to Standard shader if MRTK isn't available (editor testing)
        // Fall back to Diffuse if Standard also isn't found
        Shader shader = Shader.Find("Mixed Reality Toolkit/Standard");
        if (shader == null)
            shader = Shader.Find("Standard");
        if (shader == null)
            shader = Shader.Find("Diffuse");

        // Apply the shader as a white material
        mr.material = new Material(shader);
        mr.material.color = UnityEngine.Color.white;

        // Build the actual mesh geometry and assign it
        Mesh mesh = BuildTubeMesh(points, radius, sides);
        mf.mesh = mesh;

        // Debug log to confirm how many vertices were created
        Debug.Log($"Tube created with {mesh.vertices.Length} vertices");

        return tubeObject;
    }

    static Mesh BuildTubeMesh(List<Vector3> points, float radius, int sides)
    {
        // STEP 1: Filter out points that are too close together 
        // When drawing slowly, the app records many points in nearly the same spot
        // These bunched points cause rings to overlap and create pinching artifacts
        // We only keep a point if it's at least (radius * 2) away from the last kept point
        List<Vector3> filtered = new List<Vector3> { points[0] };
        for (int i = 1; i < points.Count; i++)
        {
            if (Vector3.Distance(points[i], filtered[filtered.Count - 1]) > radius * 2)
                filtered.Add(points[i]);
        }
        points = filtered;

        // After filtering we still need at least 2 points
        if (points.Count < 2) return new Mesh();

        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // STEP 2: Build a ring of vertices around each point
        for (int i = 0; i < points.Count; i++)
        {
            // Figure out which direction the tube is going at this point
            Vector3 forward;
            if (i < points.Count - 1)
                // Look ahead to the next point
                forward = (points[i + 1] - points[i]).normalized;
            else
                // Last point — look back at the previous point
                forward = (points[i] - points[i - 1]).normalized;

            // STEP 3: Build a stable ring orientation using cross products
            // The problem: if we just rotate around the forward direction,
            // the ring can twist unpredictably at sharp corners
            // The fix: use Vector3.up as a consistent reference direction

            Vector3 up = Vector3.up;

            // Edge case: if the tube is going straight up, Vector3.up is parallel
            // to forward which breaks the math — switch to Vector3.right instead
            if (Mathf.Abs(Vector3.Dot(forward, up)) > 0.99f)
                up = Vector3.right;

            // Cross product gives us a vector perpendicular to both up and forward
            // This becomes our "right" direction for the ring
            Vector3 right = Vector3.Cross(up, forward).normalized;

            // Recalculate up so it's perfectly perpendicular to both forward and right
            // This gives us a clean, stable coordinate frame at every ring
            up = Vector3.Cross(forward, right).normalized;

            // STEP 4: Place vertices around the ring
            for (int j = 0; j < sides; j++)
            {
                // Evenly space vertices around a full circle (2 * PI = 360 degrees)
                float angle = (float)j / sides * Mathf.PI * 2;

                // Use cos and sin to place each vertex on the circle
                // Then scale by radius to control tube thickness
                Vector3 offset = (Mathf.Cos(angle) * right + Mathf.Sin(angle) * up) * radius;

                // Final vertex position = point on the stroke path + offset from center
                vertices.Add(points[i] + offset);
            }
        }

        // STEP 5: Connect rings with triangles to form the tube surface
        for (int i = 0; i < points.Count - 1; i++)
        {
            for (int j = 0; j < sides; j++)
            {
                // Calculate the 4 vertex indices that form a quad between two rings
                int current     = i * sides + j;               // current ring, current vertex
                int next        = i * sides + (j + 1) % sides; // current ring, next vertex (wraps around)
                int currentNext = (i + 1) * sides + j;         // next ring, current vertex
                int nextNext    = (i + 1) * sides + (j + 1) % sides; // next ring, next vertex

                // Split the quad into 2 triangles
                // Triangle 1
                triangles.Add(current);
                triangles.Add(currentNext);
                triangles.Add(next);

                // Triangle 2
                triangles.Add(next);
                triangles.Add(currentNext);
                triangles.Add(nextNext);
            }
        }

        // STEP 6: Finalize the mesh 
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        // Recalculate Normals figures out which way each face points
        // This is what makes lighting and shading work correctly
        mesh.RecalculateNormals();

        return mesh;
    }
}
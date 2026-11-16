using System.Collections.Generic;
using UnityEngine;

namespace HoloGraffiti
{
    public class TubeMeshGenerator : MonoBehaviour
    {
        // Each mesh has at most this many path segments. Adjacent chunks share a ring.
        public const int MaxSegmentsPerChunk = 128;

        public static GameObject CreateTube(List<Vector3> points, Material material,
            float radius = 0.005f, int sides = 12)
        {
            if (points == null || points.Count < 2 || radius <= 0f || sides < 3)
                return null;

            var path = new List<Vector3> { points[0] };
            for (int i = 1; i < points.Count; i++)
            {
                if (Vector3.Distance(points[i], path[path.Count - 1]) > radius * 2f)
                    path.Add(points[i]);
            }
            if (path.Count < 2) return null;

            // Calculate orientations once across the entire path, not per chunk.
            var rights = new Vector3[path.Count];
            var ups = new Vector3[path.Count];
            Vector3 previousTangent = Vector3.zero;
            for (int i = 0; i < path.Count; i++)
            {
                Vector3 tangent;
                if (i == 0) tangent = (path[1] - path[0]).normalized;
                else if (i == path.Count - 1)
                    tangent = (path[i] - path[i - 1]).normalized;
                else
                {
                    Vector3 incoming = (path[i] - path[i - 1]).normalized;
                    Vector3 outgoing = (path[i + 1] - path[i]).normalized;
                    tangent = incoming + outgoing;
                    if (tangent.sqrMagnitude < 1e-8f) tangent = outgoing;
                    tangent.Normalize();
                }

                if (i == 0)
                {
                    Vector3 reference = Mathf.Abs(Vector3.Dot(tangent, Vector3.up)) < 0.9f
                        ? Vector3.up : Vector3.right;
                    rights[i] = Vector3.Cross(reference, tangent).normalized;
                }
                else
                {
                    Quaternion rotation = Quaternion.FromToRotation(previousTangent, tangent);
                    Vector3 transported = rotation * rights[i - 1];
                    rights[i] = Vector3.ProjectOnPlane(transported, tangent).normalized;
                    if (rights[i].sqrMagnitude < 1e-8f)
                        rights[i] = Vector3.Cross(Vector3.up, tangent).normalized;
                }
                ups[i] = Vector3.Cross(tangent, rights[i]).normalized;
                previousTangent = tangent;
            }

            var root = new GameObject("TubeStroke");
            root.tag = "TubeStroke";
            int chunkNumber = 0;
            for (int start = 0; start < path.Count - 1; start += MaxSegmentsPerChunk)
            {
                int end = Mathf.Min(start + MaxSegmentsPerChunk, path.Count - 1);
                Mesh mesh = BuildChunk(path, rights, ups, start, end, radius, sides,
                    start == 0, end == path.Count - 1);
                var child = new GameObject("TubeChunk_" + chunkNumber++);
                child.transform.SetParent(root.transform, false);
                child.AddComponent<MeshFilter>().sharedMesh = mesh;
                child.AddComponent<MeshRenderer>().sharedMaterial = material;
            }
            return root;
        }

        private static Mesh BuildChunk(List<Vector3> path, Vector3[] rights, Vector3[] ups,
            int start, int end, float radius, int sides, bool capStart, bool capEnd)
        {
            int ringCount = end - start + 1;
            var vertices = new List<Vector3>(ringCount * sides + 2);
            var triangles = new List<int>((ringCount - 1) * sides * 6 + 2 * sides * 3);

            for (int i = start; i <= end; i++)
            {
                for (int j = 0; j < sides; j++)
                {
                    float angle = 2f * Mathf.PI * j / sides;
                    vertices.Add(path[i] + radius *
                        (Mathf.Cos(angle) * rights[i] + Mathf.Sin(angle) * ups[i]));
                }
            }
            for (int ring = 0; ring < ringCount - 1; ring++)
            {
                for (int j = 0; j < sides; j++)

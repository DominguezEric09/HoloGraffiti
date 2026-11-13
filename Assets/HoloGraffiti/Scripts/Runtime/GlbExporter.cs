using System.Globalization;
using System.IO;
using GLTFast;
using GLTFast.Export;
using UnityEngine;

namespace HoloGraffiti
{
    // Exports all tagged stroke GameObjects in the scene to a single GLB file.
    // GLB is the binary form of glTF - it bundles geometry, materials and textures
    // into one file, making it ideal for real-time engines and web viewers.
    // Uses the glTFast package (com.unity.cloud.gltfast, Apache 2.0 license).
    public class GlbExporter : MonoBehaviour
    {
        // Called at runtime (e.g. from a UI button) to gather all strokes in the
        // scene by their "TubeStroke" tag and export them.
        // The underscore discards the Task return value because we are intentionally
        // fire-and-forget here - the headset UI does not need to await the result.

        public void ExportAllStrokes()
        {
            GameObject[] strokes = GameObject.FindGameObjectsWithTag("TubeStroke");
            _ = ExportAsync(strokes);
        }

        // Exports the given array of GameObjects to a GLB file.
        // Separated from ExportAllStrokes so tests can pass strokes in directly
        // without needing the "TubeStroke" tag registered in the project.
        public void ExportStrokes(GameObject[] strokes)
        {
            _ = ExportAsync(strokes);
        }

        // Does the actual GLB export asynchronously using glTFast.
        // async/await is needed because glTFast's SaveToFileAndDispose is async -
        // it may need to wait for texture compression or other GPU work to finish.
        private async System.Threading.Tasks.Task ExportAsync(GameObject[] strokes)
        {
            if (strokes.Length == 0)
            {
                Debug.Log("No strokes to export.");
                return;
            }

            // Build a timestamped filename so each export is unique.
            string timestamp = System.DateTime.Now.ToString(
                "yyyy-MM-dd_HH-mm-ss",
                CultureInfo.InvariantCulture
            );
            // persistentDataPath is the one folder Android always lets us write to.
            string path = Path.Combine(Application.persistentDataPath, $"drawing_{timestamp}.glb");

            // GltfFormat.Binary produces a single .glb file (geometry + textures combined).
            // GltfFormat.Json would produce a .gltf + separate .bin + texture files instead.
            var exportSettings = new ExportSettings { Format = GltfFormat.Binary };

            var export = new GameObjectExport(exportSettings);

            // AddScene registers which GameObjects to include in the export.
            export.AddScene(strokes);

            // SaveToFileAndDispose writes the file and frees all internal buffers.
            bool success = await export.SaveToFileAndDispose(path);

            if (success)
            {
                Debug.Log($"GLB exported to: {path}");
            }
            else
            {
                Debug.LogError("GLB export failed.");
            }
        }
    }
}

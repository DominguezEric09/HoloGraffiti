using System;
using System.Threading.Tasks;
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
        public void ExportAllStrokes()
        {
            GameObject[] strokes = GameObject.FindGameObjectsWithTag("TubeStroke");
            ExportStrokes(strokes);
        }

        // Exports the given array of GameObjects to a GLB file without waiting for
        // it to finish - the headset UI does not need the result.
        // Separated from ExportAllStrokes so tests can pass strokes in directly
        // without needing the "TubeStroke" tag registered in the project.
        public void ExportStrokes(GameObject[] strokes)
        {
            _ = ExportAndLogErrorsAsync(strokes);
        }

        // Nobody awaits a fire-and-forget task, so an exception inside it would
        // vanish silently. Catch it here and log it instead.
        private async Task ExportAndLogErrorsAsync(GameObject[] strokes)
        {
            try
            {
                await ExportStrokesAsync(strokes);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// Does the actual GLB export asynchronously using glTFast and returns the
        /// file's path, or null if nothing was exported.
        /// async/await is needed because glTFast's SaveToFileAndDispose is async -
        /// it may need to wait for texture compression or other GPU work to finish.
        /// directory is where the file goes; leave it out to use persistentDataPath.
        public async Task<string> ExportStrokesAsync(GameObject[] strokes, string directory = null)
        {
            if (strokes == null || strokes.Length == 0)
            {
                Debug.Log("No strokes to export.");
                return null;
            }

            string path = ExportFile.NewPath(directory, "glb");

            // GltfFormat.Binary produces a single .glb file (geometry + textures combined).
            // GltfFormat.Json would produce a .gltf + separate .bin + texture files instead.
            var exportSettings = new ExportSettings { Format = GltfFormat.Binary };

            var export = new GameObjectExport(exportSettings);

            // AddScene registers which GameObjects to include in the export.
            export.AddScene(strokes);

            // SaveToFileAndDispose writes the file and frees all internal buffers.
            bool success = await export.SaveToFileAndDispose(path);

            if (!success)
            {
                Debug.LogError("GLB export failed.");
                return null;
            }

            Debug.Log($"GLB exported to: {path}");
            return path;
        }
    }
}

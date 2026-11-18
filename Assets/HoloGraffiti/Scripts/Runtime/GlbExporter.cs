using UnityEngine;
using System.IO;
using System.Globalization;
using GLTFast;
using GLTFast.Export;

namespace HoloGraffiti
{
    /// <summary>
    /// Exports all tagged stroke GameObjects in the scene to a GLB file.
    /// GLB is a binary version of the glTF format, widely supported by game
    /// engines and 3D tools.
    ///
    /// This script uses the glTFast package (com.unity.cloud.gltfast).
    /// That package must be added to Packages/manifest.json and referenced
    /// in HoloGraffiti.Runtime.asmdef before this script will compile.
    ///
    /// ExportAllStrokes() finds strokes by the TubeStroke tag.
    /// ExportStrokes(GameObject[] strokes) is the testable version.
    /// </summary>
    public class GlbExporter : MonoBehaviour
    {
        /// <summary>
        /// Finds all GameObjects tagged TubeStroke and exports them to a GLB file.
        /// Called by the Export button on the hand menu.
        /// </summary>
        public void ExportAllStrokes()
        {
            GameObject[] strokes = GameObject.FindGameObjectsWithTag("TubeStroke");
            _ = ExportAsync(strokes);
        }

        /// <summary>
        /// Exports the given array of GameObjects to a GLB file.
        /// </summary>
        public void ExportStrokes(GameObject[] strokes)
        {
            _ = ExportAsync(strokes);
        }

        /// <summary>
        /// Async method that writes the GLB file to the app's persistent data folder.
        /// Uses glTFast's GameObjectExport to convert Unity GameObjects to glTF format.
        /// </summary>
        private async System.Threading.Tasks.Task ExportAsync(GameObject[] strokes)
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
                $"drawing_{timestamp}.glb"
            );
            var exportSettings = new ExportSettings { Format = GltfFormat.Binary };
            var export = new GameObjectExport(exportSettings);
            export.AddScene(strokes);
            bool success = await export.SaveToFileAndDispose(path);
            if (success)
                Debug.Log($"GLB exported to: {path}");
            else
                Debug.LogError("GLB export failed.");
        }
    }
}
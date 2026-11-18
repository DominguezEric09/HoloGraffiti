using UnityEngine;

namespace HoloGraffiti
{
    /// Controls the hand menu that appears when the user holds up their palm.
    /// Attach this script to the HandMenu GameObject in the scene.
    ///
    /// Wire up the two buttons in the Inspector:
    ///   - Export Button: calls ExportAllStrokes on both exporters
    ///   - Clear Button: destroys all tagged stroke GameObjects
    /// The ObjExporter and GlbExporter components should be on the same
    /// GameObject as this script, or assigned in the Inspector.
    public class HandMenuController : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The ObjExporter component that writes .obj files to app storage.")]
        private ObjExporter objExporter;

        [SerializeField]
        [Tooltip("The GlbExporter component that writes .glb files to app storage.")]
        private GlbExporter glbExporter;

        /// Called by the Export poke button.
        /// Exports all current strokes to both OBJ and GLB format.
        public void OnExportPressed()
        {
            if (objExporter != null)
                objExporter.ExportAllStrokes();
            else
                Debug.LogWarning("HandMenuController: ObjExporter is not assigned.");

            if (glbExporter != null)
                glbExporter.ExportAllStrokes();
            else
                Debug.LogWarning("HandMenuController: GlbExporter is not assigned.");
        }

        /// Called by the Clear poke button.
        /// Destroys all GameObjects tagged TubeStroke in the current scene.
        public void OnClearPressed()
        {
            GameObject[] strokes = GameObject.FindGameObjectsWithTag("TubeStroke");
            foreach (GameObject stroke in strokes)
            {
                Destroy(stroke);
            }
            Debug.Log($"HandMenuController: Cleared {strokes.Length} stroke(s).");
        }
    }
}
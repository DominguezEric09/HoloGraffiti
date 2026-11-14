using UnityEngine;

namespace HoloGraffiti
{
    // Removes every finished stroke from the scene.
    // On the headset, press the menu button on the left controller, or do the
    // left-hand menu gesture (look at your left palm, then pinch and hold).
    // The hand menu's Clear button can call ClearAllStrokes too.
    public class StrokeClearer : MonoBehaviour
    {
        [Tooltip(
            "Clear when the left controller's menu button or the left-hand menu gesture is used."
        )]
        [SerializeField]
        private bool clearOnMenuButton = true;

        private void Update()
        {
            if (!clearOnMenuButton)
                return;

            // The left Touch controller's menu button and the left-hand menu gesture
            // both report as Button.Start, so one check covers controllers and hands.
            if (
                OVRInput.GetDown(OVRInput.Button.Start, OVRInput.Controller.Touch)
                || OVRInput.GetDown(OVRInput.Button.Start, OVRInput.Controller.Hands)
            )
            {
                ClearAllStrokes();
            }
        }

        // Finds every stroke by the "TubeStroke" tag TubeMeshGenerator gives it.
        // Only runs when the user clears, so the array this allocates doesn't matter.
        public void ClearAllStrokes()
        {
            int cleared = Clear(GameObject.FindGameObjectsWithTag("TubeStroke"));
            Debug.Log($"Cleared {cleared} strokes.");
        }

        /// Destroys the given strokes and their meshes, and returns how many it removed.
        /// Separated from ClearAllStrokes so tests can pass strokes in directly.
        public static int Clear(GameObject[] strokes)
        {
            int cleared = 0;
            foreach (GameObject stroke in strokes)
            {
                if (stroke == null)
                    continue;

                // Each stroke owns a mesh TubeMeshGenerator built just for it.
                // Destroying the GameObject doesn't free that mesh, so destroy it too,
                // or every cleared stroke would keep using memory.
                MeshFilter meshFilter = stroke.GetComponent<MeshFilter>();
                if (meshFilter != null)
                    DestroyObject(meshFilter.sharedMesh);

                DestroyObject(stroke);
                cleared++;
            }

            return cleared;
        }

        // Destroy waits until the end of the frame and only works in Play mode;
        // EditMode tests need DestroyImmediate.
        private static void DestroyObject(Object obj)
        {
            if (obj == null)
                return;

            if (Application.isPlaying)
                Destroy(obj);
            else
                DestroyImmediate(obj);
        }
    }
}

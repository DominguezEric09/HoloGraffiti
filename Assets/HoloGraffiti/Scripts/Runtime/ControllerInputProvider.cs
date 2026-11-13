using UnityEngine;

namespace HoloGraffiti.Interactions
{
    public class ControllerInputProvider : MonoBehaviour, IDrawingInputProvider
    {
        [Header("Controller Settings")]
        [Tooltip("Which Touch controller draws: RTouch (right) or LTouch (left).")]
        [SerializeField]
        private OVRInput.Controller controller = OVRInput.Controller.RTouch;

        [Range(0.0f, 1.0f)]
        [SerializeField]
        private float triggerThreshold = 0.1f;

        [Tooltip(
            "The camera rig's anchor for this controller (RightControllerAnchor or LeftControllerAnchor). The rig moves it every frame, so strokes line up with the controller you see."
        )]
        [SerializeField]
        private Transform controllerAnchor;

        [Header("Position Offset")]
        [Tooltip("Where the drawing point sits relative to the controller, in meters.")]
        [SerializeField]
        private Vector3 localPositionOffset = new Vector3(0f, 0f, 0.05f);

        private void Awake()
        {
            if (controllerAnchor == null)
            {
                Debug.LogError(
                    "[ControllerInputProvider] Assign the camera rig's controller anchor to Controller Anchor."
                );
            }
        }

        public bool IsDrawing
        {
            get
            {
                if (controllerAnchor == null || !OVRInput.IsControllerConnected(controller))
                {
                    return false;
                }

                return OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controller)
                    >= triggerThreshold;
            }
        }

        public Vector3 GetDrawingWorldPosition()
        {
            if (controllerAnchor == null)
            {
                return transform.position;
            }

            // TransformPoint applies the controller's rotation too, so the offset
            // stays in front of the controller however it is held
            return controllerAnchor.TransformPoint(localPositionOffset);
        }
    }
}

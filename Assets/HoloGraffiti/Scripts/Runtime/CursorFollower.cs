using HoloGraffiti.Interactions;
using UnityEngine;

namespace HoloGraffiti.Core
{
    // Runs after the input providers have updated this frame
    [DefaultExecutionOrder(100)]
    public class CursorFollower : MonoBehaviour
    {
        [Header("Input Reference")]
        [Tooltip(
            "Any component implementing IDrawingInputProvider (PinchDetector or ControllerInputProvider)"
        )]
        [SerializeField]
        private MonoBehaviour inputProviderComponent;

        private IDrawingInputProvider inputProvider;

        private void Awake()
        {
            if (inputProviderComponent is IDrawingInputProvider provider)
            {
                inputProvider = provider;
            }
            else
            {
                Debug.LogError(
                    "[CursorFollower] Assigned Input Provider does not implement IDrawingInputProvider!"
                );
            }
        }

        private void Update()
        {
            if (inputProvider == null)
                return;

            // Follow the drawing point continuously, whether or not a stroke is active
            transform.position = inputProvider.GetDrawingWorldPosition();
        }
    }
}

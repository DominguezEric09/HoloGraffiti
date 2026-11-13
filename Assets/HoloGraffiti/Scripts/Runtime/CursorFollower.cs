using UnityEngine;
using HoloGraffiti.Interactions;

namespace HoloGraffiti.Core
{
    public class CursorFollower : MonoBehaviour
    {
        [Header("Input Reference")]
        [SerializeField] private MonoBehaviour inputProviderComponent;

        private IDrawingInputProvider inputProvider;

        private void Awake()
        {
            if (inputProviderComponent is IDrawingInputProvider provider)
            {
                inputProvider = provider;
            }
            else
            {
                Debug.LogError("[CursorFollower] Assigned Input Provider does not implement IDrawingInputProvider!");
            }
        }

        private void Update()
        {
            //if (inputProvider == null) return;
            // Update cursor position continuously regardless of whether drawing is active
            //transform.position = inputProvider.GetDrawingWorldPosition();
            //Debug.Log($"Cursor target position: {inputProvider.GetDrawingWorldPosition()}");

            if (inputProvider == null) return;
            Vector3 targetPos = inputProvider.GetDrawingWorldPosition();
            Debug.Log($"Applying position to cursor: {targetPos}");
            transform.position = targetPos;
        }
    }
}
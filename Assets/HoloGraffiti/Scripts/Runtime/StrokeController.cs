using UnityEngine;

namespace HoloGraffiti.Interactions
{
    public class StrokeController : MonoBehaviour
    {
        [Header("Input Setup")]
        [Tooltip("Attach any component implementing IDrawingInputProvider (e.g., MouseInputProvider or HandPinchInputProvider)")]
        [SerializeField] private MonoBehaviour inputProviderComponent;

        [Header("Stroke Settings")]
        [SerializeField] private float minDistanceBetweenPoints = 0.02f;
        [SerializeField] private float strokeWidth = 0.04f;
        [SerializeField] private Material defaultMaterial;

        private IDrawingInputProvider inputProvider;
        private LineRenderer currentLineRenderer;
        private Vector3 lastPointPosition;
        private bool wasDrawingLastFrame;

        private void Awake()
        {
            if (inputProviderComponent is IDrawingInputProvider provider)
            {
                inputProvider = provider;
            }
            else
            {
                Debug.LogError("[StrokeController] Assigned Input Provider does not implement IDrawingInputProvider!");
            }
        }

        private void Update()
        {
            if (inputProvider == null) return;

            bool isDrawing = inputProvider.IsDrawing;
            Vector3 currentPos = inputProvider.GetDrawingWorldPosition();

            // Pinch / Click Started
            if (isDrawing && !wasDrawingLastFrame)
            {
                StartNewStroke(currentPos);
            }
            // Pinch / Click Held
            else if (isDrawing && wasDrawingLastFrame)
            {
                UpdateCurrentStroke(currentPos);
            }
            // Pinch / Click Released
            else if (!isDrawing && wasDrawingLastFrame)
            {
                EndStroke();
            }

            wasDrawingLastFrame = isDrawing;
        }

        private void StartNewStroke(Vector3 startPos)
        {
            GameObject strokeObj = new GameObject("Stroke_Line");
            strokeObj.transform.SetParent(transform);

            currentLineRenderer = strokeObj.AddComponent<LineRenderer>();
            currentLineRenderer.startWidth = strokeWidth;
            currentLineRenderer.endWidth = strokeWidth;
            currentLineRenderer.positionCount = 0;
            currentLineRenderer.useWorldSpace = true;

            if (defaultMaterial != null)
            {
                currentLineRenderer.material = defaultMaterial;
            }

            AddPointToStroke(startPos);
            lastPointPosition = startPos;
        }

        private void UpdateCurrentStroke(Vector3 currentPos)
        {
            if (currentLineRenderer == null) return;

            if (Vector3.Distance(currentPos, lastPointPosition) >= minDistanceBetweenPoints)
            {
                AddPointToStroke(currentPos);
                lastPointPosition = currentPos;
            }
        }

        private void EndStroke()
        {
            currentLineRenderer = null;
        }

        private void AddPointToStroke(Vector3 position)
        {
            int index = currentLineRenderer.positionCount;
            currentLineRenderer.positionCount++;
            currentLineRenderer.SetPosition(index, position);
        }
    }
}
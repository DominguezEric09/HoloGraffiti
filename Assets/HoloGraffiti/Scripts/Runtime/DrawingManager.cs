using System.Collections.Generic; // Gives us access to List<> for storing stroke points
using HoloGraffiti.Interactions; // IDrawingInputProvider — where the pinch or trigger comes from
using UnityEngine; // Basic Unity functions — Vector3, GameObject, MonoBehaviour, etc.

namespace HoloGraffiti
{
    // Runs after the input providers have updated this frame,
    // so a stroke follows the hand without a one-frame delay
    [DefaultExecutionOrder(100)]
    public class DrawingManager : MonoBehaviour
    {
        // INPUT
        [Header("Input")]
        [Tooltip(
            "Any component implementing IDrawingInputProvider (PinchDetector or ControllerInputProvider)"
        )]
        [SerializeField]
        private MonoBehaviour inputProviderComponent;

        // Unity can't show an interface in the Inspector, so we take a component
        // and check in Awake that it really is an input provider

        // SETTINGS
        [Header("Drawing Settings")]
        public float minDistance = 0.01f;

        // The minimum distance (in meters) the fingertip must move
        // before we record a new point.
        // Too small = too many points, jittery lines
        // Too large = misses small movements, feels unresponsive
        // 0.01 = 1cm gap between each recorded point

        public Material strokeMaterial;

        // The material used for both the preview line and the finished tube
        // Assigned in the Inspector and shared by every stroke so they can be batched

        //PRIVATE VARIABLES

        private bool isDrawing = false;

        // A true/false flag that tracks whether the user is currently drawing
        // false = not drawing, true = actively drawing
        // Controls which method gets called in HandleDrawing()

        private LineRenderer currentStroke;

        // Holds the LineRenderer component that shows the preview line
        // while the user is actively drawing.
        // This is temporary — it gets destroyed when the stroke ends
        // and replaced by the permanent tube mesh

        private List<Vector3> currentPoints = new List<Vector3>();

        // A list of 3D positions (x, y, z coordinates) recorded during a stroke
        // Every time the fingertip moves far enough, its position gets added here
        // This is the real data — it survives after the LineRenderer is destroyed
        // and gets passed to TubeMeshGenerator to build the permanent tube

        private IDrawingInputProvider inputProvider;

        // The input source this manager draws from (one hand or one controller)

        private Transform cameraTransform;

        // The headset camera, looked up once and reused for the smoothing below

        // AWAKE
        void Awake()
        // Runs once when the scene loads
        {
            inputProvider = inputProviderComponent as IDrawingInputProvider;
            if (inputProvider == null)
                Debug.LogError(
                    "[DrawingManager] Assign a component that implements IDrawingInputProvider to Input Provider Component."
                );

            if (strokeMaterial == null)
                Debug.LogError("[DrawingManager] Assign the stroke material.");
        }

        // UPDATE
        void Update()
        // Runs every single frame
        {
            if (TryGetDrawInput(out Vector3 tipPosition, out bool pinching))
            {
                HandleDrawing(tipPosition, pinching);
            }
        }

        // ON DISABLE
        void OnDisable()
        // If this object is switched off mid-stroke, finish the stroke
        // instead of leaving a preview line behind
        {
            if (isDrawing)
                EndStroke();
        }

        // GET DRAW INPUT
        bool TryGetDrawInput(out Vector3 tipPosition, out bool pinching)
        // Asks the input provider where the drawing point is and whether
        // the user is pinching (or holding the trigger) right now
        {
            if (inputProvider == null)
            {
                tipPosition = Vector3.zero;
                pinching = false;
                return false;
            }

            pinching = inputProvider.IsDrawing;
            tipPosition = inputProvider.GetDrawingWorldPosition();
            return true;
        }

        // ── HANDLE DRAWING ──
        void HandleDrawing(Vector3 tipPosition, bool pinching)
        // Called every frame with the current fingertip position
        // Decides whether to start, continue, or end a stroke
        {
            if (pinching && !isDrawing)
                // Pinch just started and we weren't already drawing
                // Start a brand new stroke
                StartStroke(tipPosition);
            else if (pinching && isDrawing)
                // Pinch is held and we're already drawing
                // Keep extending the current stroke
                ContinueStroke(tipPosition);
            else if (!pinching && isDrawing)
                // Pinch was released and we were drawing
                // Finish the stroke and build the tube mesh
                EndStroke();
        }

        // START STROKE
        void StartStroke(Vector3 position)
        // Called when the user first pinches to begin drawing
        {
            isDrawing = true; // Set the drawing flag to true

            currentPoints.Clear(); // Wipe any leftover points from the previous stroke
            currentPoints.Add(position); // Add the very first point — where the pinch started

            // Create a new empty GameObject to hold the preview line
            GameObject obj = new GameObject("Stroke");
            currentStroke = obj.AddComponent<LineRenderer>();
            // LineRenderer is a Unity component that draws a line between a series of points
            // This is the temporary visual you see while drawing

            // Set the line width (0.008 = 8mm)
            currentStroke.startWidth = 0.008f;
            currentStroke.endWidth = 0.008f;

            // Round the corners and end caps so the line looks smooth
            currentStroke.numCornerVertices = 8;
            currentStroke.numCapVertices = 8;

            // Always face the camera so the line is always visible
            currentStroke.alignment = LineAlignment.View;

            // Place the first point of the preview line at the fingertip
            currentStroke.positionCount = 1;
            currentStroke.SetPosition(0, position);

            // Use the shared stroke material for the preview line
            currentStroke.sharedMaterial = strokeMaterial;

            // Position the line in real 3D world space
            currentStroke.useWorldSpace = true;
        }

        // CONTINUE STROKE
        void ContinueStroke(Vector3 position)
        // Called every frame while the user is holding the pinch and moving
        {
            // Hard cap — never record more than 500 points per stroke
            // Prevents performance issues with very long strokes
            if (currentPoints.Count >= 500)
                return;

            if (Vector3.Distance(position, currentPoints[currentPoints.Count - 1]) > minDistance)
            // Only record a new point if the fingertip has moved
            // farther than minDistance from the last recorded point
            // This filters out micro-jitter when the hand is nearly still
            {
                // Adaptive smoothing based on distance from camera
                // Close to camera = more smoothing (hand tracking is less accurate up close)
                // Far from camera = less smoothing (more responsive)
                if (cameraTransform == null && Camera.main != null)
                    cameraTransform = Camera.main.transform;

                // With no camera in the scene, fall back to the middle of the range
                float distanceFromCamera =
                    cameraTransform != null
                        ? Vector3.Distance(position, cameraTransform.position)
                        : 0.25f;
                float smoothFactor = Mathf.Clamp(distanceFromCamera * 2f, 0.3f, 0.7f);
                // Mathf.Clamp keeps the value between 0.3 and 0.7

                // Lerp blends between the last recorded point and the new position
                // smoothFactor controls how far toward the new position we move
                // 0.3 = move 30% of the way (smoother, more lag)
                // 0.7 = move 70% of the way (more responsive, less smooth)
                Vector3 smoothed = Vector3.Lerp(
                    currentPoints[currentPoints.Count - 1],
                    position,
                    smoothFactor
                );

                // Add the smoothed position to our list of stroke points
                currentPoints.Add(smoothed);

                // Extend the preview LineRenderer by the new point
                currentStroke.positionCount = currentPoints.Count;
                currentStroke.SetPosition(currentPoints.Count - 1, smoothed);
            }
        }

        // END STROKE
        void EndStroke()
        // Called when the user releases the pinch to finish drawing
        {
            isDrawing = false; // Reset the drawing flag

            // Destroy the temporary preview LineRenderer
            // It's no longer needed once the stroke is complete
            if (currentStroke != null)
                Destroy(currentStroke.gameObject);

            currentStroke = null; // Clear the reference

            // If we recorded at least 2 points, build the permanent tube mesh
            // (Need at least 2 points to draw a line between them)
            if (currentPoints.Count >= 2)
                TubeMeshGenerator.CreateTube(currentPoints, strokeMaterial);
            // This hands the recorded points to TubeMeshGenerator
            // which builds the solid 3D tube and adds it to the scene
        }
    }
}

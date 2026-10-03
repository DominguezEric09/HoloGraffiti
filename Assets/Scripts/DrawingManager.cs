using UnityEngine; // Basic Unity functions — Vector3, GameObject, MonoBehaviour, etc.
using System.Collections.Generic; // Gives us access to List<> for storing stroke points
using MixedReality.Toolkit; // MRTK3 core tools — XRSubsystemHelpers, TrackedHandJoint, HandJointPose
using MixedReality.Toolkit.Subsystems; // Gives us HandsAggregatorSubsystem — the hand tracking system
using UnityEngine.XR; // Gives us XRNode — used to specify left hand vs right hand

public class DrawingManager : MonoBehaviour
{
    // SETTINGS
    [Header("Drawing Settings")]
    public float minDistance = 0.01f;
    // The minimum distance (in meters) the fingertip must move
    // before we record a new point.
    // Too small = too many points, jittery lines
    // Too large = misses small movements, feels unresponsive
    // 0.01 = 1cm gap between each recorded point

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

    private HandsAggregatorSubsystem handsAggregator;
    // A reference to MRTK3's hand tracking system
    // Starts as null — gets filled in Start()
    // Used every frame in Update() to get fingertip positions

    // START
    void Start()
    // Runs once when the scene loads
    {
        // Tell the HoloLens to use its depth sensor to correctly
        // place holograms relative to real world surfaces
        UnityEngine.XR.XRSettings.useOcclusionMesh = true;

        // Find and store the MRTK3 hand tracking system
        // GetFirstRunningSubsystem searches for an active HandsAggregatorSubsystem
        // and returns it so we can use it every frame
        handsAggregator = XRSubsystemHelpers.GetFirstRunningSubsystem<HandsAggregatorSubsystem>();

        // Cap the frame rate at 60 FPS for consistent performance
        // Without this, the frame rate fluctuates which causes jitter in drawing
        Application.targetFrameRate = 60;

        // Disable vSync so the frame rate cap takes full effect
        QualitySettings.vSyncCount = 0;

        // Set the physics update rate to match 60 FPS
        Time.fixedDeltaTime = 1f / 60f;
    }

    // UPDATE
    void Update()
    // Runs every single frame — approximately 60 times per second
    {
        // If hand tracking isn't ready yet, try to find it again
        // and skip the rest of Update for this frame
        if (handsAggregator == null)
        {
            handsAggregator = XRSubsystemHelpers.GetFirstRunningSubsystem<HandsAggregatorSubsystem>();
            return; // Exit Update early — nothing else can run without hand tracking
        }

        // Try to get the right hand index fingertip position
        // TryGetJoint asks MRTK3: "where is this specific joint right now?"
        // TrackedHandJoint.IndexTip = the very tip of the index finger
        // XRNode.RightHand = we want the right hand
        // out HandJointPose jointPose = stores the position and rotation of the joint
        // Returns true if the joint was found, false if the hand isn't visible
        bool gotJoint = handsAggregator.TryGetJoint(
            TrackedHandJoint.IndexTip,
            XRNode.RightHand,
            out HandJointPose jointPose
        );

        // If right hand not found, try the left hand
        // This gives us both-hand support
        // Whichever hand is detected first is the one that draws
        if (!gotJoint)
        {
            gotJoint = handsAggregator.TryGetJoint(
                TrackedHandJoint.IndexTip,
                XRNode.LeftHand,
                out jointPose
            );
        }

        // If we successfully found a fingertip position on either hand,
        // pass it to HandleDrawing to decide what to do with it
        if (gotJoint)
        {
            HandleDrawing(jointPose.Position);
        }
    }

    // ── HANDLE DRAWING ──
    void HandleDrawing(Vector3 tipPosition)
    // Called every frame with the current fingertip position
    // Decides whether to start, continue, or end a stroke
    {
        bool pinching = IsPinching();
        // Check if the user is pinching right now (true or false)

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

    // IS PINCHING
    bool IsPinching()
    // Returns true if the user is pinching, false if not
    // We detect pinching manually by measuring the distance
    // between the index fingertip and thumb tip
    // MRTK3's built-in pinch detection (TryGetPinchProgress) was broken
    // in version 4.0.0-pre.1 — it always returned false
    // so we wrote our own replacement
    {
        if (handsAggregator == null) return false;

        // Get the right hand index fingertip position
        bool gotIndex = handsAggregator.TryGetJoint(
            TrackedHandJoint.IndexTip,
            XRNode.RightHand,
            out HandJointPose indexPose
        );

        // Get the right hand thumb tip position
        bool gotThumb = handsAggregator.TryGetJoint(
            TrackedHandJoint.ThumbTip,
            XRNode.RightHand,
            out HandJointPose thumbPose
        );

        // If right hand joints weren't found, try the left hand
        if (!gotIndex || !gotThumb)
        {
            gotIndex = handsAggregator.TryGetJoint(
                TrackedHandJoint.IndexTip,
                XRNode.LeftHand,
                out indexPose
            );

            gotThumb = handsAggregator.TryGetJoint(
                TrackedHandJoint.ThumbTip,
                XRNode.LeftHand,
                out thumbPose
            );
        }

        // If we still couldn't find both joints, can't detect a pinch
        if (!gotIndex || !gotThumb) return false;

        // Measure the distance between index tip and thumb tip
        float distance = Vector3.Distance(indexPose.Position, thumbPose.Position);

        // If fingers are closer than 3cm (0.03 meters) = pinching
        // If farther apart = not pinching
        return distance < 0.03f;
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

        // Use a simple sprite shader for the preview line
        // MRTK shader doesn't work well on LineRenderers
        currentStroke.material = new Material(Shader.Find("Sprites/Default"));
        currentStroke.material.color = UnityEngine.Color.white;

        // Position the line in real 3D world space
        currentStroke.useWorldSpace = true;
    }

    // CONTINUE STROKE 
    void ContinueStroke(Vector3 position)
    // Called every frame while the user is holding the pinch and moving
    {
        // Hard cap — never record more than 500 points per stroke
        // Prevents performance issues with very long strokes
        if (currentPoints.Count >= 500) return;

        if (Vector3.Distance(position,
            currentPoints[currentPoints.Count - 1]) > minDistance)
        // Only record a new point if the fingertip has moved
        // farther than minDistance from the last recorded point
        // This filters out micro-jitter when the hand is nearly still
        {
            // Adaptive smoothing based on distance from camera
            // Close to camera = more smoothing (hand tracking is less accurate up close)
            // Far from camera = less smoothing (more responsive)
            float distanceFromCamera = Vector3.Distance(position, Camera.main.transform.position);
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

            // Update the preview LineRenderer to show the extended line
            currentStroke.positionCount = currentPoints.Count;
            currentStroke.SetPositions(currentPoints.ToArray());
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
            TubeMeshGenerator.CreateTube(currentPoints);
        // This hands the recorded points to TubeMeshGenerator
        // which builds the solid 3D tube and adds it to the scene
    }
}
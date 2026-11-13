using System;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using UnityEngine;

namespace HoloGraffiti.Interactions
{
    // Runs after the Interaction SDK has updated the hand this frame (order 0)
    // and before DrawingManager reads it (order 100).
    [DefaultExecutionOrder(50)]
    public class PinchDetector : MonoBehaviour, IDrawingInputProvider
    {
        [Header("Hand Reference")]
        [Tooltip(
            "The Interaction SDK Hand to watch (left or right). It lives on the OVRHands object inside the interaction rig."
        )]
        [SerializeField, Interface(typeof(IHand))]
        private UnityEngine.Object targetHand;

        [Header("Pinch Settings")]
        [Range(0.1f, 1.0f)]
        [Tooltip("Pinch strength (0 to 1) needed to start a pinch.")]
        [SerializeField]
        private float pinchThreshold = 0.8f;

        [Range(0.0f, 1.0f)]
        [Tooltip(
            "Pinch strength (0 to 1) below which a pinch ends. Keep it lower than the start threshold so a pinch doesn't flicker when the fingers hover near the cutoff."
        )]
        [SerializeField]
        private float releaseThreshold = 0.5f;

        [Tooltip(
            "Seconds a pinch is kept alive after hand tracking drops out, so a brief glitch doesn't break a stroke."
        )]
        [SerializeField]
        private float trackingLossGrace = 0.2f;

        // Public properties for state checking
        public bool IsPinched { get; private set; }
        public float CurrentPinchStrength { get; private set; }

        // The point between the thumb tip and the index fingertip, in world space
        public Vector3 PinchPosition { get; private set; }

        // Events for other systems (drawing, UI, etc.) to subscribe to
        public event Action OnPinchStarted;
        public event Action OnPinchEnded;
        public event Action<float> OnPinchStayed; // Passes pinch strength

        // IDrawingInputProvider, so a pinch can drive DrawingManager directly
        public bool IsDrawing => IsPinched;

        public Vector3 GetDrawingWorldPosition() => PinchPosition;

        private IHand hand;
        private float lastTrackedTime;

        private void Awake()
        {
            hand = targetHand as IHand;
            if (hand == null)
            {
                Debug.LogError("[PinchDetector] Assign an Interaction SDK Hand to Target Hand.");
            }

            PinchPosition = transform.position;
        }

        private void Update()
        {
            if (hand == null)
            {
                return;
            }

            if (!hand.IsTrackedDataValid)
            {
                HandleTrackingLost();
                return;
            }

            lastTrackedTime = Time.time;
            CurrentPinchStrength = hand.GetFingerPinchStrength(HandFinger.Index);

            if (
                hand.GetJointPose(HandJointId.HandThumbTip, out Pose thumbTip)
                && hand.GetJointPose(HandJointId.HandIndexTip, out Pose indexTip)
            )
            {
                PinchPosition = (thumbTip.position + indexTip.position) * 0.5f;
            }

            bool wasPinched = IsPinched;
            IsPinched = NextPinchState(
                wasPinched,
                CurrentPinchStrength,
                pinchThreshold,
                releaseThreshold
            );

            if (!wasPinched && IsPinched)
            {
                OnPinchStarted?.Invoke();
            }
            else if (wasPinched && !IsPinched)
            {
                OnPinchEnded?.Invoke();
            }
            else if (IsPinched)
            {
                OnPinchStayed?.Invoke(CurrentPinchStrength);
            }
        }

        private void OnDisable()
        {
            EndPinch();
        }

        // Decides whether the hand counts as pinching this frame. Starting needs the
        // strength to reach startThreshold; once pinching, it only ends when the
        // strength falls below releaseThreshold. The gap between the two is what
        // stops a pinch from flickering on and off.
        public static bool NextPinchState(
            bool wasPinched,
            float strength,
            float startThreshold,
            float releaseThreshold
        )
        {
            // A release threshold above the start threshold would never let a pinch hold
            releaseThreshold = Mathf.Min(releaseThreshold, startThreshold);
            return wasPinched ? strength >= releaseThreshold : strength >= startThreshold;
        }

        private void HandleTrackingLost()
        {
            // Hold the pinch where it was for a moment, then let go
            if (IsPinched && Time.time - lastTrackedTime > trackingLossGrace)
            {
                EndPinch();
            }
        }

        private void EndPinch()
        {
            CurrentPinchStrength = 0f;
            if (!IsPinched)
            {
                return;
            }

            IsPinched = false;
            OnPinchEnded?.Invoke();
        }
    }
}

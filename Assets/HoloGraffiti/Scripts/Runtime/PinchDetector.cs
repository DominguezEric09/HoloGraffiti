using System;
using System.Reflection;
using UnityEngine;

namespace HoloGraffiti.Interactions
{
    public class PinchDetector : MonoBehaviour
    {
        [Header("Hand Reference")]
        [Tooltip("Assign the OVRHand component (Left or Right hand) from your hand tracking rig.")]
        [SerializeField]
        private MonoBehaviour targetHand;

        [Header("Pinch Settings")]
        [Range(0.1f, 1.0f)]
        [Tooltip("Strength threshold required to trigger a pinch (0 to 1).")]
        [SerializeField]
        private float pinchThreshold = 0.8f;

        // Public properties for state checking
        public bool IsPinched { get; private set; }
        public float CurrentPinchStrength { get; private set; }
        public Vector3 PinchPosition { get; private set; }

        // Events for other systems (drawing, UI, etc.) to subscribe to
        public event Action OnPinchStarted;
        public event Action OnPinchEnded;
        public event Action<float> OnPinchStayed; // Passes pinch strength

        private void Update()
        {
            if (targetHand == null || !IsHandTracked())
            {
                if (IsPinched)
                {
                    // Force release if hand is lost tracking
                    IsPinched = false;
                    CurrentPinchStrength = 0f;
                    InvokePinchEnded();
                }
                return;
            }

            // 1. Get pinch strength for the index finger (0 to 1)
            CurrentPinchStrength = GetIndexPinchStrength();

            // 2. Track index finger tip position in 3D world space (crucial for drawing!)
            // Note: OVRHand provides joint positions; fallback to transform position if unavailable
            PinchPosition = transform.position; // Default fallback

            // Update pinch state based on threshold
            bool wasPinched = IsPinched;
            IsPinched = CurrentPinchStrength >= pinchThreshold;

            // 3. Trigger events on state changes
            if (!wasPinched && IsPinched)
            {
                InvokePinchStarted();
            }
            else if (wasPinched && !IsPinched)
            {
                InvokePinchEnded();
            }
            else if (IsPinched)
            {
                OnPinchStayed?.Invoke(CurrentPinchStrength);
            }
        }

        private void InvokePinchStarted()
        {
            Debug.Log($"[PinchDetector] Hand: Pinch Started!");
            OnPinchStarted?.Invoke();
        }

        private void InvokePinchEnded()
        {
            Debug.Log($"[PinchDetector] Hand: Pinch Ended!");
            OnPinchEnded?.Invoke();
        }

        private bool IsHandTracked()
        {
            PropertyInfo trackedProperty = targetHand.GetType().GetProperty("IsTracked");
            return trackedProperty != null && (bool)trackedProperty.GetValue(targetHand);
        }

        private float GetIndexPinchStrength()
        {
            MethodInfo pinchMethod = targetHand.GetType().GetMethod("GetFingerPinchStrength");
            if (pinchMethod == null)
            {
                return 0f;
            }

            Type fingerType = pinchMethod.GetParameters()[0].ParameterType;
            object indexFinger = Enum.Parse(fingerType, "Index");
            return (float)pinchMethod.Invoke(targetHand, new[] { indexFinger });
        }
    }
}

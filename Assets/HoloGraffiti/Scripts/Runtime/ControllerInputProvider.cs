using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

namespace HoloGraffiti.Interactions
{
    public class ControllerInputProvider : MonoBehaviour, IDrawingInputProvider
    {
        [Header("Controller Settings")]
        [SerializeField] private XRNode controllerNode = XRNode.RightHand;
        [SerializeField] private float triggerThreshold = 0.1f;

        [Header("Position Offset")]
        [SerializeField] private Vector3 localPositionOffset = new Vector3(0f, 0f, 0.05f);

        private InputDevice targetDevice;

        private void Start()
        {
            InitializeDevice();
        }

        private void InitializeDevice()
        {
            List<InputDevice> devices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(controllerNode, devices);

            if (devices.Count > 0)
            {
                targetDevice = devices[0];
            }
        }

        public bool IsDrawing
        {
            get
            {
                if (!targetDevice.isValid)
                {
                    InitializeDevice();
                }

                if (targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
                {
                    return triggerValue >= triggerThreshold;
                }

                return false;
            }
        }

        public Vector3 GetDrawingWorldPosition()
        {
            if (!targetDevice.isValid)
            {
                InitializeDevice();
            }

            if (targetDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position))
            {
                Vector3 worldPos = transform.TransformPoint(position);
                return worldPos + transform.TransformDirection(localPositionOffset);
            }

            return transform.position + transform.TransformDirection(localPositionOffset);
        }
    }
}
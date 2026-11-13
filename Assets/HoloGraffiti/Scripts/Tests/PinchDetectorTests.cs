using HoloGraffiti.Interactions;
using NUnit.Framework;

namespace HoloGraffiti
{
    // EditMode tests for the pinch thresholds in PinchDetector.
    // The thresholds are plain math, so they can be tested without a headset.
    public class PinchDetectorTests
    {
        private const float Start = 0.8f;
        private const float Release = 0.5f;

        [Test]
        public void NextPinchState_NotPinching_StartsOnlyAtStartThreshold()
        {
            Assert.IsFalse(PinchDetector.NextPinchState(false, 0.79f, Start, Release));
            Assert.IsTrue(PinchDetector.NextPinchState(false, 0.8f, Start, Release));
        }

        [Test]
        public void NextPinchState_Pinching_HoldsBetweenTheTwoThresholds()
        {
            // 0.6 is too weak to start a pinch but strong enough to keep one going.
            // This gap is what stops a stroke from breaking when the fingers relax a little.
            Assert.IsFalse(PinchDetector.NextPinchState(false, 0.6f, Start, Release));
            Assert.IsTrue(PinchDetector.NextPinchState(true, 0.6f, Start, Release));
        }

        [Test]
        public void NextPinchState_Pinching_EndsBelowReleaseThreshold()
        {
            Assert.IsTrue(PinchDetector.NextPinchState(true, 0.5f, Start, Release));
            Assert.IsFalse(PinchDetector.NextPinchState(true, 0.49f, Start, Release));
        }

        [Test]
        public void NextPinchState_ReleaseAboveStart_BehavesLikeOneThreshold()
        {
            // A release threshold above the start threshold makes no sense,
            // so it is treated as equal to the start threshold.
            Assert.IsTrue(PinchDetector.NextPinchState(true, 0.8f, Start, 0.95f));
            Assert.IsFalse(PinchDetector.NextPinchState(true, 0.79f, Start, 0.95f));
        }
    }
}

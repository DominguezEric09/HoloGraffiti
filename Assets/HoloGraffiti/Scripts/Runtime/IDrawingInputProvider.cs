using UnityEngine;

namespace HoloGraffiti.Interactions
{
    public interface IDrawingInputProvider
    {
        /// <summary>
        /// True while the drawing trigger (mouse click or pinch) is active.
        /// </summary>
        bool IsDrawing { get; }

        /// <summary>
        /// Returns the current 3D world position of the drawing point (mouse point or index fingertip).
        /// </summary>
        Vector3 GetDrawingWorldPosition();
    }
}
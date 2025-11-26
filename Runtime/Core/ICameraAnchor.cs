using UnityEngine;

namespace InteractiveCameraSystem
{
    /// <summary>
    /// Minimal abstraction for a camera anchor/point-of-interest.
    /// Implementations can wrap existing components (e.g., VirtualCameraAnchor) or plain GameObjects.
    /// </summary>
    public interface ICameraAnchor
    {
        Transform AnchorTransform { get; }
        string Identifier { get; }
        bool IsValid { get; }
    }
}



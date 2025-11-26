using UnityEngine;

namespace InteractiveCameraSystem
{
    /// <summary>
    /// Settings for drag behavior in camera modes
    /// Based on VirtualCameraDragConfig with all the same settings
    /// </summary>
    [System.Serializable]
    public class DragSettings
    {
        [Header("Input Mapping")]
        [Tooltip("What happens when finger moves left/right. WorldX = lateral movement, RotationY = camera turns left/right like FPS")]
        public VirtualCameraAxis horizontalMapsTo = VirtualCameraAxis.WorldX;
        [Tooltip("How fast camera responds to horizontal finger movement. Range: 0.1-15. Recommended: 2-4 (responsive), 6-8 (mobile game), 1-2 (cinematic)")]
        [Range(0.1f, 15f)] public float horizontalSensitivity = 2f;
        [Tooltip("Reverse horizontal controls. Unchecked = finger right moves camera right. Checked = finger right moves camera left")]
        public bool invertHorizontal = false;
        
        [Tooltip("What happens when finger moves up/down. WorldZ = forward/back movement, RotationX = camera tilts up/down")]
        public VirtualCameraAxis verticalMapsTo = VirtualCameraAxis.WorldZ;
        [Tooltip("How fast camera responds to vertical finger movement. Range: 0.1-15. Recommended: 2-4 (responsive), 6-8 (mobile game), 1-2 (cinematic)")]
        [Range(0.1f, 15f)] public float verticalSensitivity = 2f;
        [Tooltip("Reverse vertical controls. Unchecked = finger up moves camera forward. Checked = finger up moves camera back")]
        public bool invertVertical = false;
        
        [Header("Movement Feel")]
        [Tooltip("How smooth camera movement feels DURING drag. Range: 0.1-10. Recommended: 1-2 (responsive), 1.5 (balanced), 3-5 (smooth)")]
        [Range(0.1f, 10f)] public float damping = 5f;
        
        [Header("Momentum System")]
        [Tooltip("Enable momentum after releasing finger. When disabled, camera stops instantly")]
        public bool enableMomentum = true;
        [Tooltip("How much velocity is preserved when finger is released. Range: 0-1. Recommended: 0.2-0.4 (subtle), 0.5-0.7 (moderate), 0.8+ (strong)")]
        [Range(0f, 1f)] public float momentum = 0.8f;
        [Tooltip("How quickly momentum decays after releasing finger. Range: 0.1-20. Recommended: 3-6 (natural), 8-12 (quick stop), 1-2 (long glide)")]
        [Range(0.1f, 20f)] public float momentumDecay = 2f;
        
        [Header("Speed Control")]
        [Tooltip("Maximum camera movement speed in units per second. Range: 10-2000. Recommended: 300-500 (controlled), 800+ (mobile game), 100-200 (cinematic)")]
        [Range(10f, 2000f)] public float maxCameraSpeed = 15f;
        
        [Header("Boundary Constraints")]
        [Tooltip("Enable camera boundary constraints. Prevents camera from moving outside defined areas")]
        public bool enableBoundary = false;
        
        [Header("Movement Delta Mode")]
        [Tooltip("How finger movement is converted to camera movement. Fixed to world plane via VirtualCameraBounds.")]
        public MovementDeltaMode movementDeltaMode = MovementDeltaMode.WorldPlaneViaBounds;
        
        [Header("Damping Mode")]
        [Tooltip("How damping is applied. Fixed to apply only after finger release (momentum phase).")]
        public DragDampingMode dragDampingMode = DragDampingMode.ReleaseOnly;
        
        #region Validation
        
        /// <summary>
        /// Check if these drag settings are valid
        /// </summary>
        /// <returns>True if the settings are valid</returns>
        public bool IsValid()
        {
            bool basicValid = horizontalSensitivity > 0f && verticalSensitivity > 0f && damping > 0f && maxCameraSpeed > 0f;
            
            if (!basicValid) return false;
            
            // Validate momentum parameters only if momentum is enabled
            if (enableMomentum)
            {
                return momentum >= 0f && momentum <= 1f && momentumDecay > 0f;
            }
            
            return true;
        }
        
        /// <summary>
        /// Get a summary of these drag settings
        /// </summary>
        /// <returns>String summary of the settings</returns>
        public string GetSummary()
        {
            string horizontal = horizontalMapsTo == VirtualCameraAxis.None ? "None" : horizontalMapsTo.ToString();
            string vertical = verticalMapsTo == VirtualCameraAxis.None ? "None" : verticalMapsTo.ToString();
            string momentumInfo = enableMomentum ? $"M:{momentum:F1}(D:{momentumDecay:F1})" : "NoMomentum";
            return $"Drag: H:{horizontal} V:{vertical} (Damp:{damping:F1}, {momentumInfo}, Max:{maxCameraSpeed:F1})";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Virtual camera axis types for drag input mapping
    /// </summary>
    public enum VirtualCameraAxis
    {
        None,
        WorldX,
        WorldY,
        WorldZ,
        RotationX,
        RotationY,
        RotationZ
    }
    
    /// <summary>
    /// Movement delta calculation modes
    /// </summary>
    public enum MovementDeltaMode
    {
        WorldPlaneViaBounds
    }
    
    /// <summary>
    /// Drag damping modes
    /// </summary>
    public enum DragDampingMode
    {
        ReleaseOnly
    }
}

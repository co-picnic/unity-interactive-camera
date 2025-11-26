using UnityEngine;
using Unity.Cinemachine;

namespace InteractiveCameraSystem
{
    /// <summary>
    /// ScriptableObject that defines a camera mode with all its settings and behaviors.
    /// This replaces the complex behavior system with a simple, unified configuration.
    /// </summary>
    [CreateAssetMenu(fileName = "CameraMode", menuName = "Camera/Camera Mode", order = 1)]
    public class CameraMode : ScriptableObject
    {
        [Header("Mode Information")]
        [Tooltip("Unique name for this camera mode")]
        public string modeName = "New Mode";
        
        [Tooltip("Description of what this mode is used for")]
        [TextArea(2, 4)]
        public string description = "";
        
        [Header("Cinemachine Camera")]
        [Tooltip("The CinemachineCamera that will be used for this mode")]
        public CinemachineCamera virtualCamera;
        
        [Tooltip("Mode ID for matching scene cameras (if empty, uses camera name). Recommended: use CameraModeIdentifier component on cameras.")]
        public string modeId;
        
        [Header("Camera Settings")]
        [Tooltip("Basic camera projection settings")]
        public CameraSettings cameraSettings = new CameraSettings();
        
        [Header("Follow Settings")]
        [Tooltip("Settings for following targets")]
        public FollowSettings followSettings = new FollowSettings();
        
        [Header("Drag Settings")]
        [Tooltip("Settings for drag input behavior")]
        public DragSettings dragSettings = new DragSettings();
        
        [Header("Zoom Settings")]
        [Tooltip("Settings for zoom input behavior")]
        public ZoomSettings zoomSettings = new ZoomSettings();
        
        [Header("Transition Settings")]
        [Tooltip("Settings for transitions to/from this mode")]
        public TransitionSettings transitionSettings = new TransitionSettings();
        
        [Header("Behavior Flags")]
        [Tooltip("Enable follow behavior")]
        public bool enableFollow = true;
        
        [Tooltip("Enable zoom behavior")]
        public bool enableZoom = true;
        
        [Tooltip("Enable drag behavior")]
        public bool enableDrag = true;
        
        [Header("Advanced Settings")]
        [Tooltip("Priority for this camera mode (higher = more important)")]
        [Range(0, 100)]
        public int priority = 50;
        
        [Tooltip("Whether this mode should be available in the mode list. Disabled modes are excluded from auto-discovery and auto-instantiation")]
        public bool isEnabled = true;
        
        [Tooltip("Tags for categorizing this mode")]
        public string[] tags = new string[0];
        
        #region Validation
        
        void OnValidate()
        {
            // Ensure mode name is not empty
            if (string.IsNullOrEmpty(modeName))
            {
                modeName = "New Mode";
            }
            
            // Ensure virtual camera is set
            if (virtualCamera == null)
            {
                Debug.LogWarning($"[CameraMode] '{modeName}' has no virtual camera assigned!");
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Check if this mode has a specific tag
        /// </summary>
        /// <param name="tag">Tag to check for</param>
        /// <returns>True if the mode has the tag</returns>
        public bool HasTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return false;
            
            foreach (var modeTag in tags)
            {
                if (string.Equals(modeTag, tag, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Check if this mode is valid and ready to use
        /// </summary>
        /// <returns>True if the mode is valid</returns>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(modeName) && 
                   virtualCamera != null && 
                   isEnabled;
        }
        
        /// <summary>
        /// Gets the effective mode ID for camera matching.
        /// Returns modeId if set, otherwise falls back to camera name.
        /// </summary>
        /// <returns>The ID to use for finding the camera in the scene</returns>
        public string GetEffectiveModeId()
        {
            if (!string.IsNullOrEmpty(modeId))
            {
                return modeId;
            }
            
            if (virtualCamera != null)
            {
                return virtualCamera.name;
            }
            
            return modeName; // Last fallback
        }
        
        /// <summary>
        /// Get a summary of this mode's configuration
        /// </summary>
        /// <returns>String summary of the mode</returns>
        public string GetSummary()
        {
            var summary = $"Mode: {modeName}\n";
            summary += $"Camera: {(virtualCamera != null ? virtualCamera.name : "None")}\n";
            summary += $"Follow: {(enableFollow ? "Enabled" : "Disabled")}\n";
            summary += $"Zoom: {(enableZoom ? "Enabled" : "Disabled")}\n";
            summary += $"Drag: {(enableDrag ? "Enabled" : "Disabled")}\n";
            summary += $"Priority: {priority}\n";
            
            if (tags.Length > 0)
            {
                summary += $"Tags: {string.Join(", ", tags)}";
            }
            
            return summary;
        }
        
        #endregion
    }
}

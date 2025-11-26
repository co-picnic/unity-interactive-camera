using UnityEngine;

namespace InteractiveCameraSystem
{
    /// <summary>
    /// Unified camera projection and lens settings.
    /// Replaces the complex behavior system with simple, direct configuration.
    /// </summary>
    [System.Serializable]
    public class CameraSettings
    {
        [Header("Projection")]
        [Tooltip("Field of view in degrees")]
        [Range(1f, 179f)]
        public float fieldOfView = 60f;
        
        [Tooltip("Near clipping plane distance")]
        [Range(0.01f, 10f)]
        public float nearClipPlane = 0.1f;
        
        [Tooltip("Far clipping plane distance")]
        [Range(10f, 10000f)]
        public float farClipPlane = 1000f;
        
        // Physical camera properties removed - keeping system simple
        
        [Header("Additional Settings")]
        [Tooltip("Camera pitch angle in degrees")]
        [Range(-90f, 90f)]
        public float pitchAngle = 0f;
        
        [Tooltip("Initial ground height for this camera mode")]
        public float initialGroundHeight = 0f;
        
        [Tooltip("Use initial ground height when switching to this mode")]
        public bool useInitialGroundHeight = false;
        
        #region Utility Methods
        
        /// <summary>
        /// Get the field of view (simplified - no physical camera calculations)
        /// </summary>
        /// <returns>Field of view in degrees</returns>
        public float GetFieldOfView()
        {
            return fieldOfView;
        }
        
        #endregion
    }
}

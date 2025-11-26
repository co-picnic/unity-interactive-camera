using UnityEngine;

namespace InteractiveCameraSystem
{
    /// <summary>
    /// Settings for zoom behavior in camera modes
    /// Based on VirtualCameraZoomConfig with Cinemachine-specific settings
    /// </summary>
    [System.Serializable]
    public class ZoomSettings
    {
        [Header("Zoom Sensitivity")]
        [Tooltip("Zoom sensitivity multiplier. Higher values = more zoom per pinch")]
        [Range(0.1f, 5f)] public float zoomSensitivity = 1f;
        
        [Tooltip("Reverse zoom direction. Unchecked = pinch out zooms out, Checked = pinch out zooms in")]
        public bool invertZoom = false;
        
        [Header("FOV Limits")]
        [Tooltip("Minimum field of view (zoom out limit)")]
        [Range(1f, 60f)] public float minFOV = 20f;
        
        [Tooltip("Maximum field of view (zoom in limit)")]
        [Range(60f, 120f)] public float maxFOV = 80f;
        
        [Tooltip("Initial field of view when switching to this mode")]
        [Range(1f, 120f)] public float initialFOV = 60f;
        
        [Header("Zoom Behavior")]
        [Tooltip("Enable smooth zoom transitions")]
        public bool enableSmoothZoom = true;
        
        [Tooltip("Zoom transition speed when smooth zoom is enabled")]
        [Range(0.1f, 10f)] public float zoomSpeed = 3f;
        
        [Tooltip("Enable zoom during drag (simultaneous zoom and pan)")]
        public bool allowZoomDuringDrag = true;
        
        [Tooltip("Reduce drag sensitivity during zoom to avoid conflicts")]
        public bool reduceDragSensitivityDuringZoom = true;
        
        [Tooltip("Drag sensitivity multiplier when zooming (0.5 = half sensitivity)")]
        [Range(0.1f, 1f)] public float dragSensitivityMultiplier = 0.7f;
        
        [Header("Zoom Constraints")]
        [Tooltip("Enable zoom boundary constraints (respects camera bounds)")]
        public bool enableZoomBoundary = false;
        
        [Tooltip("Minimum distance from target (prevents zooming too close)")]
        [Range(0.1f, 50f)] public float minDistance = 2f;
        
        [Tooltip("Maximum distance from target (prevents zooming too far)")]
        [Range(10f, 1000f)] public float maxDistance = 100f;
        
        [Header("Zoom Modes")]
        [Tooltip("How zoom is applied to the camera")]
        public ZoomMode zoomMode = ZoomMode.FOV;
        
        [Tooltip("Zoom interpolation curve for smooth transitions")]
        public AnimationCurve zoomCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        
        #region Validation
        
        /// <summary>
        /// Check if these zoom settings are valid
        /// </summary>
        /// <returns>True if the settings are valid</returns>
        public bool IsValid()
        {
            bool basicValid = zoomSensitivity > 0f && 
                             minFOV > 0f && maxFOV > minFOV && 
                             initialFOV >= minFOV && initialFOV <= maxFOV &&
                             zoomSpeed > 0f && 
                             minDistance > 0f && maxDistance > minDistance;
            
            if (!basicValid) return false;
            
            // Validate drag sensitivity multiplier only if reducing during zoom
            if (reduceDragSensitivityDuringZoom)
            {
                return dragSensitivityMultiplier > 0f && dragSensitivityMultiplier <= 1f;
            }
            
            return true;
        }
        
        /// <summary>
        /// Get a summary of these zoom settings
        /// </summary>
        /// <returns>String summary of the settings</returns>
        public string GetSummary()
        {
            string smoothInfo = enableSmoothZoom ? $"Smooth({zoomSpeed:F1})" : "Instant";
            string dragInfo = allowZoomDuringDrag ? $"DragOK({dragSensitivityMultiplier:F1})" : "NoDrag";
            string boundaryInfo = enableZoomBoundary ? $"Boundary({minDistance:F1}-{maxDistance:F1})" : "NoBoundary";
            return $"Zoom: {zoomMode} Sens:{zoomSensitivity:F1} FOV:{minFOV:F0}-{maxFOV:F0} ({smoothInfo}, {dragInfo}, {boundaryInfo})";
        }
        
        /// <summary>
        /// Get the effective zoom sensitivity (applies invert flag)
        /// </summary>
        /// <returns>Zoom sensitivity with invert applied</returns>
        public float GetEffectiveZoomSensitivity()
        {
            return invertZoom ? -zoomSensitivity : zoomSensitivity;
        }
        
        /// <summary>
        /// Clamp FOV to valid range
        /// </summary>
        /// <param name="fov">FOV to clamp</param>
        /// <returns>Clamped FOV</returns>
        public float ClampFOV(float fov)
        {
            return Mathf.Clamp(fov, minFOV, maxFOV);
        }
        
        /// <summary>
        /// Clamp distance to valid range
        /// </summary>
        /// <param name="distance">Distance to clamp</param>
        /// <returns>Clamped distance</returns>
        public float ClampDistance(float distance)
        {
            return Mathf.Clamp(distance, minDistance, maxDistance);
        }
        
        #endregion
    }
    
    /// <summary>
    /// Zoom application modes
    /// </summary>
    public enum ZoomMode
    {
        /// <summary>Zoom by changing field of view</summary>
        FOV,
        /// <summary>Zoom by changing camera distance from target</summary>
        Distance,
        /// <summary>Zoom by changing both FOV and distance</summary>
        Hybrid
    }
}

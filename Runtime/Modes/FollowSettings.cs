using UnityEngine;

namespace InteractiveCameraSystem
{
    /// <summary>
    /// Settings for camera following behavior.
    /// Configures how the camera follows targets and handles multi-target scenarios.
    /// </summary>
    [System.Serializable]
    public class FollowSettings
    {
        [Header("Follow Behavior")]
        [Tooltip("Offset from the target position")]
        public Vector3 followOffset = Vector3.zero;
        
        [Tooltip("Follow target on X axis")]
        public bool followX = true;
        
        [Tooltip("Follow target on Y axis")]
        public bool followY = true;
        
        [Tooltip("Follow target on Z axis")]
        public bool followZ = true;
        
        [Header("Follow Smoothing")]
        [Tooltip("How quickly the camera follows the target")]
        [Range(0.1f, 20f)]
        public float followSpeed = 2f;
        
        [Tooltip("How quickly the camera rotates to face the target")]
        [Range(0.1f, 20f)]
        public float lookAtSpeed = 2f;
        
        [Header("Multi-Target Settings")]
        [Tooltip("Use group framing for multiple targets")]
        public bool useGroupFraming = false;
        
        [Tooltip("Minimum dolly distance for group framing")]
        [Range(-50f, 50f)]
        public float dollyDistanceMin = -10f;
        
        [Tooltip("Maximum dolly distance for group framing")]
        [Range(-50f, 50f)]
        public float dollyDistanceMax = 10f;
        
        [Tooltip("Framing size - screen space bounding box that targets should occupy (1 = full screen, 0.5 = half screen)")]
        [Range(0.1f, 2f)]
        public float framingSize = 1f;
        
        [Header("Target Weighting")]
        [Tooltip("Weight for the primary target (first in list)")]
        [Range(0f, 2f)]
        public float primaryTargetWeight = 1f;
        
        [Tooltip("Weight for secondary targets")]
        [Range(0f, 2f)]
        public float secondaryTargetWeight = 0.5f;
        
        [Header("Target Radius")]
        [Tooltip("Radius for target group bounding sphere calculation")]
        [Range(0.1f, 10f)]
        public float targetRadius = 1f;
        
        [Header("Advanced Follow Options")]
        [Tooltip("Use custom follow offset instead of default")]
        public bool useCustomFollowOffset = false;
        
        [Tooltip("Custom follow offset (only used if useCustomFollowOffset is true)")]
        public Vector3 customFollowOffset = Vector3.zero;
        
        [Tooltip("Lock camera to ground level")]
        public bool lockToGround = false;
        
        [Tooltip("Ground level Y position")]
        public float groundLevel = 0f;
        
        #region Utility Methods
        
        /// <summary>
        /// Get the effective follow offset (custom or default)
        /// </summary>
        /// <returns>Effective follow offset</returns>
        public Vector3 GetEffectiveFollowOffset()
        {
            return useCustomFollowOffset ? customFollowOffset : followOffset;
        }
        
        /// <summary>
        /// Check if a specific axis should be followed
        /// </summary>
        /// <param name="axis">Axis to check (0=X, 1=Y, 2=Z)</param>
        /// <returns>True if the axis should be followed</returns>
        public bool ShouldFollowAxis(int axis)
        {
            switch (axis)
            {
                case 0: return followX;
                case 1: return followY;
                case 2: return followZ;
                default: return false;
            }
        }
        
        /// <summary>
        /// Get the weight for a target based on its position in the list
        /// </summary>
        /// <param name="targetIndex">Index of the target in the list</param>
        /// <returns>Weight for this target</returns>
        public float GetTargetWeight(int targetIndex)
        {
            return targetIndex == 0 ? primaryTargetWeight : secondaryTargetWeight;
        }
        
        /// <summary>
        /// Check if group framing should be used for the given number of targets
        /// </summary>
        /// <param name="targetCount">Number of targets</param>
        /// <returns>True if group framing should be used</returns>
        public bool ShouldUseGroupFraming(int targetCount)
        {
            return useGroupFraming && targetCount > 1;
        }
        
        #endregion
    }
}

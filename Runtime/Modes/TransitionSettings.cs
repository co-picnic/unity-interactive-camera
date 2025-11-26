using UnityEngine;

namespace InteractiveCameraSystem
{
    /// <summary>
    /// Settings for camera transitions between modes.
    /// Configures how smooth transitions are handled and their timing.
    /// </summary>
    [System.Serializable]
    public class TransitionSettings
    {
        [Header("Transition Timing")]
        [Tooltip("Duration of the transition in seconds")]
        [Range(0f, 10f)]
        public float blendDuration = 1f;
        
        [Tooltip("Delay before starting the transition")]
        [Range(0f, 5f)]
        public float blendDelay = 0f;
        
        [Header("Transition Curve")]
        [Tooltip("Animation curve for the transition")]
        public AnimationCurve blendCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Tooltip("Type of blend curve to use")]
        public BlendCurveType blendCurveType = BlendCurveType.EaseInOut;
        
        [Header("Position Transition")]
        [Tooltip("Smooth position transitions")]
        public bool smoothPosition = true;
        
        [Tooltip("Smooth rotation transitions")]
        public bool smoothRotation = true;
        
        [Tooltip("Smooth field of view transitions")]
        public bool smoothFieldOfView = true;
        
        [Header("Advanced Transition Options")]
        [Tooltip("Use custom transition curve instead of preset")]
        public bool useCustomCurve = false;
        
        [Tooltip("Custom transition curve (only used if useCustomCurve is true)")]
        public AnimationCurve customCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        
        [Tooltip("Transition priority (higher = more important)")]
        [Range(0, 100)]
        public int transitionPriority = 50;
        
        [Header("Transition Constraints")]
        [Tooltip("Lock camera to ground during transition")]
        public bool lockToGroundDuringTransition = false;
        
        [Tooltip("Ground level for transition locking")]
        public float transitionGroundLevel = 0f;
        
        [Tooltip("Maximum transition speed")]
        [Range(0.1f, 50f)]
        public float maxTransitionSpeed = 10f;
        
        #region Utility Methods
        
        /// <summary>
        /// Get the effective transition curve
        /// </summary>
        /// <returns>Animation curve to use for transitions</returns>
        public AnimationCurve GetEffectiveCurve()
        {
            if (useCustomCurve)
            {
                return customCurve;
            }
            
            return GetPresetCurve(blendCurveType);
        }
        
        /// <summary>
        /// Get a preset animation curve based on the curve type
        /// </summary>
        /// <param name="curveType">Type of curve to get</param>
        /// <returns>Animation curve</returns>
        private AnimationCurve GetPresetCurve(BlendCurveType curveType)
        {
            switch (curveType)
            {
                case BlendCurveType.Linear:
                    return AnimationCurve.Linear(0f, 0f, 1f, 1f);
                
                case BlendCurveType.EaseIn:
                    return AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                
                case BlendCurveType.EaseOut:
                    return AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                
                case BlendCurveType.EaseInOut:
                    return AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                
                case BlendCurveType.Bounce:
                    return AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // Simplified bounce
                
                case BlendCurveType.Elastic:
                    return AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // Simplified elastic
                
                default:
                    return AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            }
        }
        
        /// <summary>
        /// Check if a specific property should be smoothly transitioned
        /// </summary>
        /// <param name="property">Property to check</param>
        /// <returns>True if the property should be smoothly transitioned</returns>
        public bool ShouldSmoothProperty(TransitionProperty property)
        {
            switch (property)
            {
                case TransitionProperty.Position:
                    return smoothPosition;
                case TransitionProperty.Rotation:
                    return smoothRotation;
                case TransitionProperty.FieldOfView:
                    return smoothFieldOfView;
                default:
                    return true;
            }
        }
        
        /// <summary>
        /// Get the effective ground level for transitions
        /// </summary>
        /// <returns>Ground level to use during transitions</returns>
        public float GetEffectiveGroundLevel()
        {
            return lockToGroundDuringTransition ? transitionGroundLevel : 0f;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Types of blend curves available for transitions
    /// </summary>
    public enum BlendCurveType
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut,
        Bounce,
        Elastic
    }
    
    /// <summary>
    /// Properties that can be transitioned
    /// </summary>
    public enum TransitionProperty
    {
        Position,
        Rotation,
        FieldOfView
    }
}

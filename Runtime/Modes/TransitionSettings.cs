using UnityEngine;
using Unity.Cinemachine;

namespace InteractiveCameraSystem
{
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
        [Tooltip("Blend curve style (matches Cinemachine Brain)")]
        public CinemachineBlendDefinition.Styles blendStyle = CinemachineBlendDefinition.Styles.EaseInOut;
        
        [Header("Advanced Transition Options")]
        [Tooltip("Custom transition curve (only used when Blend Style is set to Custom)")]
        public AnimationCurve customCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Position Transition")]
        [Tooltip("Smooth position transitions")]
        public bool smoothPosition = true;
        
        [Tooltip("Smooth rotation transitions")]
        public bool smoothRotation = true;
        
        [Tooltip("Smooth field of view transitions")]
        public bool smoothFieldOfView = true;

        [Header("Transition Constraints")]
        [Tooltip("Lock camera to ground during transition")]
        public bool lockToGroundDuringTransition = false;
        
        [Tooltip("Ground level for transition locking")]
        public float transitionGroundLevel = 0f;
        
        [Tooltip("Maximum transition speed")]
        [Range(0.1f, 50f)]
        public float maxTransitionSpeed = 10f;
        
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
        
        public float GetEffectiveGroundLevel()
        {
            return lockToGroundDuringTransition ? transitionGroundLevel : 0f;
        }
    }

    public enum TransitionProperty
    {
        Position,
        Rotation,
        FieldOfView
    }
}

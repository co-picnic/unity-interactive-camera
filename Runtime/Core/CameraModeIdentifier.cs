using UnityEngine;
using Unity.Cinemachine;

namespace InteractiveCameraSystem
{
    /// <summary>
    /// Identifies a Cinemachine camera for mode matching.
    /// This allows cameras to be renamed without breaking references.
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera))]
    public class CameraModeIdentifier : MonoBehaviour
    {
        [Header("Identification")]
        [Tooltip("Unique identifier for this camera mode (should match CameraMode asset)")]
        [SerializeField] private string modeId;
        
        [Tooltip("Optional display name for debugging")]
        [SerializeField] private string displayName;
        
        [Header("Auto-Configuration")]
        [Tooltip("Automatically set modeId from the GameObject name on creation")]
        [SerializeField] private bool autoSetFromName = true;

        public string ModeId => modeId;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? modeId : displayName;

        private CinemachineCamera _cachedCamera;
        public CinemachineCamera CinemachineCamera
        {
            get
            {
                if (_cachedCamera == null)
                {
                    _cachedCamera = GetComponent<CinemachineCamera>();
                }
                return _cachedCamera;
            }
        }

        private void Awake()
        {
            // Validate that we have a mode ID
            if (string.IsNullOrEmpty(modeId))
            {
                Debug.LogWarning($"[CameraModeIdentifier] Camera '{gameObject.name}' has no modeId assigned!", this);
            }
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-set modeId from GameObject name if enabled and modeId is empty
            if (autoSetFromName && string.IsNullOrEmpty(modeId))
            {
                modeId = gameObject.name;
            }

            // Cache the display name
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = modeId;
            }
        }

        private void Reset()
        {
            // When component is first added, set modeId from GameObject name
            modeId = gameObject.name;
            displayName = modeId;
        }
        #endif

        /// <summary>
        /// Check if this identifier matches the given mode ID
        /// </summary>
        public bool Matches(string queryModeId)
        {
            return !string.IsNullOrEmpty(modeId) && 
                   modeId.Equals(queryModeId, System.StringComparison.Ordinal);
        }

        /// <summary>
        /// Get a debug string for this identifier
        /// </summary>
        public override string ToString()
        {
            return $"CameraModeIdentifier[{modeId}] on '{gameObject.name}'";
        }
    }
}


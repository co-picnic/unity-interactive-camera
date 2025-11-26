using UnityEngine;
using Unity.Cinemachine;
using Lean.Touch;
using CW.Common;

namespace InteractiveCameraSystem
{
    /// <summary>
    /// Input component that handles zoom functionality for Cinemachine cameras
    /// Based on VirtualCameraZoomComponent with Cinemachine-specific integration
    /// </summary>
    public class CinemachineZoomInputComponent : MonoBehaviour, ICinemachineCameraInputComponent
    {
        [Header("Zoom Configuration")]
        [Tooltip("Zoom sensitivity multiplier. Higher values = more zoom per pinch")]
        [SerializeField] [Range(0.1f, 5f)] private float zoomSensitivity = 1f;
        
        [Tooltip("Reverse zoom direction. Unchecked = pinch out zooms out, Checked = pinch out zooms in")]
        [SerializeField] private bool invertZoom = false;
        
        [Tooltip("Minimum field of view (zoom out limit)")]
        [SerializeField] [Range(1f, 60f)] private float minFOV = 20f;
        
        [Tooltip("Maximum field of view (zoom in limit)")]
        [SerializeField] [Range(60f, 120f)] private float maxFOV = 80f;
        
        [Tooltip("Enable smooth zoom transitions")]
        [SerializeField] private bool enableSmoothZoom = true;
        
        [Tooltip("Zoom transition speed when smooth zoom is enabled")]
        [SerializeField] [Range(0.1f, 10f)] private float zoomSpeed = 3f;
        
        [Header("Zoom Behavior")]
        [Tooltip("Enable zoom during drag (simultaneous zoom and pan)")]
        [SerializeField] private bool allowZoomDuringDrag = true;
        
        [Tooltip("Reduce drag sensitivity during zoom to avoid conflicts")]
        [SerializeField] private bool reduceDragSensitivityDuringZoom = true;
        
        [Tooltip("Drag sensitivity multiplier when zooming (0.5 = half sensitivity)")]
        [SerializeField] [Range(0.1f, 1f)] private float dragSensitivityMultiplier = 0.7f;
        
        [Header("Zoom Constraints")]
        [Tooltip("Enable zoom boundary constraints (respects camera bounds)")]
        [SerializeField] private bool enableZoomBoundary = false;
        
        [Tooltip("Minimum distance from target (prevents zooming too close)")]
        [SerializeField] [Range(0.1f, 50f)] private float minDistance = 2f;
        
        [Tooltip("Maximum distance from target (prevents zooming too far)")]
        [SerializeField] [Range(10f, 1000f)] private float maxDistance = 100f;
        
        [Header("Zoom Modes")]
        [Tooltip("How zoom is applied to the camera")]
        [SerializeField] private ZoomMode zoomMode = ZoomMode.FOV;
        
        [Tooltip("Zoom interpolation curve for smooth transitions")]
        [SerializeField] private AnimationCurve zoomCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        
        [Header("Debug")]
        [Tooltip("Enable debug logging")]
        [SerializeField] private bool debugMode = false;
        
        // State variables
        private bool isZooming = false;
        private float currentFOV;
        private float targetFOV;
        private bool isInputLocked = false;
        private bool isConfigured = false;
        
        // Camera and mode references
        private CinemachineCamera targetCamera;
        private CameraMode associatedMode;
        private bool isActive = false;
        
        // Drag component reference for coordination
        private CinemachineDragInputComponent dragComponent;
        
        #region Properties
        
        public bool IsActive => isActive && enabled;
        public CameraMode AssociatedMode => associatedMode;
        
        private bool ShowDebugInfo => debugMode;

        public float ZoomSensitivity
        {
            get { return zoomSensitivity; }
            set { zoomSensitivity = value; }
        }

        public float MinFOV
        {
            get { return minFOV; }
            set { minFOV = value; }
        }

        public float MaxFOV
        {
            get { return maxFOV; }
            set { maxFOV = value; }
        }

        public bool EnableSmoothZoom
        {
            get { return enableSmoothZoom; }
            set { enableSmoothZoom = value; }
        }

        public float ZoomSpeed
        {
            get { return zoomSpeed; }
            set { zoomSpeed = value; }
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        void Start()
        {
            // Get reference to drag component for coordination
            dragComponent = GetComponent<CinemachineDragInputComponent>();
        }
        
        void Update()
        {
            if (!IsActive || !isConfigured) return;
            
            // Apply smooth zoom if enabled
            if (enableSmoothZoom && Mathf.Abs(targetFOV - currentFOV) > 0.1f)
            {
                ApplySmoothZoom();
            }
        }
        
        void OnDisable()
        {
            isZooming = false;
            if (dragComponent != null && reduceDragSensitivityDuringZoom)
            {
                dragComponent.SetDragSensitivityMultiplier(1f);
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Initialize this zoom component for a specific camera and mode
        /// </summary>
        /// <param name="camera">The CinemachineCamera to control</param>
        /// <param name="mode">The camera mode this component is associated with</param>
        public void Initialize(CinemachineCamera camera, CameraMode mode)
        {
            targetCamera = camera;
            associatedMode = mode;
            isActive = mode != null && mode.enableZoom;
            
            if (mode != null && mode.zoomSettings != null)
            {
                ApplyZoomSettings(mode.zoomSettings);
            }
            else
            {
                // Initialize currentFOV from camera's current FOV when no settings provided
                if (camera != null)
                {
                    currentFOV = camera.Lens.FieldOfView;
                    targetFOV = currentFOV;
                }
            }
            
            isConfigured = true;
        }
        
        /// <summary>
        /// Apply zoom settings from a CameraMode's zoomSettings
        /// </summary>
        /// <param name="settings">The zoom settings to apply</param>
        public void ApplyZoomSettings(ZoomSettings settings)
        {
            if (settings == null) 
            {
                isConfigured = false;
                return;
            }
            
            // Apply zoom settings
            zoomSensitivity = settings.zoomSensitivity;
            invertZoom = settings.invertZoom;
            minFOV = settings.minFOV;
            maxFOV = settings.maxFOV;
            enableSmoothZoom = settings.enableSmoothZoom;
            zoomSpeed = settings.zoomSpeed;
            allowZoomDuringDrag = settings.allowZoomDuringDrag;
            reduceDragSensitivityDuringZoom = settings.reduceDragSensitivityDuringZoom;
            dragSensitivityMultiplier = settings.dragSensitivityMultiplier;
            enableZoomBoundary = settings.enableZoomBoundary;
            minDistance = settings.minDistance;
            maxDistance = settings.maxDistance;
            zoomMode = settings.zoomMode;
            zoomCurve = settings.zoomCurve;
            
            // Set initial FOV from settings
            currentFOV = settings.initialFOV;
            targetFOV = currentFOV;
            
            isConfigured = true;
            
            if (ShowDebugInfo)
            {
                Debug.Log($"[CinemachineZoomInputComponent] Applied zoom settings: FOV={currentFOV}, Range=[{minFOV}, {maxFOV}], Sens={zoomSensitivity}");
            }
        }
        
        /// <summary>
        /// Set the active state of this component
        /// </summary>
        /// <param name="active">Whether the component should be active</param>
        public void SetActive(bool active)
        {
            isActive = active && enabled;
        }
        
        /// <summary>
        /// Lock or unlock user input for zoom
        /// </summary>
        /// <param name="locked">True to lock input, false to unlock</param>
        public void SetInputLocked(bool locked)
        {
            isInputLocked = locked;
            
            if (locked && isZooming)
            {
                StopZooming();
            }
        }
        
        /// <summary>
        /// Check if input is currently locked
        /// </summary>
        public bool IsInputLocked => isInputLocked;
        
        /// <summary>
        /// Whether the camera is currently being zoomed
        /// </summary>
        public bool IsZooming => isZooming && enabled && isActiveAndEnabled;
        
        /// <summary>
        /// Check if the component is properly configured
        /// </summary>
        public bool IsConfigured => isConfigured;
        
        /// <summary>
        /// Get current field of view
        /// </summary>
        public float GetCurrentFOV() => currentFOV;
        
        /// <summary>
        /// Get target field of view
        /// </summary>
        public float GetTargetFOV() => targetFOV;
        
        /// <summary>
        /// Set zoom level directly (0 = min zoom, 1 = max zoom)
        /// </summary>
        /// <param name="zoomLevel">Zoom level between 0 and 1</param>
        public void SetZoomLevel(float zoomLevel)
        {
            zoomLevel = Mathf.Clamp01(zoomLevel);
            targetFOV = Mathf.Lerp(maxFOV, minFOV, zoomLevel);
            
            if (!enableSmoothZoom)
            {
                currentFOV = targetFOV;
                ApplyFOVToCamera();
            }
        }

        #endregion

        #region Input Event Handlers

        public void OnSingleFingerStart(FingerGestureData data) { }
        public void OnSingleFingerUpdate(FingerGestureData data) { }
        public void OnSingleFingerEnd(FingerGestureData data) { }

        public void OnPinchStart(FingerGestureData data)
        {
            if (!IsActive || !isConfigured || isInputLocked) 
            {
                return;
            }
            
            isZooming = true;
            
            // Coordinate with drag component
            if (dragComponent != null && reduceDragSensitivityDuringZoom)
            {
                dragComponent.SetDragSensitivityMultiplier(dragSensitivityMultiplier);
            }
        }
        
        public void OnPinchUpdate(FingerGestureData data)
        {
            if (!IsActive || !isConfigured || !isZooming || targetCamera == null || isInputLocked) 
            {
                return;
            }
            
            ProcessPinchZoom(data);
            
            // Handle simultaneous drag during zoom
            if (allowZoomDuringDrag && dragComponent != null && Mathf.Abs(data.pinchCenterDelta.magnitude) > 0.001f)
            {
                // Create simulated single finger data for drag
                var simulatedData = new FingerGestureData();
                simulatedData.gestureType = GestureType.SingleFinger;
                simulatedData.screenDelta = data.pinchCenterDelta;
                simulatedData.screenPosition = data.pinchCenter;
                
                dragComponent.OnSingleFingerUpdate(simulatedData);
            }
        }
        
        public void OnPinchEnd(FingerGestureData data)
        {
            if (!isZooming) return;
            
            isZooming = false;
            
            // Restore drag sensitivity
            if (dragComponent != null && reduceDragSensitivityDuringZoom)
            {
                dragComponent.SetDragSensitivityMultiplier(1f);
            }
        }
        
        #endregion
        
        #region Zoom Logic
        
        /// <summary>
        /// Process pinch zoom input and update target FOV
        /// </summary>
        private void ProcessPinchZoom(FingerGestureData data)
        {
            if (Mathf.Abs(data.pinchRatio - 1f) > 0.001f)
            {
                // Convert pinch ratio to zoom delta
                float rawDelta = (data.pinchRatio - 1f);
                
                // Apply invert flag
                if (invertZoom) rawDelta *= -1f;
                
                // Apply sensitivity
                float sensitizedDelta = rawDelta * zoomSensitivity;
                
                // Apply to current FOV
                float fovDelta = sensitizedDelta * currentFOV;
                targetFOV = Mathf.Clamp(currentFOV + fovDelta, minFOV, maxFOV);
                
                // Apply immediately if smooth zoom is disabled
                if (!enableSmoothZoom)
                {
                    currentFOV = targetFOV;
                    ApplyFOVToCamera();
                }
            }
        }
        
        /// <summary>
        /// Apply smooth zoom transition
        /// </summary>
        private void ApplySmoothZoom()
        {
            float dampingFactor = CwHelper.DampenFactor(zoomSpeed, Time.deltaTime);
            currentFOV = Mathf.Lerp(currentFOV, targetFOV, dampingFactor);
            ApplyFOVToCamera();
        }
        
        /// <summary>
        /// Apply FOV to the Cinemachine camera
        /// </summary>
        private void ApplyFOVToCamera()
        {
            if (targetCamera == null) return;
            
            try
            {
                var lensSettings = targetCamera.Lens;
                lensSettings.FieldOfView = currentFOV;
                targetCamera.Lens = lensSettings;
            }
            catch (System.Exception ex)
            {
                if (ShowDebugInfo)
                {
                    Debug.LogWarning($"[CinemachineZoomInputComponent] Could not apply FOV to camera: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// Stop zooming immediately
        /// </summary>
        private void StopZooming()
        {
            if (isZooming)
            {
                isZooming = false;
                
                // Restore drag sensitivity
                if (dragComponent != null && reduceDragSensitivityDuringZoom)
                {
                    dragComponent.SetDragSensitivityMultiplier(1f);
                }
            }
        }
        
        #endregion
        
        #region Editor Support
        
#if UNITY_EDITOR
        void OnValidate()
        {
            // Ensure min/max FOV are valid
            minFOV = Mathf.Max(1f, minFOV);
            maxFOV = Mathf.Max(minFOV, maxFOV);
            
            // Clamp current FOV to valid range
            if (Application.isPlaying)
            {
                currentFOV = Mathf.Clamp(currentFOV, minFOV, maxFOV);
                targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);
            }
        }
#endif
        
        #endregion
    }
}
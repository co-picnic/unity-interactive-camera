using UnityEngine;
using Unity.Cinemachine;
using Lean.Touch;
using CW.Common;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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

        [Header("Debug Mouse Wheel Zoom")]
        [Tooltip("Enable mouse wheel zoom for desktop/debug. Drives the same FOV target/smoothing as pinch zoom, and works even if the camera mode hasn't been activated yet (standalone debug feature).")]
        [SerializeField] private bool enableMouseWheelZoom = false;

        [Tooltip("How much FOV changes per scroll notch, as a fraction of current FOV. 0.5 ≈ 50% FOV change per notch. Range: 0.1-10.")]
        [SerializeField] [Range(0.1f, 10f)] private float mouseWheelSensitivity = 0.5f;

        [Tooltip("Reverse mouse wheel direction. Unchecked = scroll up zooms in (FOV decreases). Checked = scroll up zooms out.")]
        [SerializeField] private bool invertMouseWheel = false;

        [Tooltip("Bypass smooth zoom for mouse wheel input. Pinch zoom keeps its own smoothing — this only affects the wheel. Recommended ON for desktop feel.")]
        [SerializeField] private bool mouseWheelInstant = true;

        [Header("Debug Keyboard Zoom (Simulator-friendly)")]
        [Tooltip("Enable keyboard zoom keys. Required for the Simulator window, which consumes the scroll wheel itself.")]
        [SerializeField] private bool enableKeyboardZoom = false;

        [Tooltip("Primary key that zooms in (one scroll notch per press, or continuous while held).")]
        [SerializeField] private KeyCode zoomInKey = KeyCode.Equals;

        [Tooltip("Secondary key that zooms in. Useful for binding both the top-row '=' and the numpad '+'. Set to None to disable.")]
        [SerializeField] private KeyCode zoomInKeyAlt = KeyCode.KeypadPlus;

        [Tooltip("Primary key that zooms out (one scroll notch per press, or continuous while held).")]
        [SerializeField] private KeyCode zoomOutKey = KeyCode.Minus;

        [Tooltip("Secondary key that zooms out. Useful for binding both the top-row '-' and the numpad '-'. Set to None to disable.")]
        [SerializeField] private KeyCode zoomOutKeyAlt = KeyCode.KeypadMinus;

        [Tooltip("If on, holding the key continuously zooms at the given rate. If off, you must tap the key to step.")]
        [SerializeField] private bool holdToZoom = true;

        [Tooltip("Scroll notches per second when holding a zoom key. Total zoom rate ≈ notches/sec × mouseWheelSensitivity × current FOV.")]
        [SerializeField] [Range(0.1f, 10f)] private float keyboardZoomNotchesPerSecond = 3f;

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
            // Mouse wheel runs as a standalone debug feature, independent of mode activation
            // (mirrors how CinemachineDragInputComponent handles its keyboard debug controls).
            if (enableMouseWheelZoom && !isInputLocked)
            {
                ProcessMouseWheelInput();
            }

            // Keyboard zoom is the Simulator-friendly path: the Simulator window eats the
            // scroll wheel for its own device-preview zoom, so we expose +/- keys that flow
            // through Keyboard.current and reach the game in both Game view and Simulator.
            if (enableKeyboardZoom && !isInputLocked)
            {
                ProcessKeyboardZoomInput();
            }

            // Smooth zoom toward target whenever there's a delta and we have a camera.
            // Pinch only sets targetFOV when fully active, so we don't need the old guard here.
            if (enableSmoothZoom && targetCamera != null && Mathf.Abs(targetFOV - currentFOV) > 0.1f)
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
            
            // Seed FOV state from the live lens, NOT from settings.initialFOV.
            // initialFOV in ZoomSettings often disagrees with both the prefab's lens
            // FieldOfView and the mode's CameraSettings.fieldOfView, which causes a
            // visible snap on the first zoom interaction. Sourcing from the live lens
            // guarantees internal state matches what the player is actually seeing.
            // Falls back to settings.initialFOV if no camera is bound yet.
            if (targetCamera != null)
            {
                currentFOV = targetCamera.Lens.FieldOfView;
            }
            else
            {
                currentFOV = settings.initialFOV;
            }
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
                // Re-sync from the live lens when at rest. Same fix as the mouse wheel
                // path: prevents the visible snap when internal currentFOV has drifted
                // from the actual lens (e.g. after Initialize seeded a stale value, or
                // another system wrote to Lens.FieldOfView between gestures).
                if (targetCamera != null && Mathf.Abs(currentFOV - targetFOV) < 0.1f)
                {
                    float liveFOV = targetCamera.Lens.FieldOfView;
                    currentFOV = liveFOV;
                    targetFOV = liveFOV;
                }

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
        /// Process mouse scroll wheel input and update target FOV.
        /// Works as a standalone debug feature — lazy-fetches the camera and seeds FOV state
        /// from the live lens if Initialize() hasn't run yet.
        /// </summary>
        private void ProcessMouseWheelInput()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponent<CinemachineCamera>();
                if (targetCamera == null) return;
            }

            // Normalize scroll input to ~±1 per notch across input backends.
            float wheelDelta;
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse == null) return;
            // Input System returns ~±120 per notch (raw OS ticks).
            wheelDelta = mouse.scroll.ReadValue().y / 120f;
#else
            // Legacy Input returns ~±0.1 per notch; scale to match Input System.
            wheelDelta = Input.GetAxis("Mouse ScrollWheel") * 10f;
#endif
            if (Mathf.Abs(wheelDelta) < 0.0001f) return;

            if (invertMouseWheel) wheelDelta *= -1f;

            // Dispatch on the configured zoom mode so the wheel matches whatever the
            // mode/prefab is set to (FOV / Distance / Hybrid).
            switch (zoomMode)
            {
                case ZoomMode.FOV:
                    ApplyWheelFOVDelta(wheelDelta);
                    break;
                case ZoomMode.Distance:
                    ApplyWheelDistanceDelta(wheelDelta);
                    break;
                case ZoomMode.Hybrid:
                    // Split the impulse so combined visual effect is roughly equivalent.
                    ApplyWheelFOVDelta(wheelDelta * 0.5f);
                    ApplyWheelDistanceDelta(wheelDelta * 0.5f);
                    break;
            }
        }

        /// <summary>
        /// Process keyboard zoom keys (+/-) by synthesizing scroll-wheel notches and feeding
        /// them through the same zoom dispatch as the mouse wheel. Works in the Simulator
        /// window because Keyboard.current is forwarded even when the scroll wheel is not.
        /// </summary>
        private void ProcessKeyboardZoomInput()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponent<CinemachineCamera>();
                if (targetCamera == null) return;
            }

            float synthDelta = 0f;
            if (holdToZoom)
            {
                if (IsAnyZoomKeyHeld(zoomInKey, zoomInKeyAlt))
                {
                    synthDelta += keyboardZoomNotchesPerSecond * Time.deltaTime;
                }
                if (IsAnyZoomKeyHeld(zoomOutKey, zoomOutKeyAlt))
                {
                    synthDelta -= keyboardZoomNotchesPerSecond * Time.deltaTime;
                }
            }
            else
            {
                if (IsAnyZoomKeyDownThisFrame(zoomInKey, zoomInKeyAlt))
                {
                    synthDelta += 1f;
                }
                if (IsAnyZoomKeyDownThisFrame(zoomOutKey, zoomOutKeyAlt))
                {
                    synthDelta -= 1f;
                }
            }

            if (Mathf.Abs(synthDelta) < 0.0001f) return;
            if (invertMouseWheel) synthDelta *= -1f;

            switch (zoomMode)
            {
                case ZoomMode.FOV:
                    ApplyWheelFOVDelta(synthDelta);
                    break;
                case ZoomMode.Distance:
                    ApplyWheelDistanceDelta(synthDelta);
                    break;
                case ZoomMode.Hybrid:
                    ApplyWheelFOVDelta(synthDelta * 0.5f);
                    ApplyWheelDistanceDelta(synthDelta * 0.5f);
                    break;
            }
        }

        private static bool IsAnyZoomKeyHeld(KeyCode primary, KeyCode alt)
        {
            return (primary != KeyCode.None && IsZoomKeyHeld(primary))
                || (alt != KeyCode.None && IsZoomKeyHeld(alt));
        }

        private static bool IsAnyZoomKeyDownThisFrame(KeyCode primary, KeyCode alt)
        {
            return (primary != KeyCode.None && IsZoomKeyDownThisFrame(primary))
                || (alt != KeyCode.None && IsZoomKeyDownThisFrame(alt));
        }

        private static bool IsZoomKeyHeld(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                Key k = MapZoomKeyCodeToKey(key);
                if (k != Key.None && keyboard[k].isPressed) return true;
            }
#endif
            try { return Input.GetKey(key); } catch { return false; }
        }

        private static bool IsZoomKeyDownThisFrame(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                Key k = MapZoomKeyCodeToKey(key);
                if (k != Key.None && keyboard[k].wasPressedThisFrame) return true;
            }
#endif
            try { return Input.GetKeyDown(key); } catch { return false; }
        }

#if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// Minimal KeyCode → Key bridge for the zoom hotkeys. Covers the realistic choices
        /// users will bind to zoom (=, -, +, [, ], digits, letters, function keys).
        /// </summary>
        private static Key MapZoomKeyCodeToKey(KeyCode code)
        {
            if (code >= KeyCode.A && code <= KeyCode.Z)
                return (Key)((int)Key.A + (code - KeyCode.A));
            if (code >= KeyCode.Alpha0 && code <= KeyCode.Alpha9)
                return (Key)((int)Key.Digit0 + (code - KeyCode.Alpha0));
            if (code >= KeyCode.F1 && code <= KeyCode.F12)
                return (Key)((int)Key.F1 + (code - KeyCode.F1));
            if (code >= KeyCode.Keypad0 && code <= KeyCode.Keypad9)
                return (Key)((int)Key.Numpad0 + (code - KeyCode.Keypad0));

            switch (code)
            {
                case KeyCode.Equals: return Key.Equals;
                case KeyCode.Minus: return Key.Minus;
                case KeyCode.Plus: return Key.NumpadPlus;
                case KeyCode.KeypadPlus: return Key.NumpadPlus;
                case KeyCode.KeypadMinus: return Key.NumpadMinus;
                case KeyCode.LeftBracket: return Key.LeftBracket;
                case KeyCode.RightBracket: return Key.RightBracket;
                case KeyCode.Comma: return Key.Comma;
                case KeyCode.Period: return Key.Period;
                case KeyCode.Slash: return Key.Slash;
                case KeyCode.Space: return Key.Space;
                case KeyCode.Tab: return Key.Tab;
                case KeyCode.UpArrow: return Key.UpArrow;
                case KeyCode.DownArrow: return Key.DownArrow;
                case KeyCode.LeftArrow: return Key.LeftArrow;
                case KeyCode.RightArrow: return Key.RightArrow;
                case KeyCode.LeftShift: return Key.LeftShift;
                case KeyCode.RightShift: return Key.RightShift;
                case KeyCode.LeftControl: return Key.LeftCtrl;
                case KeyCode.RightControl: return Key.RightCtrl;
                default: return Key.None;
            }
        }
#endif

        /// <summary>
        /// Mouse-wheel FOV change. Re-syncs from the live lens when at rest to avoid the
        /// "snap on first scroll" bug from a stale internal FOV state.
        /// </summary>
        private void ApplyWheelFOVDelta(float wheelDelta)
        {
            if (Mathf.Abs(currentFOV - targetFOV) < 0.1f)
            {
                float liveFOV = targetCamera.Lens.FieldOfView;
                currentFOV = liveFOV;
                targetFOV = liveFOV;
            }

            // Scroll up (positive delta) zooms in → FOV decreases. Step is proportional to
            // current FOV so the perceived change is consistent across the zoom range.
            float fovDelta = -wheelDelta * mouseWheelSensitivity * currentFOV;
            targetFOV = Mathf.Clamp(currentFOV + fovDelta, minFOV, maxFOV);

            if (mouseWheelInstant || !enableSmoothZoom)
            {
                currentFOV = targetFOV;
                ApplyFOVToCamera();
            }

            if (ShowDebugInfo)
            {
                Debug.Log($"[CinemachineZoomInputComponent] Wheel FOV: delta={wheelDelta:F3}, FOV={currentFOV:F1} → {targetFOV:F1}");
            }
        }

        /// <summary>
        /// Mouse-wheel "distance" change.
        ///  - Orthographic lens → scales <c>OrthographicSize</c> within <c>[minDistance, maxDistance]</c>.
        ///  - Perspective lens → dollies the camera transform along its forward vector.
        /// Note: under an active Cinemachine body component (Follow/Tracking) the dolly will
        /// be overwritten next frame — that's a debug-feature limitation.
        /// </summary>
        private void ApplyWheelDistanceDelta(float wheelDelta)
        {
            var lens = targetCamera.Lens;
            if (lens.Orthographic)
            {
                float size = lens.OrthographicSize;
                // Scroll up (positive delta) zooms in → orthoSize shrinks.
                float sizeDelta = -wheelDelta * mouseWheelSensitivity * size;
                float newSize = Mathf.Clamp(size + sizeDelta, minDistance, maxDistance);
                lens.OrthographicSize = newSize;
                targetCamera.Lens = lens;

                if (ShowDebugInfo)
                {
                    Debug.Log($"[CinemachineZoomInputComponent] Wheel ortho size: {size:F2} → {newSize:F2}");
                }
            }
            else
            {
                // Perspective dolly. Step magnitude scales with sensitivity; 1 unit per notch
                // at sensitivity=1. Scroll up (positive delta) zooms in → move forward.
                var t = targetCamera.transform;
                float step = wheelDelta * mouseWheelSensitivity;
                t.position += t.forward * step;

                if (ShowDebugInfo)
                {
                    Debug.Log($"[CinemachineZoomInputComponent] Wheel dolly: step={step:F3}m, pos={t.position}");
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
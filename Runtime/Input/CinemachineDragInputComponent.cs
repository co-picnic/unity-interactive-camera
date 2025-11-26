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
    /// Input component that handles drag functionality for Cinemachine cameras
    /// Based on VirtualCameraDragComponent with all the same settings and functionality
    /// </summary>
    public class CinemachineDragInputComponent : MonoBehaviour, ICinemachineCameraInputComponent
    {
        [Header("Boundary Configuration")]
        [Tooltip("Reference to VirtualCameraBounds component that defines the boundary shape")]
        public VirtualCameraBounds virtualCameraBounds;
        
        [Header("Input Mapping")]
        [Tooltip("What happens when finger moves left/right. WorldX = lateral movement, RotationY = camera turns left/right like FPS")]
        [SerializeField] private VirtualCameraAxis horizontalMapsTo = VirtualCameraAxis.WorldX;
        [Tooltip("How fast camera responds to horizontal finger movement. Range: 0.1-15. Recommended: 2-4 (responsive), 6-8 (mobile game), 1-2 (cinematic)")]
        [SerializeField] [Range(0.1f, 15f)] private float horizontalSensitivity = 2f;
        [Tooltip("Reverse horizontal controls. Unchecked = finger right moves camera right. Checked = finger right moves camera left")]
        [SerializeField] private bool invertHorizontal = true;
        
        [Tooltip("What happens when finger moves up/down. WorldZ = forward/back movement, RotationX = camera tilts up/down")]
        [SerializeField] private VirtualCameraAxis verticalMapsTo = VirtualCameraAxis.WorldZ;
        [Tooltip("How fast camera responds to vertical finger movement. Range: 0.1-15. Recommended: 2-4 (responsive), 6-8 (mobile game), 1-2 (cinematic)")]
        [SerializeField] [Range(0.1f, 15f)] private float verticalSensitivity = 2f;
        [Tooltip("Reverse vertical controls. Unchecked = finger up moves camera forward. Checked = finger up moves camera back")]
        [SerializeField] private bool invertVertical = false;
        
        [Header("Movement Feel")]
        [Tooltip("How smooth camera movement feels DURING drag. Range: 0.1-10. Recommended: 1-2 (responsive), 1.5 (balanced), 3-5 (smooth)")]
        [SerializeField] [Range(0.1f, 10f)] private float damping = 5f;
        
        [Header("Momentum System")]
        [Tooltip("Enable momentum after releasing finger. When disabled, camera stops instantly")]
        [SerializeField] private bool enableMomentum = true;
        [Tooltip("How much velocity is preserved when finger is released. Range: 0-1. Recommended: 0.2-0.4 (subtle), 0.5-0.7 (moderate), 0.8+ (strong)")]
        [SerializeField] [Range(0f, 1f)] private float momentum = 0.8f;
        [Tooltip("How quickly momentum decays after releasing finger. Range: 0.1-20. Recommended: 3-6 (natural), 8-12 (quick stop), 1-2 (long glide)")]
        [SerializeField] [Range(0.1f, 20f)] private float momentumDecay = 2f;
        
        [Header("Speed Control")]
        [Tooltip("Maximum camera movement speed in units per second. Range: 10-2000. Recommended: 300-500 (controlled), 800+ (mobile game), 100-200 (cinematic)")]
        [SerializeField] [Range(10f, 2000f)] private float maxCameraSpeed = 15f;
        
        [Header("Boundary Constraints")]
        [Tooltip("Enable camera boundary constraints. Prevents camera from moving outside defined areas")]
        [SerializeField] private bool enableBoundary = false;
        
        [Header("Movement Delta Mode")]
        [Tooltip("How finger movement is converted to camera movement. Fixed to world plane via VirtualCameraBounds.")]
        [SerializeField] private MovementDeltaMode movementDeltaMode = MovementDeltaMode.WorldPlaneViaBounds;
        
        [Header("Damping Mode")]
        [Tooltip("How damping is applied. Fixed to apply only after finger release (momentum phase).")]
        [SerializeField] private DragDampingMode dragDampingMode = DragDampingMode.ReleaseOnly;
        
        [Header("Debug")]
        [Tooltip("Enable debug logging")]
        [SerializeField] private bool debugMode = false;
        
        [Header("Debug Keyboard Controls")]
        [Tooltip("Enable WASD keyboard controls for camera movement (debug feature)")]
        [SerializeField] private bool enableKeyboardControls = true;
        
        [Tooltip("Keyboard movement speed multiplier")]
        [SerializeField] [Range(0.1f, 10f)] private float keyboardSpeedMultiplier = 1f;
        
        [Tooltip("Speed multiplier when Left Shift is held (sprint/boost)")]
        [SerializeField] [Range(1f, 5f)] private float shiftSpeedMultiplier = 2f;
        
        [Tooltip("Rotation speed for Q/E keys (degrees per second)")]
        [SerializeField] [Range(10f, 360f)] private float keyboardRotationSpeed = 30f;
        
        [Tooltip("Show debug messages for keyboard input")]
        [SerializeField] private bool debugKeyboardInput = false;
        
        // State variables (matching VirtualCameraDragComponent)
        private bool isDragging = false;
        private Vector3 dragVelocity = Vector3.zero;      // Velocity during active drag
        private Vector3 momentumVelocity = Vector3.zero;  // Velocity after drag release
        private Vector3 lastDragFrameVelocity = Vector3.zero; // Velocity computed from last applied drag frame (for ReleaseOnly mode)
        private float currentDragSensitivityMultiplier = 1.0f;
        private bool isInputLocked = false;
        
        // Configuration state (matching VirtualCameraDragComponent pattern)
        private bool isConfigured = false;
        
        // Camera and mode references
        private CinemachineCamera targetCamera;
        private CameraMode associatedMode;
        private bool isActive = false;
        
        // Boundary state (matching VirtualCameraDragComponent)
        private Vector3 previousPosition;
        private Bounds cachedBounds;
        private bool boundaryDataCached = false;
        private VirtualCameraBounds lastVirtualCameraBounds;
        private bool boundaryLoggedOnce = false;
        
        #region Properties
        
        public bool IsActive => isActive && enabled;
        public CameraMode AssociatedMode => associatedMode;
        
        private bool ShowDebugInfo => debugMode;

        public float HorizontalSensitivity
        {
            get { return horizontalSensitivity; }
            set { horizontalSensitivity = value; }
        }

        public float VerticalSensitivity
        {
            get { return verticalSensitivity; }
            set { verticalSensitivity = value; }
        }

        public float Damping
        {
            get { return damping; }
            set { damping = value; }
        }

        public bool EnableMomentum
        {
            get { return enableMomentum; }
            set { enableMomentum = value; }
        }

        public float Momentum
        {
            get { return momentum; }
            set { momentum = value; }
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        void Start()
        {
            // Auto-detect VirtualCameraBounds if not assigned
            if (virtualCameraBounds == null)
            {
                AutoDetectVirtualCameraBounds();
            }
        }
        
        /// <summary>
        /// Auto-detect VirtualCameraBounds in the scene
        /// First checks the current GameObject, then searches the entire scene
        /// </summary>
        private void AutoDetectVirtualCameraBounds()
        {
            virtualCameraBounds = GetComponent<VirtualCameraBounds>();
            
            if (virtualCameraBounds == null)
            {
                virtualCameraBounds = FindFirstObjectByType<VirtualCameraBounds>();
            }
        }
        
        void LateUpdate()
        {
            // Handle keyboard debug input - allow even if not fully configured/active
            // This makes keyboard controls work as a standalone debug feature
            if (enableKeyboardControls && !isInputLocked && targetCamera != null)
            {
                ProcessKeyboardInput();
            }
            
            if (!IsActive || !isConfigured) return;
            
            // Apply momentum when not dragging
            if (!isDragging)
            {
                ApplyMomentumMovement();
            }
            
            // Apply boundary constraints if enabled
            if (enableBoundary)
            {
                UpdateBoundaryConstraints();
            }
        }
        
        void OnDisable()
        {
            isDragging = false;
            dragVelocity = Vector3.zero;
            momentumVelocity = Vector3.zero;
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Initialize this drag component for a specific camera and mode
        /// </summary>
        /// <param name="camera">The CinemachineCamera to control</param>
        /// <param name="mode">The camera mode this component is associated with</param>
        public void Initialize(CinemachineCamera camera, CameraMode mode)
        {
            targetCamera = camera;
            associatedMode = mode;
            isActive = mode != null && mode.enableDrag;
            
            if (mode != null && mode.dragSettings != null)
            {
                ApplyDragSettings(mode.dragSettings);
            }
        }
        
        /// <summary>
        /// Apply drag settings from a CameraMode's dragSettings
        /// </summary>
        /// <param name="settings">The drag settings to apply</param>
        public void ApplyDragSettings(DragSettings settings)
        {
            if (settings == null) 
            {
                isConfigured = false;
                return;
            }
            
            horizontalMapsTo = settings.horizontalMapsTo;
            horizontalSensitivity = settings.horizontalSensitivity;
            invertHorizontal = settings.invertHorizontal;
            verticalMapsTo = settings.verticalMapsTo;
            verticalSensitivity = settings.verticalSensitivity;
            invertVertical = settings.invertVertical;
            damping = settings.damping;
            enableMomentum = settings.enableMomentum;
            momentum = settings.momentum;
            momentumDecay = settings.momentumDecay;
            maxCameraSpeed = settings.maxCameraSpeed;
            enableBoundary = settings.enableBoundary;
            movementDeltaMode = settings.movementDeltaMode;
            dragDampingMode = settings.dragDampingMode;
            
            isConfigured = true;
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
        /// Set drag sensitivity multiplier (used by zoom component to reduce sensitivity during zoom)
        /// </summary>
        /// <param name="multiplier">Sensitivity multiplier (1.0 = normal, 0.7 = reduced, etc.)</param>
        public void SetDragSensitivityMultiplier(float multiplier)
        {
            currentDragSensitivityMultiplier = Mathf.Clamp(multiplier, 0.1f, 2.0f);
        }
        
        /// <summary>
        /// Stop dragging immediately (used when zoom starts to avoid conflicts)
        /// </summary>
        public void StopDragging()
        {
            if (isDragging)
            {
                isDragging = false;
                dragVelocity = Vector3.zero;
                momentumVelocity = Vector3.zero;
            }
        }
        
        /// <summary>
        /// Lock or unlock user input for drag
        /// </summary>
        /// <param name="locked">True to lock input, false to unlock</param>
        public void SetInputLocked(bool locked)
        {
            isInputLocked = locked;
            
            if (locked && isDragging)
            {
                StopDragging();
            }
            
            if (locked)
            {
                momentumVelocity = Vector3.zero;
            }
        }
        
        /// <summary>
        /// Check if input is currently locked
        /// </summary>
        public bool IsInputLocked => isInputLocked;
        
        /// <summary>
        /// Whether the camera is currently being dragged with exactly 1 finger
        /// </summary>
        public bool IsDragging => isDragging && enabled && isActiveAndEnabled;
        
        /// <summary>
        /// Check if the component is properly configured (matching VirtualCameraDragComponent pattern)
        /// </summary>
        public bool IsConfigured => isConfigured;
        
        #endregion
        
        #region Input Event Handlers
        
        public void OnSingleFingerStart(FingerGestureData data)
        {
            if (!IsActive || !isConfigured || isInputLocked) return;
            
            isDragging = true;
        }
        
        public void OnSingleFingerUpdate(FingerGestureData data)
        {
            if (!IsActive || !isConfigured || !isDragging || targetCamera == null || isInputLocked) return;

            Vector3 targetMovement = Vector3.zero;

            if (movementDeltaMode == MovementDeltaMode.WorldPlaneViaBounds)
            {
                var hasBounds = virtualCameraBounds != null && virtualCameraBounds.IsValid;
                var cam = Camera.main;

                if (!hasBounds || cam == null)
                {
                    AutoDetectVirtualCameraBounds();
                    hasBounds = virtualCameraBounds != null && virtualCameraBounds.IsValid;
                    
                    // If still no bounds, fall back to simple world movement without bounds
                    if (!hasBounds)
                    {
                        ProcessSimpleWorldMovement(data);
                        return;
                    }
                }

                Vector2 currentScreenPos = data.screenPosition;
                Vector2 prevScreenPos = data.screenPosition - data.screenDelta;

                var bounds = virtualCameraBounds.GetBounds();
                float planeY = bounds.center.y;
                var dragPlane = new Plane(Vector3.up, new Vector3(0f, planeY, 0f));

                Ray rayPrev = cam.ScreenPointToRay(prevScreenPos);
                Ray rayCurr = cam.ScreenPointToRay(currentScreenPos);

                if (dragPlane.Raycast(rayPrev, out float enterPrev) && dragPlane.Raycast(rayCurr, out float enterCurr))
                {
                    Vector3 worldPrev = rayPrev.GetPoint(enterPrev);
                    Vector3 worldCurr = rayCurr.GetPoint(enterCurr);
                    Vector3 worldDelta = worldCurr - worldPrev;

                    targetMovement = MapWorldDeltaToConfiguredAxes(worldDelta);
                }
                else
                {
                    return;
                }
            }

            targetMovement = Vector3.ClampMagnitude(targetMovement, maxCameraSpeed * Time.deltaTime);

            if (dragDampingMode == DragDampingMode.ReleaseOnly)
            {
                ApplyAxisMovement(targetMovement);
                lastDragFrameVelocity = targetMovement / Time.deltaTime;
            }
            else
            {
                ApplyDragMovement(targetMovement);
            }
        }
        
        public void OnSingleFingerEnd(FingerGestureData data)
        {
            if (!isDragging) return;
            
            isDragging = false;
            
            // Transfer drag velocity to momentum
            TransferDragToMomentum();
        }
        
        // Pinch gesture handlers (not used by drag component)
        public void OnPinchStart(FingerGestureData data) { }
        public void OnPinchUpdate(FingerGestureData data) { }
        public void OnPinchEnd(FingerGestureData data) { }
        
        #endregion
        
        #region Movement Logic (matching VirtualCameraDragComponent)
        
        /// <summary>
        /// Map a world-space delta to the configured axes using sensitivities and invert flags.
        /// </summary>
        private Vector3 MapWorldDeltaToConfiguredAxes(Vector3 worldDelta)
        {
            Vector3 movement = Vector3.zero;

            if (horizontalMapsTo != VirtualCameraAxis.None)
            {
                var axisVector = GetAxisVector(horizontalMapsTo);
                if (verticalMapsTo == VirtualCameraAxis.WorldY)
                {
                    worldDelta.y = worldDelta.x;
                }
                var projected = Vector3.Project(worldDelta, axisVector);
                if (invertHorizontal) projected *= -1f;
                movement += projected * horizontalSensitivity * currentDragSensitivityMultiplier;
            }

            if (verticalMapsTo != VirtualCameraAxis.None)
            {
                var axisVector = GetAxisVector(verticalMapsTo);
                if(verticalMapsTo == VirtualCameraAxis.WorldY)
                {
                    worldDelta.y = worldDelta.z;
                }

                var projected = Vector3.Project(worldDelta, axisVector);
                if (invertVertical) projected *= -1f;
                movement += projected * verticalSensitivity * currentDragSensitivityMultiplier;
            }

            return movement;
        }
        
        private Vector3 GetAxisVector(VirtualCameraAxis axis)
        {
            switch (axis)
            {
                case VirtualCameraAxis.WorldX: return Vector3.right;
                case VirtualCameraAxis.WorldY: return Vector3.up;
                case VirtualCameraAxis.WorldZ: return Vector3.forward;
                case VirtualCameraAxis.RotationX: return Vector3.right;
                case VirtualCameraAxis.RotationY: return Vector3.up;
                case VirtualCameraAxis.RotationZ: return Vector3.forward;
                default: return Vector3.zero;
            }
        }
        
        /// <summary>
        /// Apply movement during active dragging with damping
        /// </summary>
        private void ApplyDragMovement(Vector3 targetMovement)
        {
            // Convert input to velocity (targetMovement is already delta-based)
            var targetVelocity = targetMovement / Time.deltaTime;
            
            // Apply damping between current drag velocity and target velocity
            var dampingFactor = CwHelper.DampenFactor(damping, Time.deltaTime);
            dragVelocity = Vector3.Lerp(dragVelocity, targetVelocity, dampingFactor);
            
            // Apply the movement (velocity * time = displacement)
            ApplyAxisMovement(dragVelocity * Time.deltaTime);
        }
        
        /// <summary>
        /// Apply momentum movement after drag release
        /// </summary>
        private void ApplyMomentumMovement()
        {
            if (!enableMomentum || momentumVelocity.magnitude < 0.001f)
            {
                momentumVelocity = Vector3.zero;
                return;
            }
            
            // Apply momentum decay
            var decayFactor = CwHelper.DampenFactor(momentumDecay, Time.deltaTime);
            momentumVelocity = Vector3.Lerp(momentumVelocity, Vector3.zero, decayFactor);
            
            // Apply the movement
            ApplyAxisMovement(momentumVelocity * Time.deltaTime);
        }
        
        /// <summary>
        /// Transfer drag velocity to momentum when drag ends
        /// </summary>
        private void TransferDragToMomentum()
        {
            if (enableMomentum)
            {
                // In ReleaseOnly mode we don't maintain dragVelocity during drag,
                // so use the last frame's direct velocity instead.
                var sourceVelocity = dragDampingMode == DragDampingMode.ReleaseOnly ? lastDragFrameVelocity : dragVelocity;
                momentumVelocity = sourceVelocity * momentum;
            }
            else
            {
                momentumVelocity = Vector3.zero;
            }
            
            dragVelocity = Vector3.zero;
            lastDragFrameVelocity = Vector3.zero;
        }
        
        private void ApplyAxisMovement(Vector3 movement)
        {
            // Apply horizontal axis movement if configured
            if (horizontalMapsTo != VirtualCameraAxis.None)
            {
                ApplySingleAxisMovement(movement, horizontalMapsTo);
            }
            
            // Apply vertical axis movement if configured
            if (verticalMapsTo != VirtualCameraAxis.None)
            {
                ApplySingleAxisMovement(movement, verticalMapsTo);
            }
        }
        
        private void ApplySingleAxisMovement(Vector3 movement, VirtualCameraAxis targetAxis)
        {
            var axisVector = GetAxisVector(targetAxis);
            var axisMovement = Vector3.Project(movement, axisVector);
           
            if (axisMovement.magnitude > 0.001f)
            {
                if (IsRotationAxis(targetAxis))
                {
                    ApplyRotation(targetAxis, axisMovement.magnitude * Mathf.Sign(Vector3.Dot(movement, axisVector)));
                }
                else
                {
                    // Apply to CinemachineCamera transform
                    if (targetCamera != null)
                    {
                        targetCamera.transform.position += axisMovement;
                    }
                }
            }
        }
        
        private bool IsRotationAxis(VirtualCameraAxis axis)
        {
            return axis == VirtualCameraAxis.RotationX || axis == VirtualCameraAxis.RotationY || axis == VirtualCameraAxis.RotationZ;
        }
        
        private void ApplyRotation(VirtualCameraAxis axis, float amount)
        {
            if (targetCamera == null) return;
            
            // Apply rotation to CinemachineCamera transform
            switch (axis)
            {
                case VirtualCameraAxis.RotationX:
                    targetCamera.transform.Rotate(amount, 0, 0);
                    break;
                case VirtualCameraAxis.RotationY:
                    targetCamera.transform.Rotate(0, amount, 0);
                    break;
                case VirtualCameraAxis.RotationZ:
                    targetCamera.transform.Rotate(0, 0, amount);
                    break;
            }
        }
        
        #endregion
        
        #region Boundary System (matching VirtualCameraDragComponent)
        
        private void UpdateBoundaryConstraints()
        {
            if (!IsBoundaryValid())
                return;
                
            ValidateBoundaryCache();
            
            Vector3 currentPosition = targetCamera.transform.position;
            
            // Check if position needs constraining using rotation-aware OBB
            bool isInside = virtualCameraBounds.IsPositionInsideOriented(currentPosition);
            
            if (!isInside)
            {
                HandleBoundaryViolation(currentPosition);
            }
            
            previousPosition = targetCamera.transform.position;
        }
        
        private void HandleBoundaryViolation(Vector3 currentPosition)
        {
            Vector3 constrainedPosition = ConstrainPositionToBoundary(currentPosition);
            
            // Hard constraint - move back to boundary
            targetCamera.transform.position = constrainedPosition;
        }
        
        private Vector3 ConstrainPositionToBoundary(Vector3 position)
        {
            // Use rotation-aware constraining that respects bounds orientation
            return virtualCameraBounds.ConstrainPositionOriented(position);
        }
        
        private void ValidateBoundaryCache()
        {
            // Check if VirtualCameraBounds changed
            if (lastVirtualCameraBounds != virtualCameraBounds)
            {
                lastVirtualCameraBounds = virtualCameraBounds;
                InvalidateBoundaryCache();
            }
            
            if (!boundaryDataCached)
            {
                RefreshBoundaryCache();
            }
        }
        
        private void RefreshBoundaryCache()
        {
            if (virtualCameraBounds != null && virtualCameraBounds.IsValid)
            {
                cachedBounds = virtualCameraBounds.GetBounds();
                boundaryDataCached = true;
            }
            else
            {
                cachedBounds = new Bounds();
                boundaryDataCached = false;
            }
        }
        
        private void InvalidateBoundaryCache()
        {
            boundaryDataCached = false;
            boundaryLoggedOnce = false;
        }
        
        /// <summary>
        /// Check if a world position is inside the boundary using rotation-aware OBB
        /// </summary>
        public bool IsPositionInsideBoundary(Vector3 worldPosition)
        {
            if (!IsBoundaryValid())
                return true;
                
            return virtualCameraBounds.IsPositionInsideOriented(worldPosition);
        }
        
        /// <summary>
        /// Get the closest point on the boundary to a given position using rotation-aware constraining
        /// </summary>
        public Vector3 GetClosestBoundaryPoint(Vector3 worldPosition)
        {
            if (!IsBoundaryValid())
                return worldPosition;
                
            return virtualCameraBounds.ConstrainPositionOriented(worldPosition);
        }
        
        /// <summary>
        /// Check if boundary configuration is valid
        /// </summary>
        private bool IsBoundaryValid()
        {
            return enableBoundary && 
                   virtualCameraBounds != null && 
                   virtualCameraBounds.IsValid;
        }
        
        /// <summary>
        /// Set the VirtualCameraBounds component
        /// </summary>
        /// <param name="bounds">VirtualCameraBounds component to use</param>
        public void SetVirtualCameraBounds(VirtualCameraBounds bounds)
        {
            virtualCameraBounds = bounds;
            InvalidateBoundaryCache();
        }
        
        /// <summary>
        /// Get the current VirtualCameraBounds component
        /// </summary>
        public VirtualCameraBounds GetVirtualCameraBounds()
        {
            return virtualCameraBounds;
        }
        
        /// <summary>
        /// Process simple world movement without bounds constraints
        /// Uses proper screen-to-world conversion like original LeanDragCamera
        /// </summary>
        private void ProcessSimpleWorldMovement(FingerGestureData data)
        {
            var camera = Camera.main;
            if (camera == null) return;
            
            Vector2 currentScreenPos = data.screenPosition;
            Vector2 prevScreenPos = data.screenPosition - data.screenDelta;
            
            // Use DepthIntercept like original LeanDragCamera
            var rayPrev = camera.ScreenPointToRay(prevScreenPos);
            var rayCurr = camera.ScreenPointToRay(currentScreenPos);
            
            // Project to a plane at a reasonable depth (like DepthIntercept with Distance = 0)
            // Use camera's current position as reference depth
            float referenceDepth = camera.transform.position.z;
            var plane = new Plane(Vector3.back, new Vector3(0, 0, referenceDepth));
            
            if (plane.Raycast(rayPrev, out float enterPrev) && plane.Raycast(rayCurr, out float enterCurr))
            {
                Vector3 worldPrev = rayPrev.GetPoint(enterPrev);
                Vector3 worldCurr = rayCurr.GetPoint(enterCurr);
                Vector3 worldDelta = worldCurr - worldPrev;
                
                // Map the world delta to configured axes (same as bounds-based movement)
                Vector3 targetMovement = MapWorldDeltaToConfiguredAxes(worldDelta);
                
                // Apply movement directly to camera transform
                if (targetMovement != Vector3.zero)
                {
                    // Apply horizontal axis movement if configured
                    if (horizontalMapsTo != VirtualCameraAxis.None)
                    {
                        ApplySingleAxisMovement(targetMovement, horizontalMapsTo);
                    }
                    
                    // Apply vertical axis movement if configured
                    if (verticalMapsTo != VirtualCameraAxis.None)
                    {
                        ApplySingleAxisMovement(targetMovement, verticalMapsTo);
                    }
                }
            }
        }
        
        #endregion
        
        #region Auto-Detection
        
        /// <summary>
        /// Find VirtualCameraBounds in the scene (public method for editor use)
        /// </summary>
        public void FindVirtualCameraBoundsInScene()
        {
            AutoDetectVirtualCameraBounds();
        }
        
        #endregion
        
        #region Keyboard Debug Controls
        
        /// <summary>
        /// Process WASD keyboard input for debug camera movement
        /// </summary>
        private void ProcessKeyboardInput()
        {
            // Try to get camera if not set
            if (targetCamera == null)
            {
                targetCamera = GetComponent<CinemachineCamera>();
                if (targetCamera == null && debugKeyboardInput)
                {
                    Debug.LogWarning("[CinemachineDragInputComponent] Keyboard controls enabled but no CinemachineCamera found!");
                }
                if (targetCamera == null) return;
            }
            
            Vector2 keyboardInput = Vector2.zero;
            bool isShiftPressed = false;
            float rotationInput = 0f;
            
#if ENABLE_INPUT_SYSTEM
            // Use new Input System
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed) keyboardInput.y -= 1f; // Backward (reversed)
                if (keyboard.sKey.isPressed) keyboardInput.y += 1f; // Forward (reversed)
                if (keyboard.aKey.isPressed) keyboardInput.x += 1f; // Right (reversed)
                if (keyboard.dKey.isPressed) keyboardInput.x -= 1f; // Left (reversed)
                if (keyboard.leftShiftKey.isPressed) isShiftPressed = true; // Speed boost
                if (keyboard.qKey.isPressed) rotationInput -= 1f; // Rotate left (Q)
                if (keyboard.eKey.isPressed) rotationInput += 1f; // Rotate right (E)
            }
#else
            // Use old Input system (legacy)
            if (Input.GetKey(KeyCode.W)) keyboardInput.y -= 1f; // Backward (reversed)
            if (Input.GetKey(KeyCode.S)) keyboardInput.y += 1f; // Forward (reversed)
            if (Input.GetKey(KeyCode.A)) keyboardInput.x += 1f; // Right (reversed)
            if (Input.GetKey(KeyCode.D)) keyboardInput.x -= 1f; // Left (reversed)
            if (Input.GetKey(KeyCode.LeftShift)) isShiftPressed = true; // Speed boost
            if (Input.GetKey(KeyCode.Q)) rotationInput -= 1f; // Rotate left (Q)
            if (Input.GetKey(KeyCode.E)) rotationInput += 1f; // Rotate right (E)
#endif
            
            // Normalize diagonal movement
            if (keyboardInput.magnitude > 1f)
            {
                keyboardInput.Normalize();
            }
            
            // Apply rotation if Q/E keys are pressed (world space Y rotation only)
            if (Mathf.Abs(rotationInput) > 0.01f)
            {
                float rotationAmount = rotationInput * keyboardRotationSpeed * Time.deltaTime;
                // Rotate around world Y axis only (not local space)
                targetCamera.transform.Rotate(0, rotationAmount, 0, Space.World);
                
                if (debugKeyboardInput && Time.frameCount % 60 == 0) // Log every second
                {
                    Debug.Log($"[CinemachineDragInputComponent] Rotation input: {rotationInput}, Amount: {rotationAmount}, Camera: {(targetCamera != null ? targetCamera.name : "NULL")}");
                }
            }
            
            // Apply movement if any input detected
            if (keyboardInput.magnitude > 0.01f)
            {
                if (debugKeyboardInput && Time.frameCount % 60 == 0) // Log every second
                {
                    Debug.Log($"[CinemachineDragInputComponent] Keyboard input: {keyboardInput}, Shift: {isShiftPressed}, Camera: {(targetCamera != null ? targetCamera.name : "NULL")}");
                }
                
                // Convert keyboard input to world movement based on camera orientation
                Vector3 movement = ConvertKeyboardInputToWorldMovement(keyboardInput, isShiftPressed);
                
                // Apply movement directly (pass shift state for proper clamping)
                ApplyKeyboardMovement(movement, isShiftPressed);
            }
        }
        
        /// <summary>
        /// Convert keyboard input (WASD) to world-space movement based on camera orientation
        /// </summary>
        private Vector3 ConvertKeyboardInputToWorldMovement(Vector2 keyboardInput, bool isShiftPressed = false)
        {
            if (targetCamera == null) return Vector3.zero;
            
            // Get camera's forward and right vectors (projected onto XZ plane for ground movement)
            Transform camTransform = targetCamera.transform;
            Vector3 forward = Vector3.ProjectOnPlane(camTransform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(camTransform.right, Vector3.up).normalized;
            
            // Calculate movement direction
            Vector3 movementDirection = (forward * keyboardInput.y + right * keyboardInput.x).normalized;
            
            // Apply speed based on sensitivity settings, with shift boost
            float speedMultiplier = isShiftPressed ? shiftSpeedMultiplier : 1f;
            float baseSpeed = maxCameraSpeed * keyboardSpeedMultiplier * speedMultiplier;
            float movementSpeed = baseSpeed * Time.deltaTime;
            
            // Map to configured axes
            Vector3 worldMovement = Vector3.zero;
            
            // Horizontal axis (typically WorldX)
            if (horizontalMapsTo != VirtualCameraAxis.None)
            {
                var axisVector = GetAxisVector(horizontalMapsTo);
                var projected = Vector3.Project(movementDirection * movementSpeed, axisVector);
                if (invertHorizontal) projected *= -1f;
                worldMovement += projected * horizontalSensitivity;
            }
            
            // Vertical axis (typically WorldZ)
            if (verticalMapsTo != VirtualCameraAxis.None)
            {
                var axisVector = GetAxisVector(verticalMapsTo);
                var projected = Vector3.Project(movementDirection * movementSpeed, axisVector);
                if (invertVertical) projected *= -1f;
                worldMovement += projected * verticalSensitivity;
            }
            
            return worldMovement;
        }
        
        /// <summary>
        /// Apply keyboard movement to the camera
        /// </summary>
        private void ApplyKeyboardMovement(Vector3 movement, bool isShiftPressed = false)
        {
            if (movement.magnitude < 0.001f) return;
            
            // Clamp to max speed (include shift multiplier in clamp calculation)
            float speedMultiplier = isShiftPressed ? shiftSpeedMultiplier : 1f;
            float maxMovementSpeed = maxCameraSpeed * keyboardSpeedMultiplier * speedMultiplier * Time.deltaTime;
            movement = Vector3.ClampMagnitude(movement, maxMovementSpeed);
            
            // Apply movement based on configured axes
            ApplyAxisMovement(movement);
        }
        
        #endregion
        
        #region Editor Support
        
#if UNITY_EDITOR
        void OnValidate()
        {
            // Invalidate boundary cache when boundary collider changes in editor
            InvalidateBoundaryCache();
        }
#endif
        
        #endregion
    }
}
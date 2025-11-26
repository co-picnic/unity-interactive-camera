using UnityEngine;
using Unity.Cinemachine;

namespace InteractiveCameraSystem
{
    /// <summary>
    /// Custom Cinemachine extension for edge-triggered camera panning.
    /// Provides smooth camera movement when input is detected near screen edges.
    /// </summary>
    [SaveDuringPlay]
    [AddComponentMenu("InteractiveCameraSystem/Cinemachine/Cinemachine Edge Pan")]
    public class CinemachineEdgePan : CinemachineExtension
    {
        [Header("Edge Pan Settings")]
        [Tooltip("Size of edge zones as fraction of screen (0.15 = 15% from each edge)")]
        [Range(0.05f, 0.4f)]
        public float edgeZoneSize = 0.15f;
        
        [Tooltip("Speed of camera panning movement")]
        [Range(0.1f, 10.0f)]
        public float panSpeed = 2.0f;
        
        [Tooltip("Sensitivity multiplier for pan direction")]
        public Vector2 panSensitivity = Vector2.one;
        
        [Tooltip("Smooth interpolation speed for panning")]
        [Range(0.1f, 5.0f)]
        public float smoothingSpeed = 4.0f;
        
        [Header("Debug")]
        [Tooltip("Show debug information in console")]
        public bool debugMode = false;
        
        // Current pan direction from external input
        private Vector2 currentPanDirection = Vector2.zero;
        private Vector2 targetPanDirection = Vector2.zero;
        
        // Smooth panning state
        private Vector2 smoothedPanDirection = Vector2.zero;
        
        // Plane-based movement (like CinemachineDragInputComponent)
        private Camera mainCamera;
        private float planeY = 0f; // Ground plane Y coordinate
        
        // Target camera reference (like CinemachineDragInputComponent)
        private CinemachineCamera targetCamera;
        
        /// <summary>
        /// Initialize this edge pan component for a specific camera (like CinemachineDragInputComponent)
        /// </summary>
        /// <param name="camera">The CinemachineCamera to control</param>
        public void Initialize(CinemachineCamera camera)
        {
            targetCamera = camera;
        }
        
        /// <summary>
        /// Set the pan direction from external input (e.g., DragComponent)
        /// </summary>
        /// <param name="direction">Normalized pan direction (-1 to 1)</param>
        public void SetPanDirection(Vector2 direction)
        {
            targetPanDirection = Vector2.ClampMagnitude(direction, 1f);
            
            if (debugMode && direction.magnitude > 0.001f)
            {
                Debug.Log($"[CinemachineEdgePan] SetPanDirection called: {direction:F3}");
            }
        }
        
        /// <summary>
        /// Get current edge zone boundaries for external systems
        /// </summary>
        /// <returns>Edge zone boundaries (min, max) as normalized coordinates</returns>
        public (float min, float max) GetEdgeZoneBoundaries()
        {
            return (edgeZoneSize, 1.0f - edgeZoneSize);
        }
        
        /// <summary>
        /// Check if a normalized screen position is within edge zones
        /// </summary>
        /// <param name="normalizedPosition">Screen position normalized to 0-1</param>
        /// <returns>True if position is in edge zone</returns>
        public bool IsInEdgeZone(Vector2 normalizedPosition)
        {
            bool leftEdge = normalizedPosition.x < edgeZoneSize;
            bool rightEdge = normalizedPosition.x > (1.0f - edgeZoneSize);
            bool bottomEdge = normalizedPosition.y < edgeZoneSize;
            bool topEdge = normalizedPosition.y > (1.0f - edgeZoneSize);
            
            bool inEdgeZone = leftEdge || rightEdge || bottomEdge || topEdge;
            
            if (debugMode && inEdgeZone)
            {
                Debug.Log($"[CinemachineEdgePan] Position {normalizedPosition:F3} in edge zone: L={leftEdge}, R={rightEdge}, B={bottomEdge}, T={topEdge}");
            }
            
            return inEdgeZone;
        }
        
        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage,
            ref CameraState state,
            float deltaTime)
        {
            if (debugMode)
            {
                Debug.Log($"[CinemachineEdgePan] PostPipelineStageCallback called: stage={stage}, targetDirection={targetPanDirection:F3}, deltaTime={deltaTime:F3}");
            }
            
            // Only apply panning during the Body stage (position)
            if (stage == CinemachineCore.Stage.Body)
            {
                // Smooth the pan direction
                smoothedPanDirection = Vector2.Lerp(
                    smoothedPanDirection, 
                    targetPanDirection, 
                    smoothingSpeed * deltaTime
                );
                
                if (debugMode)
                {
                    Debug.Log($"[CinemachineEdgePan] Smoothed direction: {smoothedPanDirection:F3}, magnitude: {smoothedPanDirection.magnitude:F3}");
                }
                
                // Apply panning to camera position using plane-based movement (like CinemachineDragInputComponent)
                if (smoothedPanDirection.magnitude > 0.001f)
                {
                    Vector3 panMovement = CalculatePlaneBasedPanMovement(smoothedPanDirection, deltaTime);
                    
                    if (debugMode)
                    {
                        Debug.Log($"[CinemachineEdgePan] Calculated pan movement: {panMovement:F3}");
                    }
                    
                    // Apply movement directly to target camera transform (like CinemachineDragInputComponent)
                    if (targetCamera != null)
                    {
                        Vector3 oldPosition = targetCamera.transform.position;
                        targetCamera.transform.position += panMovement;
                        
                        if (debugMode)
                        {
                            Debug.Log($"[CinemachineEdgePan] Camera moved from {oldPosition:F3} to {targetCamera.transform.position:F3}");
                        }
                    }
                    else
                    {
                        if (debugMode)
                        {
                            Debug.LogWarning($"[CinemachineEdgePan] Target camera is null!");
                        }
                    }
                }
                else if (debugMode && targetPanDirection.magnitude > 0.001f)
                {
                    Debug.Log($"[CinemachineEdgePan] Target direction has magnitude {targetPanDirection.magnitude:F3} but smoothed is {smoothedPanDirection.magnitude:F3}");
                }
                else if (debugMode)
                {
                    Debug.Log($"[CinemachineEdgePan] No movement - smoothed direction magnitude: {smoothedPanDirection.magnitude:F3}");
                }
            }
        }
        
        /// <summary>
        /// Reset panning state (useful when switching camera modes)
        /// </summary>
        public void ResetPanning()
        {
            currentPanDirection = Vector2.zero;
            targetPanDirection = Vector2.zero;
            smoothedPanDirection = Vector2.zero;
        }
        
        private void Awake()
        {
            // Initialize camera reference
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<Camera>();
            }
            
            // Debug: Check if this extension is properly set up
            if (debugMode)
            {
                Debug.Log($"[CinemachineEdgePan] Awake called on {gameObject.name}");
            }
        }
        
        private void Start()
        {
            // Debug: Check if we're properly attached to a Cinemachine camera
            if (debugMode)
            {
                var vcam = GetComponent<CinemachineVirtualCameraBase>();
                Debug.Log($"[CinemachineEdgePan] Start called - VirtualCamera: {vcam?.name ?? "null"}");
            }
        }
        
        private void Update()
        {
        // Apply panning in Update instead of PostPipelineStageCallback (like CinemachineDragInputComponent)
        if (targetCamera != null && targetPanDirection.magnitude > 0.1f) // Increased threshold to reduce micro-movements
        {
            // Smooth the pan direction
            smoothedPanDirection = Vector2.Lerp(
                smoothedPanDirection, 
                targetPanDirection, 
                smoothingSpeed * Time.deltaTime
            );
            
            if (debugMode)
            {
                Debug.Log($"[CinemachineEdgePan] Update: targetDirection={targetPanDirection:F3}, smoothedDirection={smoothedPanDirection:F3}");
            }
            
            // Apply panning to camera position using plane-based movement
            if (smoothedPanDirection.magnitude > 0.05f) // Increased threshold for movement
            {
                Vector3 panMovement = CalculatePlaneBasedPanMovement(smoothedPanDirection, Time.deltaTime);
                
                // Only apply movement if it's significant enough to avoid stutter
                if (panMovement.magnitude > 0.001f)
                {
                    if (debugMode)
                    {
                        Debug.Log($"[CinemachineEdgePan] Update: Calculated pan movement: {panMovement:F3}");
                    }
                    
                    // Apply movement directly to target camera transform
                    if (targetCamera != null)
                    {
                        Vector3 oldPosition = targetCamera.transform.position;
                        targetCamera.transform.position += panMovement;
                        
                        if (debugMode)
                        {
                            Debug.Log($"[CinemachineEdgePan] Update: Camera moved from {oldPosition:F3} to {targetCamera.transform.position:F3}");
                        }
                    }
                }
            }
        }
        else if (targetPanDirection.magnitude <= 0.1f)
        {
            // Reset smoothed direction when target is below threshold to prevent drift
            smoothedPanDirection = Vector2.Lerp(smoothedPanDirection, Vector2.zero, smoothingSpeed * Time.deltaTime);
        }
        }
        
        /// <summary>
        /// Calculate plane-based pan movement (similar to CinemachineDragInputComponent)
        /// </summary>
        /// <param name="panDirection">Normalized pan direction (-1 to 1)</param>
        /// <param name="deltaTime">Time delta</param>
        /// <returns>World space movement vector</returns>
        private Vector3 CalculatePlaneBasedPanMovement(Vector2 panDirection, float deltaTime)
        {
            // Use Camera.main for raycasting (like CinemachineDragInputComponent)
            Camera activeCamera = Camera.main;
            
            if (activeCamera == null)
            {
                if (debugMode)
                {
                    Debug.LogWarning($"[CinemachineEdgePan] No active camera found for panning");
                }
                return Vector3.zero;
            }
            
            // Create a drag plane at the ground level (like CinemachineDragInputComponent)
            var dragPlane = new Plane(Vector3.up, new Vector3(0f, planeY, 0f));
            
            // Get camera center screen position
            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            
            // Calculate screen delta based on pan direction and speed (reduced for smoother movement)
            Vector2 screenDelta = panDirection * panSpeed * deltaTime * 10f; // Much smaller scale factor to reduce stutter
            
            // Convert screen positions to world positions on the drag plane
            Vector2 currentScreenPos = screenCenter;
            Vector2 prevScreenPos = screenCenter - screenDelta;
            
            Ray rayPrev = activeCamera.ScreenPointToRay(prevScreenPos);
            Ray rayCurr = activeCamera.ScreenPointToRay(currentScreenPos);
            
            if (dragPlane.Raycast(rayPrev, out float enterPrev) && dragPlane.Raycast(rayCurr, out float enterCurr))
            {
                Vector3 worldPrev = rayPrev.GetPoint(enterPrev);
                Vector3 worldCurr = rayCurr.GetPoint(enterCurr);
                Vector3 worldDelta = worldCurr - worldPrev;
                
                // Apply sensitivity (matching CinemachineDragInputComponent pattern)
                worldDelta.x *= panSensitivity.x;
                worldDelta.z *= panSensitivity.y;
                worldDelta.y = 0f; // Keep on ground plane
                
                if (debugMode)
                {
                    Debug.Log($"[CinemachineEdgePan] Plane calculation: screenDelta={screenDelta:F2}, worldDelta={worldDelta:F3}, camera={activeCamera.name}, planeY={planeY:F2}");
                }
                
                return worldDelta;
            }
            else
            {
                if (debugMode)
                {
                    Debug.LogWarning($"[CinemachineEdgePan] Raycast failed: rayPrev={rayPrev.origin:F3}, rayCurr={rayCurr.origin:F3}, planeY={planeY:F2}");
                }
            }
            
            if (debugMode)
            {
                Debug.LogWarning($"[CinemachineEdgePan] Failed to raycast to drag plane at Y={planeY}");
            }
            
            return Vector3.zero;
        }
        
        /// <summary>
        /// Set the ground plane Y coordinate for panning calculations
        /// </summary>
        /// <param name="y">Y coordinate of the ground plane</param>
        public void SetPlaneY(float y)
        {
            planeY = y;
        }
        
        private void OnValidate()
        {
            // Clamp values in inspector
            edgeZoneSize = Mathf.Clamp(edgeZoneSize, 0.05f, 0.4f);
            panSpeed = Mathf.Clamp(panSpeed, 0.1f, 10.0f);
            smoothingSpeed = Mathf.Clamp(smoothingSpeed, 0.1f, 5.0f);
        }
    }
}

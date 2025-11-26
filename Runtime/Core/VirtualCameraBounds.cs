using UnityEngine;

namespace InteractiveCameraSystem
{
    /// <summary>
    /// Component that defines and visualizes camera movement boundaries
    /// Works with any type of collider (BoxCollider, CapsuleCollider, etc.)
    /// Provides runtime visualization and boundary constraint functionality
    /// </summary>

    public class VirtualCameraBounds : MonoBehaviour
    {
        [Header("Visual Configuration")]
        [Tooltip("Show bounds visualization in runtime (not just editor)")]
        public bool showInRuntime = true;
        
        [Tooltip("Show bounds visualization in editor")]
        public bool showInEditor = true;
        
        [Tooltip("Color of the bounds wireframe")]
        public Color boundsColor = Color.yellow;
        
        [Tooltip("Alpha/transparency of the bounds visualization (0-1)")]
        [Range(0f, 1f)]
        public float wireframeAlpha = 0.7f;
        
        [Tooltip("Thickness of the wireframe lines in runtime")]
        [Range(0.01f, 0.1f)]
        public float lineThickness = 0.02f;
        
        [Header("Bounds Configuration")]
        [Tooltip("The collider that defines the boundary shape (BoxCollider, CapsuleCollider, etc.)")]
        [SerializeField] private Collider boundaryCollider;
        
        [Tooltip("Multiplier applied to the boundary size (1.0 = original size, 2.0 = double size, 0.5 = half size)")]
        [Range(0.1f, 5.0f)]
        public float boundsMultiplier = 1.0f;
        
        [Tooltip("Show center point indicator")]
        public bool showCenterPoint = true;
        
        [Tooltip("Show corner indicators")]
        public bool showCornerIndicators = false;
        
        [Header("Debug")]
        [Tooltip("Show debug information in console")]
        public bool debugMode = false;
        
        // Cache system
        private Bounds cachedBounds;
        private bool boundsCached = false;
        private Collider lastBoundaryCollider;
        private float lastBoundsMultiplier;
        
        // OBB (Oriented Bounding Box) system for rotation-aware bounds
        private bool isBoxCollider = false;
        private Vector3 cachedLocalExtents;
        private Matrix4x4 cachedTransformMatrix;
        private Matrix4x4 cachedInverseTransformMatrix;
        private bool obbCached = false;
        private bool lastTransformHasChanged = false;
        
        // Runtime visualization - simplified
        private bool lastShowInRuntime = false;
        
        #region Properties
        
        /// <summary>
        /// Get the collider used for boundary definition
        /// </summary>
        public Collider BoundaryCollider 
        { 
            get => boundaryCollider; 
            set => SetBoundaryCollider(value); 
        }
        
        /// <summary>
        /// Get cached bounds of the boundary collider
        /// </summary>
        public Bounds Bounds
        {
            get
            {
                ValidateCache();
                return cachedBounds;
            }
        }
        
        /// <summary>
        /// Check if the bounds component is properly configured
        /// </summary>
        public bool IsValid => boundaryCollider != null;
        
        #endregion
        
        #region Unity Lifecycle
        
        void Awake()
        {
            // Auto-assign boundary collider if not set
            if (boundaryCollider == null)
            {
                boundaryCollider = GetComponent<Collider>();
            }
            
            // Initialize cache tracking variables
            lastBoundsMultiplier = boundsMultiplier;
            
            ValidateSetup();
        }
        
        void Start()
        {
            lastShowInRuntime = showInRuntime;
            
            if (debugMode && IsValid)
            {
                Debug.Log($"[VirtualCameraBounds] {gameObject.name} bounds initialized - Size: {Bounds.size}");
            }
        }
        
        void Update()
        {
            // Simple runtime visualization using Debug.DrawLine
            if (showInRuntime && IsValid)
            {
                DrawRuntimeWireframe();
            }
        }
        

        
        void OnValidate()
        {
            // Invalidate cache when properties change
            InvalidateCache();
            
            // Sync tracking variables to prevent false positives in runtime change detection
            lastBoundsMultiplier = boundsMultiplier;
            
            // Auto-assign boundary collider if not set
            if (boundaryCollider == null)
            {
                boundaryCollider = GetComponent<Collider>();
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Check if a world position is inside the boundary
        /// </summary>
        /// <param name="position">World position to check</param>
        /// <returns>True if position is inside the boundary</returns>
        public bool IsPositionInside(Vector3 position)
        {
            if (!IsValid) return true;
            
            ValidateCache();
            return cachedBounds.Contains(position);
        }
        
        /// <summary>
        /// Constrain a position to be within the boundary
        /// </summary>
        /// <param name="position">Position to constrain</param>
        /// <returns>Constrained position within boundary</returns>
        public Vector3 ConstrainPosition(Vector3 position)
        {
            if (!IsValid) return position;
            
            ValidateCache();
            return cachedBounds.ClosestPoint(position);
        }
        
        /// <summary>
        /// Get the closest point on the boundary to a given position
        /// </summary>
        /// <param name="position">Position to find closest point to</param>
        /// <returns>Closest point on boundary</returns>
        public Vector3 GetClosestPoint(Vector3 position)
        {
            return ConstrainPosition(position);
        }
        
        /// <summary>
        /// Get the bounds of the boundary collider
        /// </summary>
        /// <returns>Bounds of the boundary</returns>
        public Bounds GetBounds()
        {
            ValidateCache();
            return cachedBounds;
        }
        
        /// <summary>
        /// Set the boundary collider (can be called at runtime)
        /// </summary>
        /// <param name="collider">New boundary collider</param>
        public void SetBoundaryCollider(Collider collider)
        {
            boundaryCollider = collider;
            InvalidateCache();
            
            if (debugMode)
            {
                string colliderName = collider != null ? collider.name : "null";
                Debug.Log($"[VirtualCameraBounds] Boundary collider set to: {colliderName}");
            }
            
            // Runtime visualization will update automatically in Update() method
        }
        
        /// <summary>
        /// Force refresh of cached bounds data
        /// </summary>
        public void RefreshBounds()
        {
            InvalidateCache();
            ValidateCache();
        }
        
        /// <summary>
        /// Check if a world position is inside the boundary using rotation-aware OBB
        /// </summary>
        /// <param name="position">World position to check</param>
        /// <returns>True if position is inside the oriented boundary</returns>
        public bool IsPositionInsideOriented(Vector3 position)
        {
            if (!IsValid) return true;
            
            ValidateOBBCache();
            
            if (isBoxCollider)
            {
                // Use OBB for BoxCollider
                Vector3 localPos = cachedInverseTransformMatrix.MultiplyPoint3x4(position);
                Vector3 scaledExtents = cachedLocalExtents * boundsMultiplier;
                
                return Mathf.Abs(localPos.x) <= scaledExtents.x && 
                       Mathf.Abs(localPos.y) <= scaledExtents.y && 
                       Mathf.Abs(localPos.z) <= scaledExtents.z;
            }
            else
            {
                // Fallback to collider geometry for other types
                Vector3 closestPoint = boundaryCollider.ClosestPoint(position);
                return Vector3.Distance(closestPoint, position) < 0.001f;
            }
        }
        
        /// <summary>
        /// Constrain a position to be within the oriented boundary
        /// </summary>
        /// <param name="position">Position to constrain</param>
        /// <returns>Constrained position within oriented boundary</returns>
        public Vector3 ConstrainPositionOriented(Vector3 position)
        {
            if (!IsValid) return position;
            
            ValidateOBBCache();
            
            if (isBoxCollider)
            {
                // Use OBB for BoxCollider - transform to local space, clamp, transform back
                Vector3 localPos = cachedInverseTransformMatrix.MultiplyPoint3x4(position);
                Vector3 scaledExtents = cachedLocalExtents * boundsMultiplier;
                
                // Clamp local coordinates to box extents
                Vector3 clampedLocal = new Vector3(
                    Mathf.Clamp(localPos.x, -scaledExtents.x, scaledExtents.x),
                    localPos.y, // Don't clamp Y for camera movement (keep original Y)
                    Mathf.Clamp(localPos.z, -scaledExtents.z, scaledExtents.z)
                );
                
                // Transform back to world space
                Vector3 clampedWorld = cachedTransformMatrix.MultiplyPoint3x4(clampedLocal);
                return new Vector3(clampedWorld.x, position.y, clampedWorld.z);
            }
            else
            {
                // Fallback to collider geometry
                Vector3 closestPoint = boundaryCollider.ClosestPoint(position);
                return new Vector3(closestPoint.x, position.y, closestPoint.z);
            }
        }
        
        #endregion
        
        #region Cache Management
        
        private void ValidateCache()
        {
            bool cacheInvalidated = false;
            
            // Check if collider reference changed
            if (lastBoundaryCollider != boundaryCollider)
            {
                lastBoundaryCollider = boundaryCollider;
                InvalidateCache();
                cacheInvalidated = true;
            }
            
            // Check if bounds multiplier changed at runtime
            if (!Mathf.Approximately(lastBoundsMultiplier, boundsMultiplier))
            {
                lastBoundsMultiplier = boundsMultiplier;
                InvalidateCache();
                cacheInvalidated = true;
                
                if (debugMode && IsValid)
                {
                    Debug.Log($"[VirtualCameraBounds] Bounds multiplier changed to {boundsMultiplier:F2} - invalidating cache");
                }
            }
            
            // Check if transform changed (for rotation-aware bounds)
            if (IsValid && boundaryCollider.transform.hasChanged != lastTransformHasChanged)
            {
                lastTransformHasChanged = boundaryCollider.transform.hasChanged;
                if (lastTransformHasChanged)
                {
                    InvalidateCache();
                    cacheInvalidated = true;
                    
                    // Reset the hasChanged flag after we've detected the change
                    boundaryCollider.transform.hasChanged = false;
                    
                    if (debugMode)
                    {
                        Debug.Log($"[VirtualCameraBounds] Transform changed - invalidating cache");
                    }
                }
            }
            
            if (!boundsCached && IsValid)
            {
                RefreshCache();
            }
        }
        
        /// <summary>
        /// Validate OBB cache for rotation-aware bounds
        /// </summary>
        private void ValidateOBBCache()
        {
            // First validate the regular cache (handles transform changes)
            ValidateCache();
            
            // Then validate OBB-specific cache
            if (!obbCached && IsValid)
            {
                RefreshOBBCache();
            }
        }
        
        private void RefreshCache()
        {
            if (boundaryCollider != null)
            {
                Bounds originalBounds = boundaryCollider.bounds;
                
                // Apply bounds multiplier to the size while keeping the center
                Vector3 scaledSize = originalBounds.size * boundsMultiplier;
                cachedBounds = new Bounds(originalBounds.center, scaledSize);
                boundsCached = true;
                
                if (debugMode)
                {
                    Debug.Log($"[VirtualCameraBounds] Bounds cache refreshed - Original Size: {originalBounds.size}, Scaled Size: {cachedBounds.size}, Multiplier: {boundsMultiplier}");
                }
            }
            else
            {
                cachedBounds = new Bounds();
                boundsCached = false;
            }
        }
        
        /// <summary>
        /// Refresh OBB cache for rotation-aware bounds
        /// </summary>
        private void RefreshOBBCache()
        {
            if (boundaryCollider != null)
            {
                // Check if this is a BoxCollider for optimized OBB
                BoxCollider boxCollider = boundaryCollider as BoxCollider;
                isBoxCollider = boxCollider != null;
                
                if (isBoxCollider)
                {
                    // Cache transform matrices for efficient local<->world conversion
                    cachedTransformMatrix = boundaryCollider.transform.localToWorldMatrix;
                    cachedInverseTransformMatrix = boundaryCollider.transform.worldToLocalMatrix;
                    
                    // Cache local extents (half-size) accounting for BoxCollider's size and center
                    Vector3 colliderSize = boxCollider.size;
                    cachedLocalExtents = colliderSize * 0.5f;
                    
                    if (debugMode)
                    {
                        Debug.Log($"[VirtualCameraBounds] OBB cache refreshed for BoxCollider - Extents: {cachedLocalExtents}, Rotation: {boundaryCollider.transform.rotation.eulerAngles}");
                    }
                }
                else
                {
                    if (debugMode)
                    {
                        Debug.Log($"[VirtualCameraBounds] Using fallback collider geometry for {boundaryCollider.GetType().Name}");
                    }
                }
                
                obbCached = true;
            }
            else
            {
                isBoxCollider = false;
                obbCached = false;
            }
        }
        
        private void InvalidateCache()
        {
            boundsCached = false;
            obbCached = false;
        }
        
        #endregion
        
        #region Runtime Visualization
        
        /// <summary>
        /// Draw runtime wireframe using Debug.DrawLine - rotation-aware for BoxCollider
        /// </summary>
        private void DrawRuntimeWireframe()
        {
            ValidateOBBCache();
            
            // Prepare line color with alpha
            Color lineColor = new Color(boundsColor.r, boundsColor.g, boundsColor.b, wireframeAlpha);
            float duration = Time.deltaTime + 0.01f; // Small buffer to prevent flickering
            
            Vector3[] corners = GetOrientedCorners();
            
            // Draw 12 edges of the box using Debug.DrawLine
            // Bottom face (4 edges)
            Debug.DrawLine(corners[0], corners[1], lineColor, duration);
            Debug.DrawLine(corners[1], corners[2], lineColor, duration);
            Debug.DrawLine(corners[2], corners[3], lineColor, duration);
            Debug.DrawLine(corners[3], corners[0], lineColor, duration);
            
            // Top face (4 edges)
            Debug.DrawLine(corners[4], corners[5], lineColor, duration);
            Debug.DrawLine(corners[5], corners[6], lineColor, duration);
            Debug.DrawLine(corners[6], corners[7], lineColor, duration);
            Debug.DrawLine(corners[7], corners[4], lineColor, duration);
            
            // Vertical edges (4 edges)
            Debug.DrawLine(corners[0], corners[4], lineColor, duration);
            Debug.DrawLine(corners[1], corners[5], lineColor, duration);
            Debug.DrawLine(corners[2], corners[6], lineColor, duration);
            Debug.DrawLine(corners[3], corners[7], lineColor, duration);
            
            // Optional: Draw center point
            if (showCenterPoint)
            {
                Color centerColor = new Color(boundsColor.r, boundsColor.g, boundsColor.b, 0.8f);
                float centerSize = 0.1f;
                Vector3 center = isBoxCollider ? cachedTransformMatrix.MultiplyPoint3x4(Vector3.zero) : cachedBounds.center;
                
                Vector3 right = isBoxCollider ? cachedTransformMatrix.MultiplyVector(Vector3.right).normalized : Vector3.right;
                Vector3 up = isBoxCollider ? cachedTransformMatrix.MultiplyVector(Vector3.up).normalized : Vector3.up;
                Vector3 forward = isBoxCollider ? cachedTransformMatrix.MultiplyVector(Vector3.forward).normalized : Vector3.forward;
                
                Debug.DrawLine(center - right * centerSize, center + right * centerSize, centerColor, duration);
                Debug.DrawLine(center - up * centerSize, center + up * centerSize, centerColor, duration);
                Debug.DrawLine(center - forward * centerSize, center + forward * centerSize, centerColor, duration);
            }
        }
        
        /// <summary>
        /// Get oriented corners for visualization (works for both BoxCollider and fallback)
        /// </summary>
        private Vector3[] GetOrientedCorners()
        {
            Vector3[] corners = new Vector3[8];
            
            if (isBoxCollider)
            {
                // Use OBB corners for BoxCollider
                Vector3 scaledExtents = cachedLocalExtents * boundsMultiplier;
                
                // Define local corners
                Vector3[] localCorners = new Vector3[8];
                localCorners[0] = new Vector3(-scaledExtents.x, -scaledExtents.y, -scaledExtents.z); // bottom-back-left
                localCorners[1] = new Vector3(scaledExtents.x, -scaledExtents.y, -scaledExtents.z);  // bottom-back-right
                localCorners[2] = new Vector3(scaledExtents.x, -scaledExtents.y, scaledExtents.z);   // bottom-front-right
                localCorners[3] = new Vector3(-scaledExtents.x, -scaledExtents.y, scaledExtents.z);  // bottom-front-left
                localCorners[4] = new Vector3(-scaledExtents.x, scaledExtents.y, -scaledExtents.z);  // top-back-left
                localCorners[5] = new Vector3(scaledExtents.x, scaledExtents.y, -scaledExtents.z);   // top-back-right
                localCorners[6] = new Vector3(scaledExtents.x, scaledExtents.y, scaledExtents.z);    // top-front-right
                localCorners[7] = new Vector3(-scaledExtents.x, scaledExtents.y, scaledExtents.z);   // top-front-left
                
                // Transform to world space
                for (int i = 0; i < 8; i++)
                {
                    corners[i] = cachedTransformMatrix.MultiplyPoint3x4(localCorners[i]);
                }
            }
            else
            {
                // Fallback to axis-aligned corners for non-BoxCollider
                Vector3 center = cachedBounds.center;
                Vector3 size = cachedBounds.size;
                
                corners[0] = center + new Vector3(-size.x, -size.y, -size.z) * 0.5f;
                corners[1] = center + new Vector3(size.x, -size.y, -size.z) * 0.5f;
                corners[2] = center + new Vector3(size.x, -size.y, size.z) * 0.5f;
                corners[3] = center + new Vector3(-size.x, -size.y, size.z) * 0.5f;
                corners[4] = center + new Vector3(-size.x, size.y, -size.z) * 0.5f;
                corners[5] = center + new Vector3(size.x, size.y, -size.z) * 0.5f;
                corners[6] = center + new Vector3(size.x, size.y, size.z) * 0.5f;
                corners[7] = center + new Vector3(-size.x, size.y, size.z) * 0.5f;
            }
            
            return corners;
        }
        
        #endregion
        
        #region Validation
        
        private void ValidateSetup()
        {
            if (boundaryCollider == null)
            {
                Debug.LogError($"[VirtualCameraBounds] {gameObject.name} requires a Collider component for boundary definition");
                return;
            }
            
            // Ensure the collider is marked as trigger if it's being used purely for bounds
            if (!boundaryCollider.isTrigger && debugMode)
            {
                Debug.LogWarning($"[VirtualCameraBounds] {gameObject.name} boundary collider is not marked as trigger - this may cause physics interactions");
            }
        }
        
        #endregion
        
        #region Editor Gizmos
        
#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!showInEditor || !IsValid) return;
            
            DrawBoundsGizmos(false);
            
            // Always show label even when not selected
            ValidateCache();
            UnityEditor.Handles.Label(
                cachedBounds.center + Vector3.up * (cachedBounds.size.y * 0.5f + 0.5f),
                $"{gameObject.name} Camera Bounds"
            );
        }
        
        void OnDrawGizmosSelected()
        {
            if (!showInEditor || !IsValid) return;
            
            DrawBoundsGizmos(true);
        }
        
        private void DrawBoundsGizmos(bool selected)
        {
            ValidateOBBCache();
            
            Color gizmoColor = selected ? 
                new Color(boundsColor.r, boundsColor.g, boundsColor.b, 1f) : 
                new Color(boundsColor.r, boundsColor.g, boundsColor.b, wireframeAlpha);
            
            if (isBoxCollider)
            {
                // Draw oriented bounds for BoxCollider
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = cachedTransformMatrix;
                
                Vector3 scaledSize = cachedLocalExtents * 2f * boundsMultiplier; // Convert extents to size
                
                Gizmos.color = gizmoColor;
                Gizmos.DrawWireCube(Vector3.zero, scaledSize);
                
                if (selected)
                {
                    // Draw filled box with low alpha when selected
                    Gizmos.color = new Color(boundsColor.r, boundsColor.g, boundsColor.b, 0.1f);
                    Gizmos.DrawCube(Vector3.zero, scaledSize);
                }
                
                Gizmos.matrix = oldMatrix;
                
                // Draw center point in world space
                if (showCenterPoint)
                {
                    Gizmos.color = new Color(boundsColor.r, boundsColor.g, boundsColor.b, 0.8f);
                    Vector3 center = cachedTransformMatrix.MultiplyPoint3x4(Vector3.zero);
                    Gizmos.DrawSphere(center, 0.1f);
                }
                
                // Draw corner indicators
                if (showCornerIndicators && selected)
                {
                    Gizmos.color = new Color(boundsColor.r, boundsColor.g, boundsColor.b, 0.6f);
                    Vector3[] corners = GetOrientedCorners();
                    
                    foreach (var corner in corners)
                    {
                        Gizmos.DrawSphere(corner, 0.05f);
                    }
                }
                
                // Additional details when selected
                if (selected)
                {
                    Vector3 center = cachedTransformMatrix.MultiplyPoint3x4(Vector3.zero);
                    UnityEditor.Handles.Label(
                        center + Vector3.up * (scaledSize.y * 0.5f + 1.0f),
                        $"Oriented Size: {scaledSize.x:F1} x {scaledSize.y:F1} x {scaledSize.z:F1}\nRotation: {boundaryCollider.transform.rotation.eulerAngles}"
                    );
                }
            }
            else
            {
                // Fallback to axis-aligned bounds for non-BoxCollider
                Gizmos.color = gizmoColor;
                Gizmos.DrawWireCube(cachedBounds.center, cachedBounds.size);
                
                if (selected)
                {
                    // Draw filled box with low alpha when selected
                    Gizmos.color = new Color(boundsColor.r, boundsColor.g, boundsColor.b, 0.1f);
                    Gizmos.DrawCube(cachedBounds.center, cachedBounds.size);
                }
                
                // Draw center point
                if (showCenterPoint)
                {
                    Gizmos.color = new Color(boundsColor.r, boundsColor.g, boundsColor.b, 0.8f);
                    Gizmos.DrawSphere(cachedBounds.center, 0.1f);
                }
                
                // Draw corner indicators
                if (showCornerIndicators && selected)
                {
                    Gizmos.color = new Color(boundsColor.r, boundsColor.g, boundsColor.b, 0.6f);
                    Vector3[] corners = GetOrientedCorners();
                    
                    foreach (var corner in corners)
                    {
                        Gizmos.DrawSphere(corner, 0.05f);
                    }
                }
                
                // Additional details when selected
                if (selected)
                {
                    UnityEditor.Handles.Label(
                        cachedBounds.center + Vector3.up * (cachedBounds.size.y * 0.5f + 1.0f),
                        $"Size: {cachedBounds.size.x:F1} x {cachedBounds.size.y:F1} x {cachedBounds.size.z:F1}"
                    );
                }
            }
        }
#endif
        
        #endregion
    }
}

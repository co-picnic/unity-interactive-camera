using UnityEngine;

namespace InteractiveCameraSystem
{
    /// <summary>
    /// Component that defines a camera anchor point in the environment
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class VirtualCameraAnchor : MonoBehaviour, IFrameable, ICameraAnchor
    {
        [Header("Visual Configuration")]
        [Tooltip("The collider that defines the visual bounds of this anchor")]
        public Collider visualCollider;
        
        [Tooltip("Distance radius multiplier based on collider size (2x by default)")]
        [Range(1f, 5f)]
        public float radiusMultiplier = 2f;
        
        [Header("Visual Debug")]
        [Tooltip("Show debug gizmos in scene view")]
        public bool showGizmos = true;
        
        [Tooltip("Color of the debug gizmos")]
        public Color gizmoColor = Color.green;
        
        // Cache the calculated radius
        private float cachedRadius;
        private bool radiusCached = false;
        
        /// <summary>
        /// Get the effective radius of this camera anchor (IFrameable implementation)
        /// </summary>
        public float Radius => EffectiveRadius;
        
        /// <summary>
        /// Get the world position of this anchor (IFrameable implementation)
        /// </summary>
        public Vector3 Position => transform.position;
        
        #region ICameraAnchor Implementation
        
        /// <summary>
        /// Transform representing the anchor position and orientation (ICameraAnchor implementation)
        /// </summary>
        public Transform AnchorTransform => transform;
        
        /// <summary>
        /// Stable identifier for deduplication or debugging (ICameraAnchor implementation)
        /// </summary>
        public string Identifier => gameObject.name;
        
        /// <summary>
        /// Whether this anchor is currently valid for targeting (ICameraAnchor implementation)
        /// </summary>
        public bool IsValid => isActiveAndEnabled && visualCollider != null;
        
        #endregion
        
        /// <summary>
        /// Get the effective radius of this camera anchor
        /// </summary>
        public float EffectiveRadius
        {
            get
            {
                if (!radiusCached)
                {
                    cachedRadius = CalculateRadius();
                    radiusCached = true;
                }
                return cachedRadius;
            }
        }
        
        void Awake()
        {
            // Auto-assign visual collider if not set
            if (visualCollider == null)
            {
                visualCollider = GetComponent<Collider>();
            }
            
            // Validate required components
            if (visualCollider == null)
            {
                Debug.LogError($"[VirtualCameraAnchor] {gameObject.name} requires a Collider component for visual bounds calculation");
            }
        }
        
        void OnValidate()
        {
            // Invalidate cached radius when properties change
            radiusCached = false;
            
            // Auto-assign visual collider if not set
            if (visualCollider == null)
            {
                visualCollider = GetComponent<Collider>();
            }
        }
        
        /// <summary>
        /// Calculate the effective radius based on the collider bounds
        /// </summary>
        private float CalculateRadius()
        {
            if (visualCollider == null)
                return 1f;
            
            // Get the largest dimension of the collider bounds
            Bounds bounds = visualCollider.bounds;
            float maxDimension = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            
            return maxDimension * radiusMultiplier;
        }
        
        /// <summary>
        /// Check if a position is within the effective radius of this anchor
        /// </summary>
        public bool IsPositionInRange(Vector3 position)
        {
            float distance = Vector3.Distance(transform.position, position);
            return distance <= EffectiveRadius;
        }
        
        /// <summary>
        /// Get the closest point on the anchor's radius to a given position
        /// </summary>
        public Vector3 GetClosestPointOnRadius(Vector3 position)
        {
            Vector3 direction = (position - transform.position).normalized;
            return transform.position + direction * EffectiveRadius;
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!showGizmos) return;
            
            // Draw main radius circle (using the calculated effective radius)
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, EffectiveRadius);
            
            // Draw collider bounds if available
            if (visualCollider != null)
            {
                Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
                Gizmos.DrawCube(visualCollider.bounds.center, visualCollider.bounds.size);
            }
            
            // Draw center point
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.8f);
            Gizmos.DrawSphere(transform.position, 0.2f);
            
            // Draw label with anchor info
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * (EffectiveRadius + 1f), 
                $"{gameObject.name} Camera Anchor\nRadius: {EffectiveRadius:F1}m"
            );
        }
        
        void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;
            
            // Draw a more detailed view when selected (using effective radius)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, EffectiveRadius);
            
            // Draw radius rings for better visualization
            Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
            for (int i = 1; i <= 3; i++)
            {
                float ringRadius = EffectiveRadius * (i / 3f);
                Gizmos.DrawWireSphere(transform.position, ringRadius);
            }
            
            // Draw directional indicators
            Gizmos.color = new Color(1f, 1f, 0f, 0.6f);
            Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
            foreach (var direction in directions)
            {
                Vector3 endPoint = transform.position + direction * EffectiveRadius;
                Gizmos.DrawLine(transform.position, endPoint);
                Gizmos.DrawSphere(endPoint, 0.1f);
            }
        }
#endif
    }
}

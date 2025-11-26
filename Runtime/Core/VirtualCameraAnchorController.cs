using UnityEngine;

namespace InteractiveCameraSystem
{
    /// <summary>
    /// Handles anchor management for the virtual camera system
    /// </summary>
    public class VirtualCameraAnchorController
    {
        private VirtualCameraAnchor activeAnchor;
        private bool debugMode;

        public VirtualCameraAnchor ActiveAnchor => activeAnchor;

        public VirtualCameraAnchorController(bool debugMode = false)
        {
            this.debugMode = debugMode;
        }

        /// <summary>
        /// Set the active camera anchor
        /// </summary>
        /// <param name="newAnchor">The new anchor to set as active</param>
        /// <returns>True if anchor was changed</returns>
        public bool SetActiveAnchor(VirtualCameraAnchor newAnchor)
        {
            if (newAnchor == activeAnchor)
            {
                if (debugMode)
                {
                    string anchorName = newAnchor != null ? $"{newAnchor.gameObject.name} Camera Anchor" : "None";
                    Debug.Log($"[VirtualCameraAnchorController] Anchor {anchorName} is already active");
                }
                return false;
            }

            var previousAnchor = activeAnchor;
            activeAnchor = newAnchor;

            if (debugMode)
            {
                string fromName = previousAnchor != null ? $"{previousAnchor.gameObject.name} Camera Anchor" : "None";
                string toName = activeAnchor != null ? $"{activeAnchor.gameObject.name} Camera Anchor" : "None";
                Debug.Log($"[VirtualCameraAnchorController] Changing active anchor from '{fromName}' to '{toName}'");
            }

            return true;
        }

        /// <summary>
        /// Clear the active anchor
        /// </summary>
        public void ClearActiveAnchor()
        {
            SetActiveAnchor(null);
        }

        /// <summary>
        /// Find and set the closest camera anchor to a given position
        /// </summary>
        /// <param name="position">The position to find the closest anchor to</param>
        /// <param name="maxDistance">Maximum distance to search for anchors</param>
        /// <returns>True if an anchor was found and set</returns>
        public bool SetClosestAnchor(Vector3 position, float maxDistance = float.MaxValue)
        {
            var closestAnchor = FindClosestAnchor(position, maxDistance);
            if (closestAnchor != null)
            {
                SetActiveAnchor(closestAnchor);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Find the closest camera anchor to a given position
        /// </summary>
        /// <param name="position">The position to find the closest anchor to</param>
        /// <param name="maxDistance">Maximum distance to search for anchors</param>
        /// <returns>The closest anchor or null if none found</returns>
        public VirtualCameraAnchor FindClosestAnchor(Vector3 position, float maxDistance = float.MaxValue)
        {
            VirtualCameraAnchor[] allAnchors = GetAllAnchors();
            VirtualCameraAnchor closestAnchor = null;
            float closestDistance = maxDistance;

            foreach (var anchor in allAnchors)
            {
                if (anchor == null) continue;

                float distance = Vector3.Distance(position, anchor.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestAnchor = anchor;
                }
            }

            return closestAnchor;
        }

        /// <summary>
        /// Get all camera anchors in the scene
        /// </summary>
        /// <returns>Array of all VirtualCameraAnchor components in the scene</returns>
        public VirtualCameraAnchor[] GetAllAnchors()
        {
            return Object.FindObjectsByType<VirtualCameraAnchor>(FindObjectsSortMode.None);
        }

        /// <summary>
        /// Check if a position is within the active anchor's range
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <returns>True if position is within active anchor's range</returns>
        public bool IsPositionInActiveAnchorRange(Vector3 position)
        {
            if (activeAnchor == null)
                return false;

            return activeAnchor.IsPositionInRange(position);
        }
    }
}

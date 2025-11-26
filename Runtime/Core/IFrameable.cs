using UnityEngine;

namespace InteractiveCameraSystem
{
    /// <summary>
    /// Interface for objects that can be framed by camera strategies.
    /// Unifies VirtualCameraAnchor and ComputedAnchor under a common contract.
    /// </summary>
    public interface IFrameable
    {
        /// <summary>
        /// World space position of the frameable object
        /// </summary>
        Vector3 Position { get; }
        
        /// <summary>
        /// Effective radius of the frameable object for camera calculations
        /// </summary>
        float Radius { get; }
        
        /// <summary>
        /// Check if a position is within the effective radius of this frameable object
        /// </summary>
        /// <param name="testPosition">Position to test</param>
        /// <returns>True if position is within radius</returns>
        bool IsPositionInRange(Vector3 testPosition);
        
        /// <summary>
        /// Get the closest point on the frameable object's radius to a given position
        /// </summary>
        /// <param name="testPosition">Position to find closest point to</param>
        /// <returns>Closest point on the radius</returns>
        Vector3 GetClosestPointOnRadius(Vector3 testPosition);
    }
}

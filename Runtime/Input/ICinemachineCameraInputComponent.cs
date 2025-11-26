using UnityEngine;

namespace InteractiveCameraSystem
{
    /// <summary>
    /// Interface for input components that can control Cinemachine cameras
    /// </summary>
    public interface ICinemachineCameraInputComponent
    {
        /// <summary>
        /// Called when a single finger gesture starts
        /// </summary>
        /// <param name="gestureData">The finger gesture data</param>
        void OnSingleFingerStart(FingerGestureData gestureData);
        
        /// <summary>
        /// Called when a single finger gesture is updating
        /// </summary>
        /// <param name="gestureData">The finger gesture data</param>
        void OnSingleFingerUpdate(FingerGestureData gestureData);
        
        /// <summary>
        /// Called when a single finger gesture ends
        /// </summary>
        /// <param name="gestureData">The finger gesture data</param>
        void OnSingleFingerEnd(FingerGestureData gestureData);
        
        /// <summary>
        /// Called when a pinch gesture starts (optional - not all components need this)
        /// </summary>
        /// <param name="gestureData">The finger gesture data</param>
        void OnPinchStart(FingerGestureData gestureData);
        
        /// <summary>
        /// Called when a pinch gesture is updating (optional - not all components need this)
        /// </summary>
        /// <param name="gestureData">The finger gesture data</param>
        void OnPinchUpdate(FingerGestureData gestureData);
        
        /// <summary>
        /// Called when a pinch gesture ends (optional - not all components need this)
        /// </summary>
        /// <param name="gestureData">The finger gesture data</param>
        void OnPinchEnd(FingerGestureData gestureData);
        
        /// <summary>
        /// Whether this input component is currently active
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// The camera mode this component is associated with
        /// </summary>
        CameraMode AssociatedMode { get; }
    }
}

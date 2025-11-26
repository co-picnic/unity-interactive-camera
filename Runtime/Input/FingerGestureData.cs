using UnityEngine;
using Lean.Touch;
using System.Collections.Generic;


namespace InteractiveCameraSystem
{
    [System.Serializable]
    public class FingerGestureData
    {
        [Header("Basic Info")]
        public GestureType gestureType;
        public int fingerCount;

        [Header("Screen Data")]
        public Vector2 screenPosition;
        public Vector2 screenDelta;

        [Header("Pinch Data")]
        public float pinchRatio;
        public Vector2 pinchCenter;
        public Vector2 pinchCenterDelta;

        [Header("Finger References")]
        public LeanFinger primaryFinger;
        public List<LeanFinger> allFingers;

        public FingerGestureData()
        {
            allFingers = new List<LeanFinger>();
            Clear();
        }

        public void Clear()
        {
            gestureType = GestureType.None;
            fingerCount = 0;
            screenPosition = Vector2.zero;
            screenDelta = Vector2.zero;
            pinchRatio = 1f;
            pinchCenter = Vector2.zero;
            pinchCenterDelta = Vector2.zero;
            primaryFinger = null;
            allFingers.Clear();
        }
    }

    /// <summary>
    /// Types of finger gestures
    /// </summary>
    public enum GestureType
    {
        None,
        SingleFinger,
        Pinch
    }
}



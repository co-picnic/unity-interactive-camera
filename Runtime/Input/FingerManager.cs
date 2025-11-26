using UnityEngine;
using UnityEngine.Events;
using Lean.Touch;
using System.Collections.Generic;


namespace InteractiveCameraSystem
{
    public class FingerManager : MonoBehaviour
    {
        private static FingerManager instance;
        public static FingerManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<FingerManager>();
                    if (instance == null)
                    {
                        Debug.LogError("[CinemachineInputManager] No instance found in scene! Please add CinemachineInputManager to a GameObject.");
                    }
                }
                return instance;
            }
        }

        [System.Serializable]
        public class FingerGestureEvent : UnityEvent<FingerGestureData> { }

        [Header("Events")]
        public FingerGestureEvent OnSingleFingerStart = new FingerGestureEvent();
        public FingerGestureEvent OnSingleFingerUpdate = new FingerGestureEvent();
        public FingerGestureEvent OnSingleFingerEnd = new FingerGestureEvent();

        public FingerGestureEvent OnPinchStart = new FingerGestureEvent();
        public FingerGestureEvent OnPinchUpdate = new FingerGestureEvent();
        public FingerGestureEvent OnPinchEnd = new FingerGestureEvent();

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        [Header("Drag Detection")]
        [SerializeField] private float dragThreshold = 30f; // Minimum pixel movement to consider it a drag

        // Public state
        public bool HasSingleFinger => currentGestureData.gestureType == GestureType.SingleFinger;
        public bool HasPinch => currentGestureData.gestureType == GestureType.Pinch;
        public FingerGestureData CurrentGestureData => currentGestureData;
        public bool IsCurrentFingerDragging => isCurrentFingerDragging;

        // Private state
        private LeanFingerFilter fingerFilter;
        private FingerGestureData currentGestureData;
        private GestureType previousGestureType = GestureType.None;
        private bool isCurrentFingerDragging = false;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Debug.LogWarning("[CinemachineInputManager] Multiple instances detected. Destroying duplicate.");
                Destroy(gameObject);
            }

            fingerFilter = new LeanFingerFilter(true);
            currentGestureData = new FingerGestureData();

            EnsureLeanTouchInScene();
        }

        void Update()
        {
            var fingers = fingerFilter.UpdateAndGetFingers();

            ProcessGestures(fingers);
        }

        private void ProcessGestures(List<LeanFinger> fingers)
        {
            // Update finger data
            currentGestureData.allFingers.Clear();
            currentGestureData.allFingers.AddRange(fingers);
            currentGestureData.fingerCount = fingers.Count;

            // Determine gesture type
            GestureType newGestureType = DetermineGestureType(fingers);

            // Handle gesture transitions
            if (newGestureType != previousGestureType)
            {
                HandleGestureTransition(previousGestureType, newGestureType);
                previousGestureType = newGestureType;
            }

            // Update current gesture
            if (newGestureType != GestureType.None)
            {
                UpdateCurrentGesture(fingers, newGestureType);
            }
        }

        private GestureType DetermineGestureType(List<LeanFinger> fingers)
        {
            switch (fingers.Count)
            {
                case 0:
                    return GestureType.None;
                case 1:
                    return GestureType.SingleFinger;
                case 2:
                default:
                    return GestureType.Pinch;
            }
        }

        private void HandleGestureTransition(GestureType from, GestureType to)
        {
            // End previous gesture
            if (from != GestureType.None)
            {
                switch (from)
                {
                    case GestureType.SingleFinger:
                        OnSingleFingerEnd?.Invoke(currentGestureData);
                        break;
                    case GestureType.Pinch:
                        OnPinchEnd?.Invoke(currentGestureData);
                        break;
                }

                currentGestureData.Clear();
                isCurrentFingerDragging = false; // Reset drag state when gesture ends
            }

            // Start new gesture
            if (to != GestureType.None)
            {
                currentGestureData.gestureType = to;
                isCurrentFingerDragging = false; // Reset drag state for new gesture

                switch (to)
                {
                    case GestureType.SingleFinger:
                        OnSingleFingerStart?.Invoke(currentGestureData);
                        break;
                    case GestureType.Pinch:
                        OnPinchStart?.Invoke(currentGestureData);
                        break;
                }
            }
        }

        private void UpdateCurrentGesture(List<LeanFinger> fingers, GestureType gestureType)
        {
            switch (gestureType)
            {
                case GestureType.SingleFinger:
                    UpdateSingleFingerGesture(fingers[0]);
                    OnSingleFingerUpdate?.Invoke(currentGestureData);
                    break;

                case GestureType.Pinch:
                    UpdatePinchGesture(fingers);
                    OnPinchUpdate?.Invoke(currentGestureData);
                    break;
            }
        }

        private void UpdateSingleFingerGesture(LeanFinger finger)
        {
            currentGestureData.primaryFinger = finger;
            currentGestureData.screenPosition = finger.ScreenPosition;
            currentGestureData.screenDelta = finger.ScreenDelta;

            // Check if finger has moved enough to be considered a drag
            CheckForDragMovement(finger);
        }

        private void UpdatePinchGesture(List<LeanFinger> fingers)
        {
            if (fingers.Count >= 2)
            {
                // Calculate pinch data
                currentGestureData.pinchRatio = LeanGesture.GetPinchRatio(fingers);
                currentGestureData.pinchCenter = LeanGesture.GetScreenCenter(fingers);
                currentGestureData.pinchCenterDelta = LeanGesture.GetScreenCenter(fingers) - LeanGesture.GetLastScreenCenter(fingers);

                // Set primary finger as first one
                currentGestureData.primaryFinger = fingers[0];
                currentGestureData.screenPosition = currentGestureData.pinchCenter;
                currentGestureData.screenDelta = currentGestureData.pinchCenterDelta;
            }
        }

        /// <summary>
        /// Check if the current finger has moved enough to be considered a drag
        /// </summary>
        private void CheckForDragMovement(LeanFinger finger)
        {
            if (finger == null || isCurrentFingerDragging) return;

            // Calculate total movement since finger started
            float totalMovement = Vector2.Distance(finger.ScreenPosition, finger.StartScreenPosition);

            if (totalMovement > dragThreshold)
            {
                isCurrentFingerDragging = true;
            }
        }

        private void EnsureLeanTouchInScene()
        {
            if (FindFirstObjectByType<LeanTouch>() == null)
            {
                gameObject.AddComponent<LeanTouch>();
            }
        }
    }
}



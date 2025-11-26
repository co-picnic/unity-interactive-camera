using UnityEngine;
using System.Collections.Generic;

namespace InteractiveCameraSystem
{
    public class CinemachineInputManager : MonoBehaviour
    {
        private FingerManager fingerManager;
        private List<ICinemachineCameraInputComponent> inputComponents = new List<ICinemachineCameraInputComponent>();
        
        // Singleton pattern
        private static CinemachineInputManager instance;
        public static CinemachineInputManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<CinemachineInputManager>();
                    if (instance == null)
                    {
                        Debug.LogError("[CinemachineInputManager] No instance found in scene! Please add CinemachineInputManager to a GameObject.");
                    }
                }
                return instance;
            }
        }

        void Awake()
        {
            // Ensure singleton pattern
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Debug.LogWarning("[CinemachineInputManager] Multiple instances detected. Destroying duplicate.");
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            fingerManager = FingerManager.Instance;
            
            // Auto-register input components on this GameObject
            AutoRegisterInputComponents();
            
            // Setup Unity Events for gesture handling
            SetupFingerListeners();
        }

        /// <summary>
        /// Setup Unity Events for gesture handling
        /// </summary>
        public void SetupFingerListeners()
        {
            if (fingerManager == null)
            {
                fingerManager = FingerManager.Instance;
            }

            // Clear existing listeners first
            ClearFingerListeners();

            // Add new listeners
            fingerManager.OnSingleFingerStart.AddListener(OnSingleFingerStart);
            fingerManager.OnSingleFingerUpdate.AddListener(OnSingleFingerUpdate);
            fingerManager.OnSingleFingerEnd.AddListener(OnSingleFingerEnd);
            
            fingerManager.OnPinchStart.AddListener(OnPinchStart);
            fingerManager.OnPinchUpdate.AddListener(OnPinchUpdate);
            fingerManager.OnPinchEnd.AddListener(OnPinchEnd);
        }

        /// <summary>
        /// Clear all gesture event listeners
        /// </summary>
        private void ClearFingerListeners()
        {
            if (fingerManager == null) return;

            fingerManager.OnSingleFingerStart.RemoveAllListeners();
            fingerManager.OnSingleFingerUpdate.RemoveAllListeners();
            fingerManager.OnSingleFingerEnd.RemoveAllListeners();
            
            fingerManager.OnPinchStart.RemoveAllListeners();
            fingerManager.OnPinchUpdate.RemoveAllListeners();
            fingerManager.OnPinchEnd.RemoveAllListeners();
        }


        /// <summary>
        /// Auto-register input components on this GameObject
        /// </summary>
        private void AutoRegisterInputComponents()
        {
            var components = GetComponents<ICinemachineCameraInputComponent>();
            foreach (var component in components)
            {
                RegisterInputComponent(component);
            }
        }

        public void RegisterInputComponent(ICinemachineCameraInputComponent component)
        {
            if (component == null) return;

            if (!inputComponents.Contains(component))
            {
                inputComponents.Add(component);
            }
        }

        public void UnregisterInputComponent(ICinemachineCameraInputComponent component)
        {
            if (component == null) return;

            inputComponents.Remove(component);
        }

        public void ClearInputComponents()
        {
            inputComponents.Clear();
        }

        private void OnSingleFingerStart(FingerGestureData gestureData)
        {
            foreach (var inputComponent in inputComponents)
            {
                if (inputComponent.IsActive)
                {
                    inputComponent.OnSingleFingerStart(gestureData);
                }
            }
        }

        private void OnSingleFingerUpdate(FingerGestureData gestureData)
        {
            foreach (var inputComponent in inputComponents)
            {
                if (inputComponent.IsActive)
                {
                    inputComponent.OnSingleFingerUpdate(gestureData);
                }
            }
        }

        private void OnSingleFingerEnd(FingerGestureData gestureData)
        {
            foreach (var inputComponent in inputComponents)
            {
                if (inputComponent.IsActive)
                {
                    inputComponent.OnSingleFingerEnd(gestureData);
                }
            }
        }

        private void OnPinchStart(FingerGestureData gestureData)
        {
            foreach (var inputComponent in inputComponents)
            {
                if (inputComponent.IsActive)
                {
                    inputComponent.OnPinchStart(gestureData);
                }
            }
        }

        private void OnPinchUpdate(FingerGestureData gestureData)
        {
            foreach (var inputComponent in inputComponents)
            {
                if (inputComponent.IsActive)
                {
                    inputComponent.OnPinchUpdate(gestureData);
                }
            }
        }

        private void OnPinchEnd(FingerGestureData gestureData)
        {
            foreach (var inputComponent in inputComponents)
            {
                if (inputComponent.IsActive)
                {
                    inputComponent.OnPinchEnd(gestureData);
                }
            }
        }

    }
}

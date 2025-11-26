using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InteractiveCameraSystem
{
    /// <summary>
    /// Simplified Cinemachine-based camera management system.
    /// Replaces the complex behavior-based system with direct Cinemachine integration.
    /// </summary>
    public class CinemachineCameraManager : MonoBehaviour
    {
        private static CinemachineCameraManager _instance;
        public static CinemachineCameraManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<CinemachineCameraManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("CinemachineCameraManager");
                        _instance = go.AddComponent<CinemachineCameraManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        
        [Header("Camera Modes")]
        [Tooltip("List of all available camera modes")]
        [Header("Camera Mode Discovery")]
        [SerializeField] private bool autoDiscoverModes = true;
        [SerializeField] private bool allowManualOverride = true;
        [SerializeField] private List<CameraMode> manualCameraModes = new List<CameraMode>();
        
        [SerializeField] private List<CameraMode> cameraModes = new List<CameraMode>();
        
        [Header("Camera Auto-Management")]
        [Tooltip("Automatically scan the scene for existing cameras on play")]
        [SerializeField] private bool autoDiscoverCameras = false;
        
        [Tooltip("Automatically instantiate missing cameras from registered modes when entering play mode")]
        [SerializeField] private bool autoInstantiateMissingCamerasOnPlay = false;
        
        [Header("Current State")]
        [Tooltip("Currently active camera mode")]
        [SerializeField] private CameraMode currentMode;
        
        [Tooltip("Default camera mode to activate on startup")]
        [SerializeField] private CameraMode defaultMode;
        
        [Tooltip("Currently active targets for following")]
        [SerializeField] private List<Transform> activeTargets = new List<Transform>();
        
        [Header("Cinemachine Components")]
        [Tooltip("Cinemachine Brain component")]
        [SerializeField] private CinemachineBrain brain;
        
        [Tooltip("Cinemachine Target Group for multi-target following")]
        [SerializeField] private CinemachineTargetGroup targetGroup;
        
        [Header("Debug")]
        [Tooltip("Enable debug logging")]
        [SerializeField] private bool debugMode = false;
        
        [Header("Settings")]
        [Tooltip("Default transition duration between modes")]
        [SerializeField] private float defaultTransitionDuration = 1f;
        
        [Header("Camera Management")]
        [Tooltip("Parent transform for instantiated cameras (optional)")]
        [SerializeField] private Transform cameraParent;
        
        [Header("Input Handling")]
        [Tooltip("Enable input handling for drag functionality")]
        [SerializeField] private bool enableInputHandling = true;
        
        // Runtime state
        private Dictionary<string, CameraMode> modeLookup;
        private CinemachineCamera activeCamera;
        private bool isTransitioning = false;
        private CinemachineInputManager inputManager;
        private Dictionary<Transform, float> targetRadii = new Dictionary<Transform, float>();
        
        // Events
        public System.Action<CameraMode> OnModeChanged;
        public System.Action<List<Transform>> OnTargetsChanged;
        public System.Action<bool> OnTransitionStateChanged;
        
        #region Properties
        
        public CameraMode CurrentMode => currentMode;
        
        public CameraMode DefaultMode => defaultMode;
        
        public IReadOnlyList<Transform> ActiveTargets => activeTargets.AsReadOnly();
        
        public bool IsTransitioning => isTransitioning;
        
        public CinemachineBrain Brain => brain;
        
        public CinemachineTargetGroup TargetGroup => targetGroup;
        
        public CinemachineCamera ActiveCamera => activeCamera;
        
        #endregion
        
        #region Unity Lifecycle
        
        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                if (transform.parent != null)
                {
                    transform.SetParent(null);
                }
                DontDestroyOnLoad(gameObject);
                InitializeCameraSystem();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeCameraSystem()
        {
            // Auto-discover camera modes if enabled
            if (autoDiscoverModes)
            {
                AutoDiscoverCameraModes();
            }
            
            // Add manual overrides if allowed
            if (allowManualOverride && manualCameraModes.Count > 0)
            {
                foreach (var manualMode in manualCameraModes)
                {
                    if (manualMode != null && !cameraModes.Contains(manualMode))
                    {
                        cameraModes.Add(manualMode);
                    }
                }
            }
            
            // Initialize the rest of the system
            BuildModeLookup();
            
            // Auto-discover cameras in scene if enabled
            if (autoDiscoverCameras)
            {
                AutoDiscoverCameras();
            }
            
            // Auto-instantiate missing cameras if enabled
            if (autoInstantiateMissingCamerasOnPlay)
            {
                AutoInstantiateMissingCameras();
            }
        }
        
        /// <summary>
        /// Auto-discovers camera modes from the plugin folder
        /// </summary>
        public void AutoDiscoverCameraModes()
        {
            cameraModes.Clear();
            
#if UNITY_EDITOR
            // Editor-only: Use AssetDatabase for discovery
            AutoDiscoverFromAssetDatabase();
#else
            // Runtime: Use Resources folder
            AutoDiscoverFromResources();
#endif
            
        }
        
#if UNITY_EDITOR
        private void AutoDiscoverFromAssetDatabase()
        {
            // Only search in Assets folder to avoid exposing package files
            // Users should create their own CameraMode assets in their project
            string[] searchPaths = {
                "Assets",
            };
            
            foreach (string searchPath in searchPaths)
            {
                if (UnityEditor.AssetDatabase.IsValidFolder(searchPath))
                {
                    string[] guids = UnityEditor.AssetDatabase.FindAssets("t:CameraMode", new[] { searchPath });
                    foreach (string guid in guids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                        CameraMode mode = UnityEditor.AssetDatabase.LoadAssetAtPath<CameraMode>(path);
                        if (mode != null && mode.isEnabled && !cameraModes.Contains(mode))
                        {
                            cameraModes.Add(mode);
                        }
                    }
                }
            }
        }
#endif
        
        private void AutoDiscoverFromResources()
        {
            var discoveredModes = Resources.LoadAll<CameraMode>("CameraModes");
            foreach (var mode in discoveredModes)
            {
                if (mode != null && mode.isEnabled && !cameraModes.Contains(mode))
                {
                    cameraModes.Add(mode);
                }
            }
        }
        
        /// <summary>
        /// Auto-discovers camera modes from cameras in the scene
        /// </summary>
        public void AutoDiscoverFromSceneCameras()
        {
            var camerasInScene = FindObjectsOfType<CinemachineCamera>();
            foreach (var camera in camerasInScene)
            {
                string modeName = camera.name.Replace("CM vcam", "").Trim();
                if (string.IsNullOrEmpty(modeName))
                {
                    modeName = camera.name;
                }
                
                // Check if we already have a mode for this camera
                var existingMode = cameraModes.FirstOrDefault(m => m.virtualCamera == camera);
                if (existingMode == null)
                {
                    // Create a basic mode for this camera
                    var newMode = ScriptableObject.CreateInstance<CameraMode>();
                    newMode.modeName = modeName;
                    newMode.description = $"Auto-generated mode for {camera.name}";
                    newMode.virtualCamera = camera;
                    newMode.priority = 50;
                    newMode.isEnabled = true;
                    cameraModes.Add(newMode);
                }
            }
            
        }
        
        /// <summary>
        /// Auto-discovers cameras in the scene and logs information about them
        /// </summary>
        public void AutoDiscoverCameras()
        {
            var camerasInScene = FindObjectsOfType<CinemachineCamera>(true);
            
            if (debugMode || camerasInScene.Length > 0)
            {
                Debug.Log($"[CinemachineCameraManager] Auto-Discover Cameras: Found {camerasInScene.Length} camera(s) in scene:");
                foreach (var camera in camerasInScene)
                {
                    var identifier = camera.GetComponent<CameraModeIdentifier>();
                    string idInfo = identifier != null ? $" [Mode ID: {identifier.ModeId}]" : "";
                    string modeInfo = " (No matching mode)";
                    
                    // Check if this camera matches any registered mode
                    foreach (var mode in cameraModes)
                    {
                        if (mode.virtualCamera == camera || 
                            (identifier != null && identifier.Matches(mode.GetEffectiveModeId())) ||
                            camera.name == mode.GetEffectiveModeId())
                        {
                            modeInfo = $" (Matches mode: {mode.modeName})";
                            break;
                        }
                    }
                    
                    Debug.Log($"  - {camera.name} on GameObject '{camera.gameObject.name}'{idInfo}{modeInfo}");
                }
            }
        }
        
        /// <summary>
        /// Auto-instantiates missing cameras from registered modes that don't exist in the scene
        /// Skips modes with isEnabled = false
        /// </summary>
        public void AutoInstantiateMissingCameras()
        {
            if (cameraModes.Count == 0)
            {
                if (debugMode)
                {
                    Debug.Log("[CinemachineCameraManager] Auto-Instantiate: No camera modes registered, skipping instantiation");
                }
                return;
            }
            
            int instantiatedCount = 0;
            int skippedCount = 0;
            int excludedCount = 0;
            int errorCount = 0;
            
            if (debugMode)
            {
                Debug.Log("[CinemachineCameraManager] Auto-Instantiate: Checking for missing cameras...");
            }
            
            // Ensure we have a "Cameras" parent GameObject in the scene (same as Instantiate All Cameras button)
            GameObject camerasParent = GameObject.Find("Cameras");
            if (camerasParent == null)
            {
                camerasParent = new GameObject("Cameras");
                if (debugMode)
                {
                    Debug.Log("[CinemachineCameraManager] Auto-Instantiate: Created 'Cameras' parent GameObject");
                }
            }
            else if (debugMode)
            {
                Debug.Log("[CinemachineCameraManager] Auto-Instantiate: Found existing 'Cameras' parent GameObject");
            }
            
            // Temporarily set the camera parent to our "Cameras" GameObject (same as button behavior)
            var originalCameraParent = cameraParent;
            cameraParent = camerasParent.transform;
            
            // Iterate through all registered modes
            foreach (var mode in cameraModes)
            {
                // Skip disabled modes
                if (!mode.isEnabled)
                {
                    excludedCount++;
                    if (debugMode)
                    {
                        Debug.Log($"[CinemachineCameraManager] Auto-Instantiate: Skipping mode '{mode.modeName}' (disabled)");
                    }
                    continue;
                }
                
                if (mode == null)
                {
                    errorCount++;
                    Debug.LogWarning("[CinemachineCameraManager] Auto-Instantiate: Found null mode, skipping.");
                    continue;
                }
                
                if (mode.virtualCamera == null)
                {
                    errorCount++;
                    Debug.LogWarning($"[CinemachineCameraManager] Auto-Instantiate: Mode '{mode.modeName}' has no virtual camera reference, skipping.");
                    continue;
                }
                
                // Check if camera already exists in scene
                var existingCamera = FindCameraByName(mode.GetEffectiveModeId());
                if (existingCamera != null)
                {
                    skippedCount++;
                    if (debugMode)
                    {
                        Debug.Log($"[CinemachineCameraManager] Auto-Instantiate: Camera for mode '{mode.modeName}' already exists in scene, skipping");
                    }
                    continue;
                }
                
                // Instantiate the camera
                try
                {
                    var result = InstantiateCameraInScene(mode.virtualCamera);
                    if (result != null)
                    {
                        instantiatedCount++;
                        Debug.Log($"[CinemachineCameraManager] Auto-Instantiate: Successfully instantiated camera '{result.name}' for mode '{mode.modeName}'");
                    }
                    else
                    {
                        errorCount++;
                        Debug.LogWarning($"[CinemachineCameraManager] Auto-Instantiate: Failed to instantiate camera for mode '{mode.modeName}'");
                    }
                }
                catch (System.Exception e)
                {
                    errorCount++;
                    Debug.LogError($"[CinemachineCameraManager] Auto-Instantiate: Exception while instantiating camera for mode '{mode.modeName}': {e.Message}");
                }
            }
            
            // Restore the original camera parent (optional - we could leave it set to "Cameras")
            // This matches the behavior of the "Instantiate All Cameras" button
            cameraParent = originalCameraParent;
            
            if (debugMode || instantiatedCount > 0)
            {
                Debug.Log($"[CinemachineCameraManager] Auto-Instantiate Complete: Instantiated={instantiatedCount}, Skipped={skippedCount}, Excluded={excludedCount}, Errors={errorCount}");
            }
        }
        
        void Start()
        {
            InitializeInputHandling();
            
            if (debugMode)
            {
                DebugStartupState();
            }
            
            EnsureDefaultCameraActive();
            
            CameraMode initialMode = currentMode ?? defaultMode;
            if (initialMode != null)
            {
                SwitchToMode(initialMode, false);
            }
        }
        
        #endregion
        
        #region Public Debug Methods
        
        [ContextMenu("Debug Current State")]
        public void DebugCurrentState()
        {
            DebugStartupState();
            
            if (activeCamera != null)
            {
                var camera = activeCamera.GetComponent<Camera>();
                if (camera != null)
                {
                    Debug.Log($"Active Camera '{activeCamera.name}' FOV: {camera.fieldOfView}, Priority: {activeCamera.Priority}");
                }
            }
        }
        
        public bool SwitchToDefaultMode(bool smoothTransition = true)
        {
            if (defaultMode != null)
            {
                return SwitchToMode(defaultMode, smoothTransition);
            }
            else
            {
                Debug.LogWarning("[CinemachineCameraManager] No default mode set!");
                return false;
            }
        }

        [ContextMenu("Switch to Default Mode")]
        public void ForceSwitchToDefaultMode()
        {
            SwitchToDefaultMode(false);
        }
        
        [ContextMenu("Refresh Current Mode Settings")]
        public void RefreshCurrentModeSettings()
        {
            if (currentMode != null)
            {
                ApplyModeSettings(currentMode);
                UpdateCameraComponents(currentMode);
                UpdateTargetsForMode(currentMode);
            }
            else
            {
                Debug.LogWarning("No current mode set!");
            }
        }
        
        [ContextMenu("Check All Camera Instances")]
        public void CheckAllCameraInstances()
        {
            int foundCount = 0;
            int missingCount = 0;
            
            foreach (var mode in cameraModes)
            {
                if (mode != null && mode.virtualCamera != null)
                {
                    var sceneCamera = FindCameraByName(mode.GetEffectiveModeId());
                    if (sceneCamera != null)
                    {
                        foundCount++;
                    }
                    else
                    {
                        missingCount++;
                        Debug.LogWarning($"Missing scene camera for mode '{mode.modeName}'. Expected camera name/ID: '{mode.GetEffectiveModeId()}'");
                    }
                }
            }
            
            Debug.Log($"Camera Status: {foundCount} found, {missingCount} missing");
        }
        
        #endregion
        
        #region Debug Methods
        
        private void DebugStartupState()
        {
            Debug.Log($"[CinemachineCameraManager] Brain: {(brain != null ? "Found" : "Missing")}, TargetGroup: {(targetGroup != null ? "Found" : "Missing")}");
            Debug.Log($"[CinemachineCameraManager] Built mode lookup with {cameraModes.Count} modes");
        }
        
        #endregion
        
        #region Initialization
        
        
        private void BuildModeLookup()
        {
            modeLookup = new Dictionary<string, CameraMode>();
            
            foreach (var mode in cameraModes)
            {
                if (mode != null && !string.IsNullOrEmpty(mode.modeName))
                {
                    if (modeLookup.ContainsKey(mode.modeName))
                    {
                        Debug.LogWarning($"[CinemachineCameraManager] Duplicate mode name: {mode.modeName}");
                    }
                    else
                    {
                        modeLookup[mode.modeName] = mode;
                    }
                }
            }
        }
        
        #endregion
        
        #region Input Handling
        
        private void InitializeInputHandling()
        {
            if (!enableInputHandling) return;
            
            inputManager = CinemachineInputManager.Instance;
            inputManager.SetupFingerListeners();
        }
        
        public void RegisterInputComponent(ICinemachineCameraInputComponent component)
        {
            if (inputManager == null) return;
            
            inputManager.RegisterInputComponent(component);
        }
        
        public void UnregisterInputComponent(ICinemachineCameraInputComponent component)
        {
            if (inputManager == null) return;
            
            inputManager.UnregisterInputComponent(component);
        }
        
        #endregion
        
        #region Mode Management
        
        public bool SwitchToMode(string modeName, bool smoothTransition = true)
        {
            if (string.IsNullOrEmpty(modeName))
            {
                return false;
            }
            
            if (modeLookup == null)
            {
                BuildModeLookup();
            }
            
            if (!modeLookup.TryGetValue(modeName, out CameraMode targetMode))
            {
                return false;
            }
            
            return SwitchToMode(targetMode, smoothTransition);
        }
        
        public bool SwitchToMode(CameraMode mode, bool smoothTransition = true)
        {
            if (mode == null)
            {
                return false;
            }
            
            if (mode.virtualCamera == null)
            {
                return false;
            }
            
            if (currentMode == mode)
            {
                if (debugMode)
                {
                    Debug.Log($"[CinemachineCameraManager] Already in mode '{mode.modeName}', applying settings without switching");
                }
                
                ApplyModeSettings(mode);
                UpdateCameraComponents(mode);
                UpdateTargetsForMode(mode);
                
                return true;
            }
            
            var previousMode = currentMode;
            currentMode = mode;
            
            SwitchCinemachineCamera(mode, smoothTransition);
            ApplyModeSettings(mode);
            UpdateCameraComponents(mode);
            UpdateTargetsForMode(mode);
            
            if (debugMode)
            {
                string transitionText = smoothTransition ? " (smooth)" : " (instant)";
            }
            
            OnModeChanged?.Invoke(mode);
            
            return true;
        }
        
        public IReadOnlyList<CameraMode> GetAllModes()
        {
            return cameraModes.AsReadOnly();
        }
        
        public CameraMode GetMode(string modeName)
        {
            modeLookup.TryGetValue(modeName, out CameraMode mode);
            return mode;
        }
        
        public void SetDefaultMode(CameraMode mode)
        {
            if (mode != null && !cameraModes.Contains(mode))
            {
                return;
            }
            
            defaultMode = mode;
            
            if (debugMode)
            {
                string modeName = mode?.modeName ?? "null";
            }
        }
        
        public void AddMode(CameraMode mode)
        {
            if (mode == null || string.IsNullOrEmpty(mode.modeName))
            {
                return;
            }
            
            if (modeLookup == null)
            {
                BuildModeLookup();
            }
            
            if (modeLookup.ContainsKey(mode.modeName))
            {
                return;
            }
            
            cameraModes.Add(mode);
            modeLookup[mode.modeName] = mode;
            
            if (debugMode)
            {
                Debug.Log($"[CinemachineCameraManager] Added mode '{mode.modeName}'");
            }
        }
        
        public bool RemoveMode(string modeName)
        {
            if (modeLookup == null)
            {
                BuildModeLookup();
            }
            
            if (!modeLookup.TryGetValue(modeName, out CameraMode mode))
            {
                return false;
            }
            
            cameraModes.Remove(mode);
            modeLookup.Remove(modeName);
            
            if (currentMode == mode && cameraModes.Count > 0)
            {
                SwitchToMode(cameraModes[0], false);
            }
            
            if (debugMode)
            {
                Debug.Log($"[CinemachineCameraManager] Removed mode '{modeName}'");
            }
            
            return true;
        }
        
        #endregion
        
        #region Target Management
        
        public void SetTargets(List<Transform> targets)
        {
            SetTargets(targets, null);
        }
        
        public void SetTargets(List<Transform> targets, float? radius)
        {
            if (targets == null)
            {
                targets = new List<Transform>();
            }
            
            var validTargets = targets.Where(t => t != null).ToList();
            
            activeTargets.Clear();
            activeTargets.AddRange(validTargets);
            
            // Update stored radii
            // Remove old targets that are no longer in the list
            var targetsToRemove = targetRadii.Keys.Where(t => !activeTargets.Contains(t)).ToList();
            foreach (var oldTarget in targetsToRemove)
            {
                targetRadii.Remove(oldTarget);
            }
            
            // Set radius for all new targets
            if (radius.HasValue)
            {
                // Override radius for all targets
                foreach (var target in validTargets)
                {
                    targetRadii[target] = radius.Value;
                }
            }
            else
            {
                // Set default radius for targets that don't have one
                float defaultRadius = currentMode?.followSettings?.targetRadius ?? 1f;
                foreach (var target in validTargets)
                {
                    if (!targetRadii.ContainsKey(target))
                    {
                        targetRadii[target] = defaultRadius;
                    }
                }
            }
            
            UpdateCinemachineTargets();
            
            if (debugMode)
            {
                string targetNames = string.Join(", ", validTargets.Select(t => t.name));
                string radiusInfo = radius.HasValue ? $" with radius {radius.Value}" : "";
                Debug.Log($"[CinemachineCameraManager] Set targets: [{targetNames}]{radiusInfo}");
            }
            
            OnTargetsChanged?.Invoke(activeTargets);
        }
        
        public void AddTarget(Transform target)
        {
            AddTarget(target, null);
        }
        
        public void AddTarget(Transform target, float? radius)
        {
            if (target == null) return;
            
            if (!activeTargets.Contains(target))
            {
                activeTargets.Add(target);
                
                // Store the radius for this target if provided
                if (radius.HasValue)
                {
                    targetRadii[target] = radius.Value;
                }
                else if (!targetRadii.ContainsKey(target))
                {
                    // Store default radius from mode or fallback
                    targetRadii[target] = currentMode?.followSettings?.targetRadius ?? 1f;
                }
                
                UpdateCinemachineTargets();
                
                if (debugMode)
                {
                    float actualRadius = targetRadii.ContainsKey(target) ? targetRadii[target] : 1f;
                    Debug.Log($"[CinemachineCameraManager] Added target: {target.name} with radius {actualRadius:F2}");
                }
                
                OnTargetsChanged?.Invoke(activeTargets);
            }
        }
        
        public void RemoveTarget(Transform target)
        {
            if (target == null) return;
            
            if (activeTargets.Remove(target))
            {
                // Clean up stored radius
                targetRadii.Remove(target);
                
                UpdateCinemachineTargets();
                
                if (debugMode)
                {
                    Debug.Log($"[CinemachineCameraManager] Removed target: {target.name}");
                }
                
                OnTargetsChanged?.Invoke(activeTargets);
            }
        }
        
        public void ClearTargets()
        {
            activeTargets.Clear();
            targetRadii.Clear();
            UpdateCinemachineTargets();
            
            if (debugMode)
            {
                Debug.Log("[CinemachineCameraManager] Cleared all targets");
            }
            
            OnTargetsChanged?.Invoke(activeTargets);
        }
        
        public void SetMultipleActiveAnchors(VirtualCameraAnchor[] anchors, bool smoothTransition = true)
        {
            if (anchors == null)
            {
                anchors = new VirtualCameraAnchor[0];
            }
            
            var validAnchors = anchors.Where(a => a != null).ToArray();
            var targetTransforms = validAnchors.Select(a => a.transform).ToList();
            
            if (debugMode)
            {
                string anchorNames = string.Join(", ", validAnchors.Select(a => a.name));
                Debug.Log($"[CinemachineCameraManager] Set multiple active anchors: [{anchorNames}]");
            }
            
            SetTargets(targetTransforms);
        }

        /// <summary>
        /// Overload to accept plugin-agnostic anchors via ICameraAnchor.
        /// Converts anchors to their underlying transforms and delegates to SetTargets.
        /// </summary>
        public void SetMultipleActiveAnchors(ICameraAnchor[] anchors, bool smoothTransition = true)
        {
            if (anchors == null)
            {
                anchors = new ICameraAnchor[0];
            }

            var validAnchors = anchors.Where(a => a != null && a.IsValid).ToArray();
            var targetTransforms = validAnchors
                .Select(a => a.AnchorTransform)
                .Where(t => t != null)
                .ToList();

            if (debugMode)
            {
                string anchorNames = string.Join(", ", validAnchors.Select(a => a.Identifier));
                Debug.Log($"[CinemachineCameraManager] Set multiple active anchors (ICameraAnchor): [{anchorNames}]");
            }

            SetTargets(targetTransforms);
        }
        
        
        #endregion
        
        #region Dynamic Component Management
        
        public void UpdateCameraComponents(CameraMode mode)
        {
            if (mode == null || mode.virtualCamera == null) return;
            
            var cameraInstance = FindCameraByName(mode.GetEffectiveModeId());
            if (cameraInstance == null) return;
            
            UpdateFollowComponent(cameraInstance, mode);
            UpdateGroupFramingComponent(cameraInstance, mode);
            UpdateRotationComposerComponent(cameraInstance, mode);
            UpdateConfinerComponent(cameraInstance, mode);
            UpdateInputComponents(cameraInstance, mode);
            
            if (debugMode)
            {
                Debug.Log($"[CinemachineCameraManager] Updated components for camera '{cameraInstance.name}' based on behavior flags");
            }
        }
        
        private void UpdateFollowComponent(CinemachineCamera camera, CameraMode mode)
        {
            var followComponent = camera.GetComponent<CinemachineFollow>();
            
            if (mode.enableFollow)
            {
                if (followComponent == null)
                {
                    followComponent = camera.gameObject.AddComponent<CinemachineFollow>();
                    if (debugMode)
                    {
                        Debug.Log($"[CinemachineCameraManager] Added CinemachineFollow to '{camera.name}'");
                    }
                }
                followComponent.FollowOffset = mode.followSettings.followOffset;
            }
            else
            {
                if (followComponent != null)
                {
                    DestroyImmediate(followComponent);
                    if (debugMode)
                    {
                        Debug.Log($"[CinemachineCameraManager] Removed CinemachineFollow from '{camera.name}'");
                    }
                }
            }
        }
        
        private void UpdateGroupFramingComponent(CinemachineCamera camera, CameraMode mode)
        {
            var groupFraming = camera.GetComponent<CinemachineGroupFraming>();
            
            if (mode.followSettings.useGroupFraming)
            {
                if (groupFraming == null)
                {
                    groupFraming = camera.gameObject.AddComponent<CinemachineGroupFraming>();
                    if (debugMode)
                    {
                        Debug.Log($"[CinemachineCameraManager] Added CinemachineGroupFraming to '{camera.name}'");
                    }
                }
                groupFraming.DollyRange = new Vector2(mode.followSettings.dollyDistanceMin, mode.followSettings.dollyDistanceMax);
                groupFraming.FramingSize = mode.followSettings.framingSize;
            }
            else
            {
                if (groupFraming != null)
                {
                    DestroyImmediate(groupFraming);
                    if (debugMode)
                    {
                        Debug.Log($"[CinemachineCameraManager] Removed CinemachineGroupFraming from '{camera.name}'");
                    }
                }
            }
        }
        
        private void UpdateRotationComposerComponent(CinemachineCamera camera, CameraMode mode)
        {
            var rotationComposer = camera.GetComponent<CinemachineRotationComposer>();
            
            bool needsRotationComposer = mode.enableFollow || mode.HasTag("cutscene") || mode.HasTag("conversation");
            
            if (needsRotationComposer)
            {
                if (rotationComposer == null)
                {
                    rotationComposer = camera.gameObject.AddComponent<CinemachineRotationComposer>();
                    if (debugMode)
                    {
                        Debug.Log($"[CinemachineCameraManager] Added CinemachineRotationComposer to '{camera.name}'");
                    }
                }
            }
            else
            {
                if (rotationComposer != null)
                {
                    DestroyImmediate(rotationComposer);
                    if (debugMode)
                    {
                        Debug.Log($"[CinemachineCameraManager] Removed CinemachineRotationComposer from '{camera.name}'");
                    }
                }
            }
        }
        
        private void UpdateConfinerComponent(CinemachineCamera camera, CameraMode mode)
        {
            var confiner = camera.GetComponent<CinemachineConfiner3D>();
            
            bool needsConfiner = mode.HasTag("exploration") || mode.HasTag("explore");
            
            if (needsConfiner)
            {
                if (confiner == null)
                {
                    confiner = camera.gameObject.AddComponent<CinemachineConfiner3D>();
                    if (debugMode)
                    {
                        Debug.Log($"[CinemachineCameraManager] Added CinemachineConfiner3D to '{camera.name}'");
                    }
                }
            }
            else
            {
                if (confiner != null)
                {
                    DestroyImmediate(confiner);
                    if (debugMode)
                    {
                        Debug.Log($"[CinemachineCameraManager] Removed CinemachineConfiner3D from '{camera.name}'");
                    }
                }
            }
        }
        
        private void UpdateInputComponents(CinemachineCamera camera, CameraMode mode)
        {
            var dragInputComponent = camera.GetComponent<CinemachineDragInputComponent>();
            
            if (mode.enableDrag)
            {
                if (dragInputComponent == null)
                {
                    dragInputComponent = camera.gameObject.AddComponent<CinemachineDragInputComponent>();
                    if (debugMode)
                    {
                        Debug.Log($"[CinemachineCameraManager] Added CinemachineDragInputComponent to '{camera.name}'");
                    }
                }
                
                dragInputComponent.Initialize(camera, mode);
                dragInputComponent.SetActive(true);
                
                RegisterInputComponent(dragInputComponent);
            }
            else
            {
                if (dragInputComponent != null)
                {
                    UnregisterInputComponent(dragInputComponent);
                    
                    DestroyImmediate(dragInputComponent);
                    
                    if (debugMode)
                    {
                        Debug.Log($"[CinemachineCameraManager] Removed CinemachineDragInputComponent from '{camera.name}'");
                    }
                }
            }
            
            var zoomInputComponent = camera.GetComponent<CinemachineZoomInputComponent>();
            
            if (mode.enableZoom)
            {
                if (zoomInputComponent == null)
                {
                    zoomInputComponent = camera.gameObject.AddComponent<CinemachineZoomInputComponent>();
                    if (debugMode)
                    {
                        Debug.Log($"[CinemachineCameraManager] Added CinemachineZoomInputComponent to '{camera.name}'");
                    }
                }
                
                zoomInputComponent.Initialize(camera, mode);
                zoomInputComponent.SetActive(true);
                
                RegisterInputComponent(zoomInputComponent);
            }
            else
            {
                if (zoomInputComponent != null)
                {
                    UnregisterInputComponent(zoomInputComponent);
                    DestroyImmediate(zoomInputComponent);
                    
                    if (debugMode)
                    {
                        Debug.Log($"[CinemachineCameraManager] Removed CinemachineZoomInputComponent from '{camera.name}'");
                    }
                }
            }
        }
        
        private void EnsureDefaultCameraActive()
        {
            if (defaultMode == null || defaultMode.virtualCamera == null) return;
            
            var defaultCamera = FindCameraByName(defaultMode.GetEffectiveModeId());
            if (defaultCamera == null)
            {
                if (debugMode)
                {
                    Debug.LogWarning($"[CinemachineCameraManager] Default camera '{defaultMode.GetEffectiveModeId()}' not found in scene");
                }
                return;
            }
            
            if (!defaultCamera.gameObject.activeInHierarchy)
            {
                defaultCamera.gameObject.SetActive(true);
                if (debugMode)
                {
                    Debug.Log($"[CinemachineCameraManager] Activated default camera GameObject: '{defaultCamera.gameObject.name}'");
                }
            }
            
            if (debugMode)
            {
                Debug.Log($"[CinemachineCameraManager] Ensured default camera '{defaultCamera.name}' is active");
            }
        }
        
        private void ManageCameraStates(CinemachineCamera activeCamera)
        {
            if (activeCamera == null) return;
            
            var allCameras = FindObjectsOfType<CinemachineCamera>(true);
            
            foreach (var camera in allCameras)
            {
                if (camera == null) continue;
                
                if (camera == activeCamera)
                {
                    if (!camera.gameObject.activeInHierarchy)
                    {
                        camera.gameObject.SetActive(true);
                    }
                    
                    if (debugMode)
                    {
                        Debug.Log($"[CinemachineCameraManager] Enabled active camera '{camera.name}'");
                    }
                }
                else
                {
                    // Disable all other cameras when switching modes
                    if (camera.gameObject.activeInHierarchy)
                    {
                        camera.gameObject.SetActive(false);
                        
                        if (debugMode)
                        {
                            Debug.Log($"[CinemachineCameraManager] Disabled camera '{camera.name}'");
                        }
                    }
                }
            }
        }
        
        #endregion
        
        #region Cinemachine Integration
        
        private void SwitchCinemachineCamera(CameraMode mode, bool smoothTransition)
        {
            if (mode == null || mode.virtualCamera == null) return;
            
            var newCamera = FindCameraByName(mode.GetEffectiveModeId());
            if (newCamera == null)
            {
                Debug.LogError($"[CinemachineCameraManager] No camera found with name/ID '{mode.GetEffectiveModeId()}' in scene. Make sure there's a CinemachineCamera in the scene with this name or a CameraModeIdentifier with matching ID.");
                return;
            }
            
            if (debugMode)
            {
                Debug.Log($"[CinemachineCameraManager] Found camera '{newCamera.name}' on GameObject '{newCamera.gameObject.name}'");
            }
            
            if (activeCamera == newCamera)
            {
                return;
            }
            
            var previousCamera = activeCamera;
            activeCamera = newCamera;
            
            ManageCameraStates(newCamera);
            
            // After switching cameras, update the TrackingTarget to ensure it's set correctly
            // This is important because UpdateCinemachineTargets might have been called before activeCamera was set
            UpdateTrackingTarget();
            
            if (debugMode)
            {
                string transitionText = smoothTransition ? "smooth" : "instant";
                Debug.Log($"[CinemachineCameraManager] Switched to camera '{activeCamera.name}' ({transitionText}) - Priority: {activeCamera.Priority}");
            }
        }
        
        /// <summary>
        /// Gets the currently active camera mode
        /// </summary>
        public CameraMode GetCurrentMode()
        {
            return currentMode;
        }
        
        /// <summary>
        /// Finds a camera by name in the scene (public version for editor access)
        /// </summary>
        public CinemachineCamera FindCameraByName(string cameraName)
        {
            if (string.IsNullOrEmpty(cameraName)) return null;
            
            if (debugMode)
            {
                Debug.Log($"[CinemachineCameraManager] Searching for camera with name/ID '{cameraName}'");
            }
            
            var allCameras = FindObjectsOfType<CinemachineCamera>(true);
            
            if (debugMode)
            {
                Debug.Log($"[CinemachineCameraManager] Found {allCameras.Length} CinemachineCamera objects in scene:");
                foreach (var camera in allCameras)
                {
                    var identifier = camera.GetComponent<CameraModeIdentifier>();
                    string idInfo = identifier != null ? $" [ID: {identifier.ModeId}]" : "";
                    Debug.Log($"  - Name: '{camera.name}', GameObject: '{camera.gameObject.name}'{idInfo}");
                }
            }
            
            // First pass: Try to find by CameraModeIdentifier
            foreach (var camera in allCameras)
            {
                var identifier = camera.GetComponent<CameraModeIdentifier>();
                if (identifier != null && identifier.Matches(cameraName))
                {
                    if (debugMode)
                    {
                        Debug.Log($"[CinemachineCameraManager] Found camera by ID: '{identifier.ModeId}' on GameObject '{camera.gameObject.name}'");
                    }
                    return camera;
                }
            }
            
            // Second pass: Fallback to name-based search for backward compatibility
            foreach (var camera in allCameras)
            {
                if (camera.name == cameraName)
                {
                    if (debugMode)
                    {
                        Debug.Log($"[CinemachineCameraManager] Found matching camera by name: '{camera.name}' on GameObject '{camera.gameObject.name}'");
                    }
                    return camera;
                }
            }
            
            if (debugMode)
            {
                Debug.LogWarning($"[CinemachineCameraManager] No camera found with name/ID '{cameraName}' in scene");
            }
            
            return null;
        }
        
        /// <summary>
        /// Gets the current camera parent transform
        /// </summary>
        public Transform GetCameraParent()
        {
            return cameraParent;
        }
        
        /// <summary>
        /// Sets the camera parent transform for new camera instantiations
        /// </summary>
        public void SetCameraParent(Transform parent)
        {
            cameraParent = parent;
        }
        
        public CinemachineCamera InstantiateCameraInScene(CinemachineCamera cameraPrefab)
        {
            if (cameraPrefab == null)
            {
                Debug.LogError("[CinemachineCameraManager] Cannot instantiate null camera prefab");
                return null;
            }
            
            var existingCamera = FindCameraByName(cameraPrefab.name);
            if (existingCamera != null)
            {
                if (debugMode)
                {
                    Debug.Log($"[CinemachineCameraManager] Camera '{cameraPrefab.name}' already exists in scene");
                }
                return existingCamera;
            }
            
            GameObject cameraInstance;
#if UNITY_EDITOR
            cameraInstance = PrefabUtility.InstantiatePrefab(cameraPrefab.gameObject) as GameObject;
#else
            cameraInstance = Instantiate(cameraPrefab.gameObject);
#endif
            
            if (cameraParent != null)
            {
                cameraInstance.transform.SetParent(cameraParent);
            }
            cameraInstance.SetActive(false);
            
            // Get the CinemachineCamera component
            var cinemachineCamera = cameraInstance.GetComponent<CinemachineCamera>();
            if (cinemachineCamera == null)
            {
                Debug.LogError($"[CinemachineCameraManager] Instantiated camera '{cameraInstance.name}' has no CinemachineCamera component");
                DestroyImmediate(cameraInstance);
                return null;
            }
            
            cinemachineCamera.Priority = 0;
            
            if (debugMode)
            {
                Debug.Log($"[CinemachineCameraManager] Instantiated camera '{cameraInstance.name}' from prefab '{cameraPrefab.name}' in scene");
            }
            
            return cinemachineCamera;
        }

        private void UpdateCinemachineTargets(float? overrideRadius = null)
        {
            if (targetGroup == null) return;
            
            // Update target group with current active targets
            targetGroup.Targets.Clear();
            
            foreach (var target in activeTargets)
            {
                if (target != null)
                {
                    // Get radius for this specific target:
                    // 1. Use override if provided (for setting all targets at once)
                    // 2. Use stored radius for this target
                    // 3. Use current mode's follow settings
                    // 4. Default to 1f
                    float targetRadius = overrideRadius ?? 
                                        (targetRadii.ContainsKey(target) ? targetRadii[target] : 
                                        (currentMode?.followSettings?.targetRadius ?? 1f));
                    
                    var cinemachineTarget = new CinemachineTargetGroup.Target
                    {
                        Object = target,
                        Weight = 1f,
                        Radius = targetRadius
                    };
                    targetGroup.Targets.Add(cinemachineTarget);
                }
            }
            
            targetGroup.RotationMode = CinemachineTargetGroup.RotationModes.GroupAverage;
            
            // Update TrackingTarget on the active camera if it exists
            // If activeCamera is null (e.g., during initialization), we'll update it later when the camera is set
            if (activeCamera != null)
            {
                UpdateTrackingTarget();
            }
            
            if (debugMode && activeTargets.Count > 0)
            {
                string targetInfo = string.Join(", ", activeTargets.Select(t => 
                {
                    float r = targetRadii.ContainsKey(t) ? targetRadii[t] : 1f;
                    return $"{t.name}(r:{r:F1})";
                }));
                string trackingTargetInfo = activeCamera != null 
                    ? (activeCamera.Target.TrackingTarget != null ? activeCamera.Target.TrackingTarget.name : "null")
                    : "activeCamera is null";
                Debug.Log($"[CinemachineCameraManager] Updated Cinemachine targets: [{targetInfo}], TrackingTarget: {trackingTargetInfo}");
            }
        }
        
        /// <summary>
        /// Updates the TrackingTarget on the active camera based on current targets and mode settings.
        /// This is called separately to ensure it runs even if activeCamera wasn't set when UpdateCinemachineTargets was called.
        /// </summary>
        private void UpdateTrackingTarget()
        {
            if (activeCamera == null || targetGroup == null) return;
            
            // Set TrackingTarget to targetGroup if we have targets AND the current mode has follow enabled
            // This ensures the camera follows the target group when appropriate
            if (activeTargets.Count > 0 && currentMode != null && currentMode.enableFollow)
            {
                activeCamera.Target.TrackingTarget = targetGroup.transform;
                
                if (debugMode)
                {
                    Debug.Log($"[CinemachineCameraManager] Set TrackingTarget to targetGroup (mode: {currentMode.modeName}, enableFollow: {currentMode.enableFollow}, targets: {activeTargets.Count})");
                }
            }
            else if (activeTargets.Count > 0)
            {
                // If we have targets but follow is disabled, clear the TrackingTarget
                // This prevents the camera from following when it shouldn't
                activeCamera.Target.TrackingTarget = null;
                
                if (debugMode)
                {
                    string modeInfo = currentMode != null ? $"mode: {currentMode.modeName}, enableFollow: {currentMode.enableFollow}" : "no current mode";
                    Debug.Log($"[CinemachineCameraManager] Cleared TrackingTarget despite having targets ({modeInfo})");
                }
            }
            else
            {
                // No targets, clear TrackingTarget
                activeCamera.Target.TrackingTarget = null;
            }
        }
        
        private void ApplyModeSettings(CameraMode mode)
        {
            if (mode.virtualCamera == null) return;
            
            if (mode.cameraSettings != null)
            {
                ApplyCameraSettings(mode.cameraSettings);
            }
            
            if (mode.followSettings != null)
            {
                ApplyFollowSettings(mode.followSettings);
            }
            
            if (mode.transitionSettings != null)
            {
                ApplyTransitionSettings(mode.transitionSettings);
            }
        }
        
        private void ApplyCameraSettings(CameraSettings settings)
        {
            if (activeCamera == null) 
            {
                if (debugMode)
                {
                    Debug.LogWarning("[CinemachineCameraManager] ApplyCameraSettings called but activeCamera is null!");
                }
                return;
            }
            
            if (debugMode)
            {
                Debug.Log($"[CinemachineCameraManager] Applying camera settings to SCENE INSTANCE '{activeCamera.name}' (GameObject: {activeCamera.gameObject.GetInstanceID()}): FOV={settings.fieldOfView}, Pitch={settings.pitchAngle}°");
            }
            
            try
            {
                var lensSettings = activeCamera.Lens;
                lensSettings.FieldOfView = settings.fieldOfView;
                lensSettings.NearClipPlane = settings.nearClipPlane;
                lensSettings.FarClipPlane = settings.farClipPlane;
                activeCamera.Lens = lensSettings;
                
                if (debugMode)
                {
                    Debug.Log($"[CinemachineCameraManager] Applied camera settings via Cinemachine API: FOV={lensSettings.FieldOfView}");
                }
            }
            catch (System.Exception ex)
            {
                if (debugMode)
                {
                    Debug.LogWarning($"[CinemachineCameraManager] Could not apply camera settings via Cinemachine API: {ex.Message}");
                }
            }
            
            // Copy transform from the camera mode's virtual camera prefab
            if (currentMode != null && currentMode.virtualCamera != null)
            {
                // Copy position and rotation from the prefab
                activeCamera.transform.position = currentMode.virtualCamera.transform.position;
                activeCamera.transform.rotation = currentMode.virtualCamera.transform.rotation;
                
                if (debugMode)
                {
                    Debug.Log($"[CinemachineCameraManager] Applied transform from prefab: Position={currentMode.virtualCamera.transform.position}, Rotation={currentMode.virtualCamera.transform.rotation.eulerAngles}");
                }
            }
            
            // Apply pitch angle override if specified (takes priority over prefab rotation)
            if (settings.pitchAngle != 0f)
            {
                var currentRotation = activeCamera.transform.rotation;
                var newRotation = Quaternion.Euler(settings.pitchAngle, currentRotation.eulerAngles.y, currentRotation.eulerAngles.z);
                activeCamera.transform.rotation = newRotation;
                
                if (debugMode)
                {
                    Debug.Log($"[CinemachineCameraManager] Applied pitch angle override: {settings.pitchAngle}°");
                }
            }
            
            // Apply ground height override if specified (takes priority over prefab position)
            if (settings.useInitialGroundHeight)
            {
                var currentPosition = activeCamera.transform.position;
                activeCamera.transform.position = new Vector3(currentPosition.x, settings.initialGroundHeight, currentPosition.z);
                
                if (debugMode)
                {
                    Debug.Log($"[CinemachineCameraManager] Applied ground height override: {settings.initialGroundHeight}");
                }
            }
            
        }
        
        private void ApplyFollowSettings(FollowSettings settings)
        {
            if (activeCamera == null) return;
            
            var followComponent = activeCamera.GetComponent<CinemachineFollow>();
            if (followComponent != null)
            {
                followComponent.FollowOffset = settings.GetEffectiveFollowOffset();
                
                
                if (debugMode)
                {
                    Debug.Log($"[CinemachineCameraManager] Applied follow settings: Offset={settings.GetEffectiveFollowOffset()}, Speed={settings.followSpeed} (damping not applied - needs API verification)");
                }
            }
            
            var rotationComposer = activeCamera.GetComponent<CinemachineRotationComposer>();
            if (rotationComposer != null)
            {
                
                if (debugMode)
                {
                    Debug.Log($"[CinemachineCameraManager] Applied rotation settings: LookAtSpeed={settings.lookAtSpeed} (damping not applied - needs API verification)");
                }
            }
            
            if (settings.useGroupFraming)
            {
                var groupFraming = activeCamera.GetComponent<CinemachineGroupFraming>();
                if (groupFraming != null)
                {
                    groupFraming.DollyRange = new Vector2(settings.dollyDistanceMin, settings.dollyDistanceMax);
                    
                    groupFraming.FramingSize = settings.framingSize;
                    
                    if (debugMode)
                    {
                        Debug.Log($"[CinemachineCameraManager] Applied group framing settings: Dolly={settings.dollyDistanceMin}-{settings.dollyDistanceMax}, FramingSize={settings.framingSize}");
                    }
                }
                else
                {
                    if (debugMode)
                    {
                        Debug.LogWarning("[CinemachineCameraManager] CinemachineGroupFraming component not found - group framing will not work");
                    }
                }
            }
            
            if (targetGroup != null && activeTargets.Count > 1)
            {
                ApplyTargetGroupSettings(settings);
            }
        }
        
        private void ApplyTargetGroupSettings(FollowSettings settings)
        {
            if (targetGroup == null) return;
            
            for (int i = 0; i < targetGroup.Targets.Count && i < activeTargets.Count; i++)
            {
                var target = targetGroup.Targets[i];
                target.Weight = settings.GetTargetWeight(i);
            }
            
            if (debugMode)
            {
                Debug.Log($"[CinemachineCameraManager] Applied target group settings: PrimaryWeight={settings.primaryTargetWeight}, SecondaryWeight={settings.secondaryTargetWeight}");
            }
        }
        
        private void ApplyTransitionSettings(TransitionSettings settings)
        {
            if (activeCamera == null) return;
            
            if (brain != null)
            {
                if (debugMode)
                {
                    Debug.Log($"[CinemachineCameraManager] Applied transition settings: Duration={settings.blendDuration}");
                }
            }
        }
        
        private void UpdateTargetsForMode(CameraMode mode)
        {
            UpdateCinemachineTargets();
        }
        
        #endregion
        
        #region Editor Support
        
        #if UNITY_EDITOR
        void OnValidate()
        {
            if (Application.isPlaying)
            {
                BuildModeLookup();
            }
        }
        #endif
        
        #endregion
    }
}

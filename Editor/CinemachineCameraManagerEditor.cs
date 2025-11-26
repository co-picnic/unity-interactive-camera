using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using InteractiveCameraSystem;
using Unity.Cinemachine;

namespace InteractiveCameraSystem.Editor
{
    [CustomEditor(typeof(CinemachineCameraManager))]
    public class CinemachineCameraManagerEditor : UnityEditor.Editor
    {
        private CinemachineCameraManager manager;
        private SerializedProperty cameraModesProp;
        private SerializedProperty defaultModeProp;
        private SerializedProperty debugModeProp;
        private SerializedProperty defaultTransitionDurationProp;
        private SerializedProperty brainProp;
        private SerializedProperty targetGroupProp;
        private SerializedProperty cameraParentProp;
        private SerializedProperty autoDiscoverCamerasProp;
        private SerializedProperty autoInstantiateMissingCamerasOnPlayProp;
        
        // Foldout states
        private bool _cameraModesFolder = true;
        
        private void OnEnable()
        {
            manager = (CinemachineCameraManager)target;
            cameraModesProp = serializedObject.FindProperty("cameraModes");
            defaultModeProp = serializedObject.FindProperty("defaultMode");
            debugModeProp = serializedObject.FindProperty("debugMode");
            defaultTransitionDurationProp = serializedObject.FindProperty("defaultTransitionDuration");
            brainProp = serializedObject.FindProperty("brain");
            targetGroupProp = serializedObject.FindProperty("targetGroup");
            cameraParentProp = serializedObject.FindProperty("cameraParent");
            autoDiscoverCamerasProp = serializedObject.FindProperty("autoDiscoverCameras");
            autoInstantiateMissingCamerasOnPlayProp = serializedObject.FindProperty("autoInstantiateMissingCamerasOnPlay");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.LabelField("Cinemachine Camera Manager", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Mode management
            DrawModeManagement();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space(5);
            
            // Settings
            DrawSettings();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space(5);
            
            // Debug info
            DrawDebugInfo();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawModeManagement()
        {
            // Camera Modes section with distinguished header
            _cameraModesFolder = VirtualCameraEditorGUI.DrawProfileFoldout(
                "📷 Camera Modes", 
                _cameraModesFolder, 
                () => {
                    // Custom mode list
                    DrawCustomModeList();
                    
                    EditorGUILayout.Space(8f);
                    
                    // Essential Tools
                    EditorGUILayout.LabelField("Essential Tools", EditorStyles.boldLabel);
                    EditorGUILayout.HelpBox("Core tools for camera system management.", MessageType.Info);
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Auto-Discover Modes", GUILayout.Height(25)))
                    {
                        manager.AutoDiscoverCameraModes();
                        RefreshModeList();
                    }
                    if (GUILayout.Button("Instantiate All Cameras", GUILayout.Height(25)))
                    {
                        InstantiateAllCameras();
                    }
                    EditorGUILayout.EndHorizontal();
                },
                null,
                () => ShowCameraModesContextMenu());
        }
        
        private void DrawCustomModeList()
        {
            if (cameraModesProp.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No camera modes configured. Right-click on a CinemachineCamera in the scene and select 'Create Camera Mode' to add one.", MessageType.Info);
                return;
            }
            
            // Mode list
            for (int i = 0; i < cameraModesProp.arraySize; i++)
            {
                var modeProperty = cameraModesProp.GetArrayElementAtIndex(i);
                var mode = modeProperty.objectReferenceValue as CameraMode;
                
                if (mode == null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"<null>");
                    if (GUILayout.Button("Remove", GUILayout.Width(60)))
                    {
                        cameraModesProp.DeleteArrayElementAtIndex(i);
                        i--;
                    }
                    EditorGUILayout.EndHorizontal();
                    continue;
                }
                
                // Mode row
                EditorGUILayout.BeginHorizontal();
                
                // Mode name
                EditorGUILayout.LabelField(mode.modeName, EditorStyles.boldLabel);
                
                // Action buttons
                if (GUILayout.Button("Switch To", GUILayout.Width(70)))
                {
                    SwitchToMode(mode);
                }
                
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeObject = mode;
                    EditorGUIUtility.PingObject(mode);
                }
                
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    if (EditorUtility.DisplayDialog("Remove Camera Mode", 
                        $"Are you sure you want to remove '{mode.modeName}'?", "Yes", "No"))
                    {
                        cameraModesProp.DeleteArrayElementAtIndex(i);
                        i--;
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
        
        private void DrawSettings()
        {
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(defaultModeProp, new GUIContent("Default Mode", "Camera mode to activate on startup"));
            EditorGUILayout.PropertyField(debugModeProp);
            EditorGUILayout.PropertyField(defaultTransitionDurationProp);
            
            EditorGUILayout.Space();
            
            // Cinemachine Components section
            EditorGUILayout.LabelField("Cinemachine Components", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(brainProp, new GUIContent("Brain", "Cinemachine Brain component (auto-discovered if not set)"));
            EditorGUILayout.PropertyField(targetGroupProp, new GUIContent("Target Group", "Cinemachine Target Group for multi-target following"));
            
            EditorGUILayout.Space();
            
            // Camera Management section
            EditorGUILayout.LabelField("Camera Management", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(cameraParentProp, new GUIContent("Camera Parent", "Parent transform for instantiated cameras (optional)"));
            
            EditorGUILayout.Space();
            
            // Camera Auto-Management section
            EditorGUILayout.LabelField("Camera Auto-Management", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(autoDiscoverCamerasProp, new GUIContent("Auto-Discover Cameras", "Automatically scan the scene for existing cameras on play"));
            EditorGUILayout.PropertyField(autoInstantiateMissingCamerasOnPlayProp, new GUIContent("Auto-Instantiate Missing Cameras on Play", "Automatically instantiate missing cameras from registered modes when entering play mode"));
            
            EditorGUILayout.Space();
        }
        
        private void DrawDebugInfo()
        {
            if (!debugModeProp.boolValue) return;
            
            EditorGUILayout.LabelField("Debug Information", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Mode Count:", cameraModesProp.arraySize.ToString());
            EditorGUILayout.LabelField("Active Targets:", manager.ActiveTargets?.Count.ToString() ?? "0");
            EditorGUILayout.LabelField("Is Transitioning:", manager.IsTransitioning.ToString());
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
        }
        
        #region Helper Methods
        
        private void RefreshModeList()
        {
            // Refresh the mode list display
            serializedObject.Update();
            
            // Auto-discover cameras in the scene and suggest creating modes for them
            var camerasInScene = FindObjectsOfType<CinemachineCamera>();
            var existingModes = manager.GetAllModes();
            var camerasWithoutModes = new List<CinemachineCamera>();
            
            foreach (var camera in camerasInScene)
            {
                bool hasMode = false;
                foreach (var mode in existingModes)
                {
                    if (mode.virtualCamera == camera)
                    {
                        hasMode = true;
                        break;
                    }
                }
                
                if (!hasMode)
                {
                    camerasWithoutModes.Add(camera);
                }
            }
            
            if (camerasWithoutModes.Count > 0)
            {
                Debug.Log($"[CinemachineCameraManagerEditor] Found {camerasWithoutModes.Count} cameras in scene without camera modes. Right-click on them to create modes.");
            }
        }
        
        private void AutoCreateModesForSceneCameras()
        {
            var camerasInScene = FindObjectsOfType<CinemachineCamera>();
            var existingModes = manager.GetAllModes();
            int createdCount = 0;
            
            foreach (var camera in camerasInScene)
            {
                bool hasMode = false;
                foreach (var mode in existingModes)
                {
                    if (mode.virtualCamera == camera)
                    {
                        hasMode = true;
                        break;
                    }
                }
                
                if (!hasMode)
                {
                    CinemachineCameraContextMenu.CreateCameraModeFromCinemachineCamera(new MenuCommand(camera.gameObject));
                    createdCount++;
                }
            }
            
            if (createdCount > 0)
            {
                Debug.Log($"[CinemachineCameraManagerEditor] Auto-created {createdCount} camera modes for scene cameras.");
                RefreshModeList();
            }
            else
            {
                Debug.Log("[CinemachineCameraManagerEditor] All cameras in scene already have camera modes.");
            }
        }
        
        private void InstantiateAllCameras()
        {
            int instantiatedCount = 0;
            int skippedCount = 0;
            int errorCount = 0;
            
            Debug.Log("[CinemachineCameraManagerEditor] Starting camera instantiation...");
            
            // Ensure we have a "Cameras" parent GameObject in the scene
            GameObject camerasParent = GameObject.Find("Cameras");
            if (camerasParent == null)
            {
                camerasParent = new GameObject("Cameras");
                Debug.Log("[CinemachineCameraManagerEditor] Created 'Cameras' parent GameObject");
            }
            else
            {
                Debug.Log("[CinemachineCameraManagerEditor] Found existing 'Cameras' parent GameObject");
            }
            
            // Temporarily set the camera parent to our "Cameras" GameObject
            var originalCameraParent = manager.GetCameraParent();
            manager.SetCameraParent(camerasParent.transform);
            
            // Instantiate all camera modes in the scene for edit mode visibility
            foreach (var mode in manager.GetAllModes())
            {
                if (mode == null)
                {
                    Debug.LogWarning("[CinemachineCameraManagerEditor] Found null mode, skipping.");
                    errorCount++;
                    continue;
                }
                
                if (mode.virtualCamera == null)
                {
                    Debug.LogWarning($"[CinemachineCameraManagerEditor] Mode '{mode.modeName}' has no virtual camera reference, skipping.");
                    errorCount++;
                    continue;
                }
                
                Debug.Log($"[CinemachineCameraManagerEditor] Attempting to instantiate camera for mode '{mode.modeName}' (prefab: '{mode.virtualCamera.name}')");
                
                try
                {
                    var result = manager.InstantiateCameraInScene(mode.virtualCamera);
                    if (result != null)
                    {
                        instantiatedCount++;
                        Debug.Log($"[CinemachineCameraManagerEditor] Successfully instantiated camera '{result.name}' for mode '{mode.modeName}' under 'Cameras' GameObject");
                    }
                    else
                    {
                        skippedCount++;
                        Debug.Log($"[CinemachineCameraManagerEditor] Camera for mode '{mode.modeName}' already exists or instantiation failed");
                    }
                }
                catch (System.Exception e)
                {
                    errorCount++;
                    Debug.LogError($"[CinemachineCameraManagerEditor] Failed to instantiate camera for mode '{mode.modeName}': {e.Message}");
                }
            }
            
            // Restore the original camera parent
            manager.SetCameraParent(originalCameraParent);
            
            // Provide summary
            Debug.Log($"[CinemachineCameraManagerEditor] Camera instantiation complete:");
            Debug.Log($"  - Successfully instantiated: {instantiatedCount}");
            Debug.Log($"  - Skipped (already exists): {skippedCount}");
            Debug.Log($"  - Errors: {errorCount}");
            Debug.Log($"[CinemachineCameraManagerEditor] All cameras placed under 'Cameras' GameObject in scene hierarchy");
            
            if (instantiatedCount == 0 && skippedCount == 0 && errorCount == 0)
            {
                Debug.Log("[CinemachineCameraManagerEditor] No camera modes found to instantiate.");
            }
        }
        
        private void UpdateAllComponents()
        {
            // Update all camera components based on their mode settings
            foreach (var mode in manager.GetAllModes())
            {
                if (mode?.virtualCamera != null)
                {
                    manager.UpdateCameraComponents(mode);
                }
            }
        }
        
        private void DebugInputSystem()
        {
            Debug.Log("=== INPUT SYSTEM DEBUG ===");
            
            // Check CinemachineInputManager
            var inputManager = CinemachineInputManager.Instance;
            if (inputManager == null)
            {
                Debug.LogError("❌ CinemachineInputManager not found in scene!");
                return;
            }
            Debug.Log("✅ CinemachineInputManager found");
            
            // Check FingerManager
            var fingerManager = FingerManager.Instance;
            if (fingerManager == null)
            {
                Debug.LogError("❌ FingerManager not found in scene!");
                return;
            }
            Debug.Log("✅ FingerManager found");
            
            // Check current mode
            var currentMode = manager.GetCurrentMode();
            if (currentMode == null)
            {
                Debug.LogWarning("⚠️ No current mode set");
                return;
            }
            Debug.Log($"📷 Current mode: {currentMode.modeName}");
            Debug.Log($"   - Enable Drag: {currentMode.enableDrag}");
            Debug.Log($"   - Enable Zoom: {currentMode.enableZoom}");
            
            // Check camera instance
            var cameraInstance = manager.FindCameraByName(currentMode.virtualCamera.name);
            if (cameraInstance == null)
            {
                Debug.LogError($"❌ Camera '{currentMode.virtualCamera.name}' not found in scene!");
                return;
            }
            Debug.Log($"✅ Camera '{cameraInstance.name}' found in scene");
            
            // Check input components on camera
            var dragComponent = cameraInstance.GetComponent<CinemachineDragInputComponent>();
            if (dragComponent == null)
            {
                Debug.LogError($"❌ CinemachineDragInputComponent not found on camera '{cameraInstance.name}'");
                Debug.LogError("   → Try clicking 'Update All Components' button to add missing components");
            }
            else
            {
                Debug.Log($"✅ CinemachineDragInputComponent found on camera");
                Debug.Log($"   - IsActive: {dragComponent.IsActive}");
                Debug.Log($"   - Enabled: {dragComponent.enabled}");
            }
            
            var zoomComponent = cameraInstance.GetComponent<CinemachineZoomInputComponent>();
            if (zoomComponent == null)
            {
                Debug.LogWarning($"⚠️ CinemachineZoomInputComponent not found on camera '{cameraInstance.name}'");
            }
            else
            {
                Debug.Log($"✅ CinemachineZoomInputComponent found on camera");
                Debug.Log($"   - IsActive: {zoomComponent.IsActive}");
                Debug.Log($"   - Enabled: {zoomComponent.enabled}");
            }
            
            // Check camera GameObject state
            Debug.Log($"📷 Camera GameObject '{cameraInstance.gameObject.name}':");
            Debug.Log($"   - Active in Hierarchy: {cameraInstance.gameObject.activeInHierarchy}");
            Debug.Log($"   - Active Self: {cameraInstance.gameObject.activeSelf}");
            Debug.Log($"   - Camera Priority: {cameraInstance.Priority}");
            
            // Check if camera has proper Cinemachine components
            var transposer = cameraInstance.GetComponent<CinemachineTransposer>();
            var composer = cameraInstance.GetComponent<CinemachineComposer>();
            Debug.Log($"🔧 Cinemachine Components:");
            Debug.Log($"   - Transposer: {(transposer != null ? "✅ Found" : "❌ Missing")}");
            Debug.Log($"   - Composer: {(composer != null ? "✅ Found" : "❌ Missing")}");
            
            Debug.Log("=== END INPUT SYSTEM DEBUG ===");
        }
        
        private void SwitchToMode(CameraMode mode)
        {
            if (mode == null)
            {
                Debug.LogWarning("[CinemachineCameraManagerEditor] Cannot switch to null mode");
                return;
            }
            
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[CinemachineCameraManagerEditor] Mode switching is only available during play mode");
                return;
            }
            
            if (manager == null)
            {
                Debug.LogError("[CinemachineCameraManagerEditor] Manager is null");
                return;
            }
            
            try
            {
                bool success = manager.SwitchToMode(mode, true);
                if (success)
                {
                    Debug.Log($"[CinemachineCameraManagerEditor] Successfully switched to mode: {mode.modeName}");
                    // Force a repaint to update the UI
                    Repaint();
                }
                else
                {
                    Debug.LogWarning($"[CinemachineCameraManagerEditor] Failed to switch to mode: {mode.modeName}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CinemachineCameraManagerEditor] Error switching to mode '{mode.modeName}': {ex.Message}");
            }
        }
        
        #endregion
        
        /// <summary>
        /// Show context menu for Camera Modes section
        /// </summary>
        private void ShowCameraModesContextMenu()
        {
            var menu = new GenericMenu();
            
            menu.AddItem(new GUIContent("Refresh Modes"), false, () => {
                RefreshModeList();
            });
            menu.AddItem(new GUIContent("Instantiate All Cameras"), false, () => {
                InstantiateAllCameras();
            });
            menu.AddItem(new GUIContent("Update All Components"), false, () => {
                UpdateAllComponents();
            });
            
            menu.ShowAsContext();
        }
    }
    
    /// <summary>
    /// Context menu for CinemachineCamera GameObjects to create camera modes
    /// </summary>
    public static class CinemachineCameraContextMenu
    {
        [MenuItem("CONTEXT/CinemachineCamera/Create Camera Mode")]
        public static void CreateCameraModeFromCinemachineCamera(MenuCommand command)
        {
            var cinemachineCamera = command.context as CinemachineCamera;
            if (cinemachineCamera == null) return;
            
            // Find the CinemachineCameraManager in the scene
            var cameraManager = Object.FindFirstObjectByType<CinemachineCameraManager>();
            if (cameraManager == null)
            {
                Debug.LogError("[CinemachineCameraContextMenu] No CinemachineCameraManager found in scene! Please add one first.");
                return;
            }
            
            // Create a new CameraMode asset
            string modeName = cinemachineCamera.name.Replace("CM vcam", "").Trim();
            if (string.IsNullOrEmpty(modeName))
            {
                modeName = "NewCameraMode";
            }
            
            // Create the CameraMode asset
            var cameraMode = ScriptableObject.CreateInstance<CameraMode>();
            cameraMode.modeName = modeName;
            cameraMode.description = $"Camera mode created from {cinemachineCamera.name}";
            cameraMode.priority = 5;
            cameraMode.isEnabled = true;
            
            // Convert the scene instance into a prefab
            string prefabPath = $"Assets/CinemachineCameras/{modeName}_Camera.prefab";
            string prefabDirectory = System.IO.Path.GetDirectoryName(prefabPath);
            if (!System.IO.Directory.Exists(prefabDirectory))
            {
                System.IO.Directory.CreateDirectory(prefabDirectory);
            }
            
            // Convert the scene instance into a prefab and connect it
            var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(cinemachineCamera.gameObject, prefabPath, InteractionMode.UserAction);
            var prefabCinemachineCamera = prefab.GetComponent<CinemachineCamera>();
            
            // Assign the prefab to the mode (not the scene instance)
            cameraMode.virtualCamera = prefabCinemachineCamera;
            
            // Save the CameraMode asset
            string assetPath = $"Assets/Plugins/InteractiveCameraSystem/Examples/CameraModeAssets/{modeName}.asset";
            string directory = System.IO.Path.GetDirectoryName(assetPath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            
            AssetDatabase.CreateAsset(cameraMode, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Add to the camera manager's list
            var serializedManager = new SerializedObject(cameraManager);
            var cameraModesProperty = serializedManager.FindProperty("cameraModes");
            cameraModesProperty.arraySize++;
            var newModeProperty = cameraModesProperty.GetArrayElementAtIndex(cameraModesProperty.arraySize - 1);
            newModeProperty.objectReferenceValue = cameraMode;
            serializedManager.ApplyModifiedProperties();
            
            // Rename the scene camera to match the mode
            cinemachineCamera.name = $"{modeName}_Camera";
            
            Debug.Log($"[CinemachineCameraContextMenu] Created camera mode '{modeName}' and added to CinemachineCameraManager");
            Debug.Log($"[CinemachineCameraContextMenu] Converted scene instance to prefab '{prefab.name}' at {prefabPath}");
            Debug.Log($"[CinemachineCameraContextMenu] Scene camera is now connected to prefab and renamed to '{cinemachineCamera.name}'");
            
            // Select the created asset
            Selection.activeObject = cameraMode;
            EditorGUIUtility.PingObject(cameraMode);
        }
    }
}
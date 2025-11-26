using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using InteractiveCameraSystem;

namespace InteractiveCameraSystem.Editor
{
    /// <summary>
    /// Menu items for creating InteractiveCameraSystem assets in the project
    /// </summary>
    public static class InteractiveCameraSystemMenuItems
    {
        [MenuItem("Assets/Create/InteractiveCameraSystem/Interactive Camera System", priority = 1)]
        public static void CreateCameraMode()
        {
            // Get the currently selected folder or use Assets root
            string folderPath = GetSelectedFolderPath();
            
            // Create the camera mode asset
            CameraMode cameraMode = ScriptableObject.CreateInstance<CameraMode>();
            cameraMode.modeName = "New Camera Mode";
            cameraMode.description = "A new camera mode";
            cameraMode.isEnabled = true;
            cameraMode.priority = 10;
            
            // Set default settings
            cameraMode.cameraSettings.fieldOfView = 60f;
            cameraMode.cameraSettings.nearClipPlane = 0.1f;
            cameraMode.cameraSettings.farClipPlane = 1000f;
            
            cameraMode.followSettings.followSpeed = 2f;
            cameraMode.followSettings.lookAtSpeed = 2f;
            
            cameraMode.dragSettings.horizontalSensitivity = 2f;
            cameraMode.dragSettings.verticalSensitivity = 2f;
            cameraMode.dragSettings.damping = 5f;
            
            cameraMode.zoomSettings.minFOV = 20f;
            cameraMode.zoomSettings.maxFOV = 80f;
            cameraMode.zoomSettings.zoomSpeed = 3f;
            
            cameraMode.transitionSettings.blendDuration = 1f;
            cameraMode.transitionSettings.transitionPriority = 50;
            
            // Create the asset
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/NewCameraMode.asset");
            AssetDatabase.CreateAsset(cameraMode, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Select the created asset
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = cameraMode;
            
            Debug.Log($"Created Camera Mode: {assetPath}");
        }
        
        [MenuItem("Assets/Create/InteractiveCameraSystem/Interactive Camera Prefab", priority = 2)]
        public static void CreateCameraPrefab()
        {
            // Get the currently selected folder or use Assets root
            string folderPath = GetSelectedFolderPath();
            
            // Create a new GameObject
            GameObject cameraPrefab = new GameObject("NewCameraPrefab");
            
            // Add CinemachineVirtualCamera component
            var cinemachineCamera = cameraPrefab.AddComponent<Unity.Cinemachine.CinemachineCamera>();
            cinemachineCamera.Priority = 0;
            cinemachineCamera.Lens.FieldOfView = 60f;
            cinemachineCamera.Lens.NearClipPlane = 0.1f;
            cinemachineCamera.Lens.FarClipPlane = 1000f;
            
            // Add input components
            var dragInput = cameraPrefab.AddComponent<CinemachineDragInputComponent>();
            var zoomInput = cameraPrefab.AddComponent<CinemachineZoomInputComponent>();
            
            // Set default input settings
            dragInput.HorizontalSensitivity = 2f;
            dragInput.VerticalSensitivity = 2f;
            dragInput.Damping = 5f;
            dragInput.EnableMomentum = true;
            dragInput.Momentum = 0.8f;
            
            zoomInput.ZoomSensitivity = 1f;
            zoomInput.MinFOV = 20f;
            zoomInput.MaxFOV = 80f;
            zoomInput.EnableSmoothZoom = true;
            zoomInput.ZoomSpeed = 3f;
            
            // Create the prefab
            string prefabPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/NewCameraPrefab.prefab");
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(cameraPrefab, prefabPath);
            
            // Clean up the temporary GameObject
            Object.DestroyImmediate(cameraPrefab);
            
            // Select the created prefab
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = prefab;
            
            Debug.Log($"Created Camera Prefab: {prefabPath}");
        }
        
        [MenuItem("Tools/InteractiveCameraSystem/Copy Example Assets to Project", priority = 1)]
        public static void CopyExampleAssetsToProject()
        {
            // Create the main folder structure
            string baseFolder = "Assets/InteractiveCameraAssets";
            string cameraModesFolder = $"{baseFolder}/CameraModes";
            string cameraPrefabsFolder = $"{baseFolder}/CameraPrefabs";
            string managerFolder = $"{baseFolder}/Manager";
            
            // Create folders if they don't exist
            CreateFolderIfNotExists(baseFolder);
            CreateFolderIfNotExists(cameraModesFolder);
            CreateFolderIfNotExists(cameraPrefabsFolder);
            CreateFolderIfNotExists(managerFolder);
            
            // Copy camera mode assets
            CopyAsset("Packages/com.interactivecamera.interactivecamera-system/Examples/CameraModeAssets/ConversationMode.asset", 
                     $"{cameraModesFolder}/ConversationMode.asset");
            CopyAsset("Packages/com.interactivecamera.interactivecamera-system/Examples/CameraModeAssets/ExplorationMode.asset", 
                     $"{cameraModesFolder}/ExplorationMode.asset");
            CopyAsset("Packages/com.interactivecamera.interactivecamera-system/Examples/CameraModeAssets/FollowMode.asset", 
                     $"{cameraModesFolder}/FollowMode.asset");
            CopyAsset("Packages/com.interactivecamera.interactivecamera-system/Examples/CameraModeAssets/IntroMode.asset", 
                     $"{cameraModesFolder}/IntroMode.asset");
            
            // Copy camera prefabs
            CopyAsset("Packages/com.interactivecamera.interactivecamera-system/Examples/CinemachineCameras/ConversationMode_Camera.prefab", 
                     $"{cameraPrefabsFolder}/ConversationMode_Camera.prefab");
            CopyAsset("Packages/com.interactivecamera.interactivecamera-system/Examples/CinemachineCameras/ExplorationMode_Camera.prefab", 
                     $"{cameraPrefabsFolder}/ExplorationMode_Camera.prefab");
            CopyAsset("Packages/com.interactivecamera.interactivecamera-system/Examples/CinemachineCameras/FollowMode_Camera.prefab", 
                     $"{cameraPrefabsFolder}/FollowMode_Camera.prefab");
            CopyAsset("Packages/com.interactivecamera.interactivecamera-system/Examples/CinemachineCameras/IntroMode_Camera.prefab", 
                     $"{cameraPrefabsFolder}/IntroMode_Camera.prefab");
            
            // Copy camera manager prefab
            CopyAsset("Packages/com.interactivecamera.interactivecamera-system/Examples/CinemachineCameraManager.prefab", 
                     $"{managerFolder}/CinemachineCameraManager.prefab");
            
            AssetDatabase.Refresh();
            
            // Update references in copied CameraMode assets to point to copied prefabs
            UpdateCameraModeReferences(cameraModesFolder, cameraPrefabsFolder);
            
            // Update references in copied manager prefab to point to copied CameraMode assets
            UpdateManagerPrefabReferences(managerFolder, cameraModesFolder);
            
            // Select the created folder
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(baseFolder);
            
            Debug.Log($"✅ InteractiveCameraSystem example assets ready at: {baseFolder}");
            Debug.Log($"💡 Tip: Assets that already existed were skipped to prevent duplicates");
        }
        
        /// <summary>
        /// Updates references in copied CameraMode assets to point to copied camera prefabs instead of package assets
        /// </summary>
        private static void UpdateCameraModeReferences(string cameraModesFolder, string cameraPrefabsFolder)
        {
            // Create mapping from package prefab paths to local prefab paths
            var prefabMapping = new Dictionary<string, string>
            {
                { "Packages/com.interactivecamera.interactivecamera-system/Examples/CinemachineCameras/ConversationMode_Camera.prefab", 
                  $"{cameraPrefabsFolder}/ConversationMode_Camera.prefab" },
                { "Packages/com.interactivecamera.interactivecamera-system/Examples/CinemachineCameras/ExplorationMode_Camera.prefab", 
                  $"{cameraPrefabsFolder}/ExplorationMode_Camera.prefab" },
                { "Packages/com.interactivecamera.interactivecamera-system/Examples/CinemachineCameras/FollowMode_Camera.prefab", 
                  $"{cameraPrefabsFolder}/FollowMode_Camera.prefab" },
                { "Packages/com.interactivecamera.interactivecamera-system/Examples/CinemachineCameras/IntroMode_Camera.prefab", 
                  $"{cameraPrefabsFolder}/IntroMode_Camera.prefab" }
            };
            
            // Find all CameraMode assets in the folder
            string[] cameraModeGuids = AssetDatabase.FindAssets("t:CameraMode", new[] { cameraModesFolder });
            
            foreach (string guid in cameraModeGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                CameraMode mode = AssetDatabase.LoadAssetAtPath<CameraMode>(path);
                
                if (mode == null) continue;
                
                // Check if this mode references a package asset
                if (mode.virtualCamera != null)
                {
                    string prefabPath = AssetDatabase.GetAssetPath(mode.virtualCamera);
                    
                    // Check if it's referencing a package asset
                    if (prefabPath.StartsWith("Packages/"))
                    {
                        // Find the corresponding copied prefab
                        if (prefabMapping.TryGetValue(prefabPath, out string localPrefabPath))
                        {
                            var localPrefab = AssetDatabase.LoadAssetAtPath<Unity.Cinemachine.CinemachineCamera>(localPrefabPath);
                            
                            if (localPrefab != null)
                            {
                                // Update the reference using SerializedObject
                                var serializedObject = new SerializedObject(mode);
                                var virtualCameraProperty = serializedObject.FindProperty("virtualCamera");
                                virtualCameraProperty.objectReferenceValue = localPrefab;
                                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                                
                                // Force update and save
                                EditorUtility.SetDirty(mode);
                                AssetDatabase.SaveAssets();
                                
                                Debug.Log($"✓ Updated reference in {mode.modeName}: {prefabPath} -> local copy");
                            }
                            else
                            {
                                Debug.LogError($"Could not load local prefab from {localPrefabPath}");
                            }
                        }
                    }
                }
            }
            
            AssetDatabase.Refresh();
        }
        
        /// <summary>
        /// Updates references in the copied manager prefab to point to copied CameraMode assets instead of package assets
        /// </summary>
        private static void UpdateManagerPrefabReferences(string managerFolder, string cameraModesFolder)
        {
            // Create mapping from package CameraMode paths to local paths
            var cameraModeMapping = new Dictionary<string, string>
            {
                { "Packages/com.interactivecamera.interactivecamera-system/Examples/CameraModeAssets/ConversationMode.asset", 
                  $"{cameraModesFolder}/ConversationMode.asset" },
                { "Packages/com.interactivecamera.interactivecamera-system/Examples/CameraModeAssets/ExplorationMode.asset", 
                  $"{cameraModesFolder}/ExplorationMode.asset" },
                { "Packages/com.interactivecamera.interactivecamera-system/Examples/CameraModeAssets/FollowMode.asset", 
                  $"{cameraModesFolder}/FollowMode.asset" },
                { "Packages/com.interactivecamera.interactivecamera-system/Examples/CameraModeAssets/IntroMode.asset", 
                  $"{cameraModesFolder}/IntroMode.asset" }
            };
            
            string managerPath = $"{managerFolder}/CinemachineCameraManager.prefab";
            var managerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(managerPath);
            
            if (managerPrefab == null) return;
            
            var managerComponent = managerPrefab.GetComponent<CinemachineCameraManager>();
            if (managerComponent == null) return;
            
            bool needsSave = false;
            
            // Update manager component's defaultMode
            SerializedObject serializedComponent = new SerializedObject(managerComponent);
            SerializedProperty defaultModeProperty = serializedComponent.FindProperty("defaultMode");
            if (defaultModeProperty != null && defaultModeProperty.objectReferenceValue != null)
            {
                string modePath = AssetDatabase.GetAssetPath(defaultModeProperty.objectReferenceValue);
                if (cameraModeMapping.TryGetValue(modePath, out string localModePath))
                {
                    var localMode = AssetDatabase.LoadAssetAtPath<CameraMode>(localModePath);
                    if (localMode != null)
                    {
                        defaultModeProperty.objectReferenceValue = localMode;
                        Debug.Log($"✓ Updated defaultMode in manager: {modePath} -> local copy");
                        needsSave = true;
                    }
                }
            }
            serializedComponent.ApplyModifiedProperties();
            
            // Update IntroSequenceController component if it exists
            var introSeqController = managerPrefab.transform.Find("IntroSequencer")?.GetComponent<IntroSequenceController>();
            if (introSeqController != null)
            {
                SerializedObject serializedIntro = new SerializedObject(introSeqController);
                
                // Update introMode
                var introModeProperty = serializedIntro.FindProperty("introMode");
                if (introModeProperty != null && introModeProperty.objectReferenceValue != null)
                {
                    string modePath = AssetDatabase.GetAssetPath(introModeProperty.objectReferenceValue);
                    if (cameraModeMapping.TryGetValue(modePath, out string localModePath))
                    {
                        var localMode = AssetDatabase.LoadAssetAtPath<CameraMode>(localModePath);
                        if (localMode != null)
                        {
                            introModeProperty.objectReferenceValue = localMode;
                            Debug.Log($"✓ Updated introMode in IntroSequenceController: {modePath} -> local copy");
                            needsSave = true;
                        }
                    }
                }
                
                // Update gameplayMode
                var gameplayModeProperty = serializedIntro.FindProperty("gameplayMode");
                if (gameplayModeProperty != null && gameplayModeProperty.objectReferenceValue != null)
                {
                    string modePath = AssetDatabase.GetAssetPath(gameplayModeProperty.objectReferenceValue);
                    if (cameraModeMapping.TryGetValue(modePath, out string localModePath))
                    {
                        var localMode = AssetDatabase.LoadAssetAtPath<CameraMode>(localModePath);
                        if (localMode != null)
                        {
                            gameplayModeProperty.objectReferenceValue = localMode;
                            Debug.Log($"✓ Updated gameplayMode in IntroSequenceController: {modePath} -> local copy");
                            needsSave = true;
                        }
                    }
                }
                
                serializedIntro.ApplyModifiedProperties();
            }
            
            if (needsSave)
            {
                EditorUtility.SetDirty(managerComponent);
                EditorUtility.SetDirty(managerPrefab);
                AssetDatabase.SaveAssets();
            }
            
            AssetDatabase.Refresh();
        }
        
        /// <summary>
        /// Creates a folder if it doesn't exist
        /// </summary>
        private static void CreateFolderIfNotExists(string folderPath)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string parentFolder = System.IO.Path.GetDirectoryName(folderPath).Replace('\\', '/');
                string folderName = System.IO.Path.GetFileName(folderPath);
                AssetDatabase.CreateFolder(parentFolder, folderName);
            }
        }
        
        /// <summary>
        /// Copies an asset from source to destination
        /// </summary>
        private static void CopyAsset(string sourcePath, string destinationPath)
        {
            // Check if source exists
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(sourcePath) == null)
            {
                Debug.LogWarning($"⚠️ Source asset not found: {sourcePath}");
                return;
            }
            
            // Check if destination already exists
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(destinationPath) != null)
            {
                Debug.Log($"✓ Asset already exists, skipping: {destinationPath}");
                return;
            }
            
            // Copy the asset
            AssetDatabase.CopyAsset(sourcePath, destinationPath);
        }
        
        /// <summary>
        /// Gets the currently selected folder path, or returns "Assets" if none selected
        /// </summary>
        private static string GetSelectedFolderPath()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            
            if (string.IsNullOrEmpty(path))
            {
                return "Assets";
            }
            
            if (AssetDatabase.IsValidFolder(path))
            {
                return path;
            }
            
            // If a file is selected, get its parent folder
            return System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
        }
    }
}

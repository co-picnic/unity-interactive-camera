using UnityEngine;
using UnityEditor;
using InteractiveCameraSystem;
using Unity.Cinemachine;

namespace InteractiveCameraSystem.Examples.Editor
{
    /// <summary>
    /// Editor window for setting up the InteractiveCameraSystem in a scene
    /// Access via: Tools > InteractiveCameraSystem > Setup Camera System
    /// </summary>
    public class CameraSystemSetupWindow : EditorWindow
    {
        private bool createEventSystem = true;
        private bool createCinemachineBrain = true;
        private bool createCameraManager = true;
        private bool createInputManager = true;
        private bool createFingerManager = true;
        private bool createExampleAnchors = true;
        private int anchorCount = 3;
        
        private float fieldOfView = 60f;
        private float nearClipPlane = 0.1f;
        private float farClipPlane = 1000f;
        
        [MenuItem("Tools/InteractiveCameraSystem/Setup Camera System")]
        public static void ShowWindow()
        {
            GetWindow<CameraSystemSetupWindow>("Camera System Setup");
        }
        
        void OnGUI()
        {
            GUILayout.Label("Camera System Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            GUILayout.Label("This tool will automatically create all necessary components for the InteractiveCameraSystem.");
            GUILayout.Space(10);
            
            // Component selection
            GUILayout.Label("Components to Create:", EditorStyles.boldLabel);
            createEventSystem = EditorGUILayout.Toggle("Event System (Required for Lean Touch)", createEventSystem);
            createCinemachineBrain = EditorGUILayout.Toggle("Cinemachine Brain", createCinemachineBrain);
            createCameraManager = EditorGUILayout.Toggle("Camera Manager", createCameraManager);
            createInputManager = EditorGUILayout.Toggle("Input Manager", createInputManager);
            createFingerManager = EditorGUILayout.Toggle("Finger Manager", createFingerManager);
            createExampleAnchors = EditorGUILayout.Toggle("Example Anchors", createExampleAnchors);
            
            if (createExampleAnchors)
            {
                EditorGUI.indentLevel++;
                anchorCount = EditorGUILayout.IntSlider("Number of Anchors", anchorCount, 1, 10);
                EditorGUI.indentLevel--;
            }
            
            GUILayout.Space(10);
            
            // Camera settings
            GUILayout.Label("Camera Settings:", EditorStyles.boldLabel);
            fieldOfView = EditorGUILayout.Slider("Field of View", fieldOfView, 10f, 120f);
            nearClipPlane = EditorGUILayout.Slider("Near Clip Plane", nearClipPlane, 0.01f, 10f);
            farClipPlane = EditorGUILayout.Slider("Far Clip Plane", farClipPlane, 100f, 10000f);
            
            GUILayout.Space(20);
            
            // Setup button
            if (GUILayout.Button("Setup Camera System", GUILayout.Height(30)))
            {
                SetupCameraSystem();
            }
            
            GUILayout.Space(10);
            
            // Cleanup button
            if (GUILayout.Button("Clean Up Camera System", GUILayout.Height(25)))
            {
                CleanUpCameraSystem();
            }
            
            GUILayout.Space(10);
            
            // Help text
            EditorGUILayout.HelpBox(
                "This will create all necessary GameObjects and components in the current scene. " +
                "Make sure you have a scene open before running the setup.",
                MessageType.Info
            );
        }
        
        void SetupCameraSystem()
        {
            Debug.Log("=== Setting up InteractiveCameraSystem ===");
            
            // Create parent GameObject for organization
            GameObject parentGO = new GameObject("CinemachineCamera");
            Debug.Log("✅ Created parent GameObject: CinemachineCamera");
            
            if (createEventSystem)
            {
                CreateEventSystem(parentGO);
            }
            
            if (createCinemachineBrain)
            {
                CreateCinemachineBrain(parentGO);
            }
            
            if (createCameraManager)
            {
                CreateCameraManager(parentGO);
            }
            
            if (createInputManager)
            {
                CreateInputManager(parentGO);
            }
            
            if (createFingerManager)
            {
                CreateFingerManager(parentGO);
            }
            
            if (createExampleAnchors)
            {
                CreateExampleAnchors(parentGO);
            }
            
            Debug.Log("=== Camera System Setup Complete ===");
            
            // Show completion message
            EditorUtility.DisplayDialog(
                "Setup Complete", 
                "Camera system has been set up successfully!\n\nAll components are organized under the 'CinemachineCamera' GameObject in the Hierarchy.",
                "OK"
            );
        }
        
        void CreateEventSystem(GameObject parent)
        {
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null)
            {
                Debug.Log("Event System already exists, skipping creation.");
                return;
            }
            
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.transform.SetParent(parent.transform);
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            
            Debug.Log("✅ Created Event System");
        }
        
        void CreateCinemachineBrain(GameObject parent)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraGO = new GameObject("Main Camera");
                cameraGO.tag = "MainCamera";
                cameraGO.transform.SetParent(parent.transform);
                mainCamera = cameraGO.AddComponent<Camera>();
            }
            
            if (mainCamera.GetComponent<CinemachineBrain>() == null)
            {
                mainCamera.gameObject.AddComponent<CinemachineBrain>();
                
                // Configure camera
                mainCamera.fieldOfView = fieldOfView;
                mainCamera.nearClipPlane = nearClipPlane;
                mainCamera.farClipPlane = farClipPlane;
                
                Debug.Log("✅ Created Cinemachine Brain");
            }
            else
            {
                Debug.Log("Cinemachine Brain already exists, skipping creation.");
            }
        }
        
        void CreateCameraManager(GameObject parent)
        {
            if (FindFirstObjectByType<CinemachineCameraManager>() != null)
            {
                Debug.Log("Camera Manager already exists, skipping creation.");
                return;
            }
            
            GameObject managerGO = new GameObject("CinemachineCameraManager");
            managerGO.transform.SetParent(parent.transform);
            managerGO.AddComponent<CinemachineCameraManager>();
            
            Debug.Log("✅ Created Camera Manager");
        }
        
        void CreateInputManager(GameObject parent)
        {
            if (FindFirstObjectByType<CinemachineInputManager>() != null)
            {
                Debug.Log("Input Manager already exists, skipping creation.");
                return;
            }
            
            GameObject inputGO = new GameObject("CinemachineInputManager");
            inputGO.transform.SetParent(parent.transform);
            inputGO.AddComponent<CinemachineInputManager>();
            
            Debug.Log("✅ Created Input Manager");
        }
        
        void CreateFingerManager(GameObject parent)
        {
            if (FindFirstObjectByType<FingerManager>() != null)
            {
                Debug.Log("Finger Manager already exists, skipping creation.");
                return;
            }
            
            GameObject fingerGO = new GameObject("FingerManager");
            fingerGO.transform.SetParent(parent.transform);
            fingerGO.AddComponent<FingerManager>();
            
            Debug.Log("✅ Created Finger Manager");
        }
        
        void CreateExampleAnchors(GameObject parent)
        {
            GameObject anchorParent = new GameObject("Camera Anchors");
            anchorParent.transform.SetParent(parent.transform);
            
            for (int i = 0; i < anchorCount; i++)
            {
                GameObject anchorGO = new GameObject($"Anchor_{i + 1}");
                anchorGO.transform.SetParent(anchorParent.transform);
                anchorGO.transform.position = new Vector3(i * 5f, 0f, 0f);
                
                VirtualCameraAnchor anchor = anchorGO.AddComponent<VirtualCameraAnchor>();
                // The anchor name is automatically set to the GameObject name
                // anchor.Identifier will return gameObject.name
                
                Debug.Log($"✅ Created Anchor_{i + 1}");
            }
        }
        
        void CleanUpCameraSystem()
        {
            Debug.Log("=== Cleaning up Camera System ===");
            
            // Find and remove the parent GameObject (this will remove all children)
            GameObject parentGO = GameObject.Find("CinemachineCamera");
            if (parentGO != null)
            {
                DestroyImmediate(parentGO);
                Debug.Log("✅ Removed CinemachineCamera parent and all children");
            }
            else
            {
                // Fallback: remove components individually if parent not found
                var cameraManager = FindFirstObjectByType<CinemachineCameraManager>();
                if (cameraManager != null)
                {
                    DestroyImmediate(cameraManager.gameObject);
                    Debug.Log("✅ Removed Camera Manager");
                }
                
                var inputManager = FindFirstObjectByType<CinemachineInputManager>();
                if (inputManager != null)
                {
                    DestroyImmediate(inputManager.gameObject);
                    Debug.Log("✅ Removed Input Manager");
                }
                
                var fingerManager = FindFirstObjectByType<FingerManager>();
                if (fingerManager != null)
                {
                    DestroyImmediate(fingerManager.gameObject);
                    Debug.Log("✅ Removed Finger Manager");
                }
                
                var anchors = FindObjectsOfType<VirtualCameraAnchor>();
                foreach (var anchor in anchors)
                {
                    DestroyImmediate(anchor.gameObject);
                }
                Debug.Log($"✅ Removed {anchors.Length} Anchors");
            }
            
            Debug.Log("=== Cleanup Complete ===");
            
            // Show completion message
            EditorUtility.DisplayDialog(
                "Cleanup Complete", 
                "Camera system has been cleaned up successfully!\n\nAll components have been removed from the scene.",
                "OK"
            );
        }
    }
}

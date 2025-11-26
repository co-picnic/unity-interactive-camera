using UnityEngine;
using UnityEditor;
using InteractiveCameraSystem;

namespace InteractiveCameraSystem.Examples.Editor
{
    /// <summary>
    /// Helper script to create CameraMode assets with proper references
    /// </summary>
    public class CameraModeAssetCreator : EditorWindow
    {

        void OnGUI()
        {
            GUILayout.Label("CameraMode Asset Creator", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.Label("This tool creates CameraMode assets that can be assigned to IntroSequenceController.");
            GUILayout.Space(10);

            if (GUILayout.Button("Create IntroMode Asset"))
            {
                CreateIntroModeAsset();
            }

            if (GUILayout.Button("Create GameplayMode Asset"))
            {
                CreateGameplayModeAsset();
            }

            GUILayout.Space(10);
            GUILayout.Label("Instructions:", EditorStyles.boldLabel);
            GUILayout.Label("1. Click the buttons above to create the assets");
            GUILayout.Label("2. The assets will be created in Examples/CameraModeAssets/");
            GUILayout.Label("3. Assign your CinemachineCamera to the virtualCamera field");
            GUILayout.Label("4. Use the assets with IntroSequenceController");
        }

        void CreateIntroModeAsset()
        {
            var asset = ScriptableObject.CreateInstance<CameraMode>();
            asset.modeName = "IntroMode";
            asset.description = "Camera mode for intro sequences";
            asset.enableFollow = false;
            asset.enableZoom = false;
            asset.enableDrag = false;
            asset.priority = 5;

            string path = "Assets/Plugins/InteractiveCameraSystem/Examples/CameraModeAssets/IntroMode.asset";
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Created IntroMode asset at: {path}");
            EditorUtility.DisplayDialog("Success", "IntroMode asset created successfully!", "OK");
        }

        void CreateGameplayModeAsset()
        {
            var asset = ScriptableObject.CreateInstance<CameraMode>();
            asset.modeName = "GameplayMode";
            asset.description = "Camera mode for gameplay";
            asset.enableFollow = true;
            asset.enableZoom = true;
            asset.enableDrag = true;
            asset.priority = 50;

            string path = "Assets/Plugins/InteractiveCameraSystem/Examples/CameraModeAssets/GameplayMode.asset";
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Created GameplayMode asset at: {path}");
            EditorUtility.DisplayDialog("Success", "GameplayMode asset created successfully!", "OK");
        }
    }
}

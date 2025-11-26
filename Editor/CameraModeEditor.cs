using UnityEngine;
using UnityEditor;
using InteractiveCameraSystem;

namespace InteractiveCameraSystem.Editor
{
    /// <summary>
    /// Simple custom editor for CameraMode that conditionally shows settings based on behavior flags
    /// </summary>
    [CustomEditor(typeof(CameraMode))]
    public class CameraModeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // Always show basic info
            EditorGUILayout.PropertyField(serializedObject.FindProperty("modeName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("virtualCamera"));
            
            EditorGUILayout.Space();
            
            // Always show camera settings
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraSettings"), true);
            
            EditorGUILayout.Space();
            
            // Show behavior flags
            EditorGUILayout.LabelField("Behavior Flags", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enableFollow"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enableZoom"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enableDrag"));
            
            EditorGUILayout.Space();
            
            // Conditionally show follow settings
            if (serializedObject.FindProperty("enableFollow").boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("followSettings"), true);
                EditorGUILayout.Space();
            }
            
            // Conditionally show drag settings
            if (serializedObject.FindProperty("enableDrag").boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("dragSettings"), true);
                EditorGUILayout.Space();
            }
            
            // Conditionally show zoom settings
            if (serializedObject.FindProperty("enableZoom").boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("zoomSettings"), true);
                EditorGUILayout.Space();
            }
            
            // Always show transition settings
            EditorGUILayout.PropertyField(serializedObject.FindProperty("transitionSettings"), true);
            
            EditorGUILayout.Space();
            
            // Show advanced settings
            EditorGUILayout.LabelField("Advanced", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("priority"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isEnabled"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tags"), true);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}



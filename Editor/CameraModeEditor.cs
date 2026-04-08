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
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoPopulateTargets"), 
                new GUIContent("Auto Populate Targets", "Auto-populate target group from SetCameraTarget components on startup. Uncheck for modes where targets are set programmatically (e.g. conversation)."));
            
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
            
            // Transition settings — draw manually so customCurve only shows for Custom style
            var transProp = serializedObject.FindProperty("transitionSettings");
            EditorGUILayout.LabelField("Transition Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(transProp.FindPropertyRelative("blendDuration"));
            EditorGUILayout.PropertyField(transProp.FindPropertyRelative("blendDelay"));
            EditorGUILayout.PropertyField(transProp.FindPropertyRelative("blendStyle"), new GUIContent("Blend Curve"));
            
            var styleProp = transProp.FindPropertyRelative("blendStyle");
            if (styleProp.enumValueIndex == (int)Unity.Cinemachine.CinemachineBlendDefinition.Styles.Custom)
            {
                EditorGUILayout.PropertyField(transProp.FindPropertyRelative("customCurve"), new GUIContent("Custom Curve"));
            }
            
            EditorGUILayout.Space(4);
            EditorGUILayout.PropertyField(transProp.FindPropertyRelative("smoothPosition"));
            EditorGUILayout.PropertyField(transProp.FindPropertyRelative("smoothRotation"));
            EditorGUILayout.PropertyField(transProp.FindPropertyRelative("smoothFieldOfView"));
            
            EditorGUILayout.Space(4);
            EditorGUILayout.PropertyField(transProp.FindPropertyRelative("lockToGroundDuringTransition"));
            if (transProp.FindPropertyRelative("lockToGroundDuringTransition").boolValue)
            {
                EditorGUILayout.PropertyField(transProp.FindPropertyRelative("transitionGroundLevel"));
            }
            EditorGUILayout.PropertyField(transProp.FindPropertyRelative("maxTransitionSpeed"));
            EditorGUI.indentLevel--;
            
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



using UnityEngine;
using UnityEditor;
using System;

namespace InteractiveCameraSystem.Editor
{
    /// <summary>
    /// GUI utilities for VirtualCamera editor with Unity Volume-like styling
    /// </summary>
    public static class VirtualCameraEditorGUI
    {
        private static readonly float StandardSpacing = EditorGUIUtility.standardVerticalSpacing;
        private static readonly float SectionSpacing = 6f;
        private static readonly float SubSectionSpacing = 2f;
        private static readonly float PropertySpacing = 2f;
        
        // New styling constants for modern profile-like appearance
        private static readonly float ProfileSpacing = 8f;
        private static readonly float ProfileHeaderHeight = 22f;
        private static readonly float ProfileContentPadding = 4f;
        
        // Style colors (compatible with both light and dark themes)
        private static Color ProfileBackgroundColor => EditorGUIUtility.isProSkin 
            ? new Color(0.2f, 0.2f, 0.2f, 1f)  // Dark theme: darker gray
            : new Color(0.85f, 0.85f, 0.85f, 1f); // Light theme: light gray
            
        private static Color ProfileHeaderColor => EditorGUIUtility.isProSkin 
            ? new Color(0.25f, 0.25f, 0.25f, 1f)  // Dark theme: slightly lighter
            : new Color(0.8f, 0.8f, 0.8f, 1f);    // Light theme: slightly darker
            
        private static Color ProfileBorderColor => EditorGUIUtility.isProSkin 
            ? new Color(0.1f, 0.1f, 0.1f, 1f)     // Dark theme: very dark
            : new Color(0.6f, 0.6f, 0.6f, 1f);    // Light theme: medium gray
        
        /// <summary>
        /// Draw a header for the inspector
        /// </summary>
        public static void DrawHeader(string title)
        {
            EditorGUILayout.Space(SectionSpacing);
            var headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                padding = new RectOffset(0, 0, 4, 4)
            };
            EditorGUILayout.LabelField(title, headerStyle);
            DrawHorizontalLine();
            EditorGUILayout.Space(SectionSpacing);
        }
        
        /// <summary>
        /// Draw a boxed section with Volume-like styling
        /// </summary>
        public static void DrawBoxedSection(string title, Action content, bool defaultExpanded = true)
        {
            var foldoutKey = $"VirtualCamera_BoxedSection_{title.Replace(" ", "_")}";
            var isExpanded = EditorPrefs.GetBool(foldoutKey, defaultExpanded);
            
            EditorGUILayout.Space(PropertySpacing);
            
            // Create a subtle background style similar to Unity Volume
            var backgroundStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(8, 8, 4, 4),
                margin = new RectOffset(0, 0, 2, 2)
            };
            
            // Draw the section with subtle background
            EditorGUILayout.BeginVertical(backgroundStyle);
            
            // Section header with cleaner foldout
            var headerStyle = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Normal,
                fontSize = 11,
                margin = new RectOffset(0, 0, 2, 2)
            };
            
            isExpanded = EditorGUILayout.Foldout(isExpanded, title, headerStyle);
            EditorPrefs.SetBool(foldoutKey, isExpanded);
            
            if (isExpanded)
            {
                EditorGUILayout.Space(PropertySpacing);
                content?.Invoke();
                EditorGUILayout.Space(PropertySpacing);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// Draw a sub-section within a boxed section
        /// </summary>
        public static void DrawSubSection(string title, Action content)
        {
            if (!string.IsNullOrEmpty(title))
            {
                EditorGUILayout.Space(PropertySpacing);
                var subHeaderStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 10,
                    fontStyle = FontStyle.Bold,
                    normal = { 
                        textColor = EditorGUIUtility.isProSkin 
                            ? new Color(0.8f, 0.8f, 0.8f, 1f) 
                            : new Color(0.3f, 0.3f, 0.3f, 1f) 
                    }
                };
                EditorGUILayout.LabelField(title, subHeaderStyle);
                EditorGUILayout.Space(PropertySpacing);
            }
            
            if (content != null)
            {
                content.Invoke();
                EditorGUILayout.Space(SubSectionSpacing);
            }
        }
        
        /// <summary>
        /// Draw a property group with consistent spacing
        /// </summary>
        public static void DrawPropertyGroup(Action content)
        {
            EditorGUILayout.BeginVertical();
            content?.Invoke();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(StandardSpacing);
        }
        
        /// <summary>
        /// Draw a foldout header (legacy support)
        /// </summary>
        public static bool DrawFoldoutHeader(string title, bool isExpanded)
        {
            return EditorGUILayout.Foldout(isExpanded, title, EditorStyles.foldoutHeader);
        }
        
        /// <summary>
        /// Draw a horizontal line separator
        /// </summary>
        public static void DrawHorizontalLine()
        {
            var rect = EditorGUILayout.GetControlRect(false, 1);
            var lineColor = EditorGUIUtility.isProSkin 
                ? new Color(0.3f, 0.3f, 0.3f, 0.6f) 
                : new Color(0.7f, 0.7f, 0.7f, 0.6f);
            EditorGUI.DrawRect(rect, lineColor);
        }
        
        /// <summary>
        /// Draw a help box with consistent styling
        /// </summary>
        public static void DrawHelpBox(string message, MessageType messageType = MessageType.Info)
        {
            var helpBoxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                fontSize = 10,
                padding = new RectOffset(8, 8, 4, 4),
                margin = new RectOffset(0, 0, (int)PropertySpacing, (int)PropertySpacing)
            };
            
            EditorGUILayout.LabelField(message, helpBoxStyle);
        }
        
        /// <summary>
        /// Create a disabled scope for read-only properties
        /// </summary>
        public static EditorGUI.DisabledScope CreateDisabledScope(bool disabled = true)
        {
            return new EditorGUI.DisabledScope(disabled);
        }
        
        /// <summary>
        /// Draw a profile-style foldout exactly like Unity's Volume Profile system
        /// Uses a separate background rect to achieve full width without affecting controls.
        /// </summary>
        /// <param name="title">The title to display in the header</param>
        /// <param name="isExpanded">Current expanded state</param>
        /// <param name="content">Action to draw the content when expanded</param>
        /// <param name="headerActions">Optional action to draw additional controls in the header (like toggles)</param>
        /// <param name="menuAction">Optional action for context menu (three dots icon)</param>
        /// <returns>New expanded state</returns>
        public static bool DrawProfileFoldout(string title, bool isExpanded, Action content, 
            Action headerActions = null, Action menuAction = null)
        {
            DrawSplitter();

            // Get the rect for the header
            var headerRect = GUILayoutUtility.GetRect(16f, 22f, GUILayout.ExpandWidth(true));

            // Background rect spans the full inspector width
            var bgRect = new Rect(0, headerRect.y, EditorGUIUtility.currentViewWidth - 1, headerRect.height);
            EditorGUI.DrawRect(bgRect, ProfileBackgroundColor);

            // Reserve space for icons on the right
            float menuWidth = 0f;
            if (menuAction != null) menuWidth += 20f;
            if (headerActions != null) menuWidth += 24f;

            // The foldout occupies the useful area, without manual offsets
            var foldoutRect = new Rect(headerRect.x, headerRect.y, headerRect.width - menuWidth, headerRect.height);

            // Draw foldout with custom style exactly like Volume Profile
            var foldoutStyle = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 12,
                normal = { textColor = EditorGUIUtility.isProSkin ? new Color(0.85f, 0.85f, 0.85f) : Color.black }
            };

            var newExpanded = EditorGUI.Foldout(foldoutRect, isExpanded, title, true, foldoutStyle);

            // Draw header actions (like enable/disable toggles)
            if (headerActions != null)
            {
                var actionsRect = new Rect(headerRect.xMax - menuWidth + 4f, headerRect.y + 2f, 20f, headerRect.height - 4f);
                GUILayout.BeginArea(actionsRect);
                headerActions();
                GUILayout.EndArea();
            }

            // Draw three dots menu button, aligned to the right of the interactive area
            if (menuAction != null)
            {
                var menuRect = new Rect(headerRect.xMax - 20f, headerRect.y + 2f, 16f, headerRect.height - 4f);
                
                var menuButtonStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    normal = { 
                        textColor = EditorGUIUtility.isProSkin 
                            ? new Color(0.7f, 0.7f, 0.7f, 1f) 
                            : new Color(0.4f, 0.4f, 0.4f, 1f) 
                    },
                    hover = { 
                        textColor = EditorGUIUtility.isProSkin 
                            ? new Color(1f, 1f, 1f, 1f) 
                            : new Color(0.2f, 0.2f, 0.2f, 1f) 
                    }
                };
                
                if (GUI.Button(menuRect, "⋮", menuButtonStyle))
                {
                    menuAction();
                }
            }

            // Draw content if expanded with proper lateral padding
            if (newExpanded && content != null)
            {
                DrawProfileContentWithCorrectPadding(content);
            }

            return newExpanded;
        }
        
        /// <summary>
        /// Draw a splitter line like Volume Profile uses between sections
        /// Uses full inspector width and minimal spacing to avoid pixel gaps
        /// </summary>
        public static void DrawSplitter()
        {
            EditorGUILayout.Space(0.5f); // Minimal spacing to avoid pixel gaps
            var splitterRect = GUILayoutUtility.GetRect(1f, 1f, GUILayout.ExpandWidth(true));
            
            // Expand splitter to full inspector width
            splitterRect.xMin = 0;
            splitterRect.xMax = EditorGUIUtility.currentViewWidth;
            
            EditorGUI.DrawRect(splitterRect, ProfileBorderColor);
            // No space after splitter to prevent gaps
        }
        
        /// <summary>
        /// Draw content with correct padding like Volume Profile
        /// </summary>
        private static void DrawProfileContentWithCorrectPadding(Action content)
        {
            EditorGUILayout.BeginVertical();
            content();
            GUILayout.Space(4f); // Small bottom padding to separate from next section
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// Draw content inside a profile foldout with modern Volume Profile styling
        /// Uses default background (no custom coloring) for cleaner appearance
        /// </summary>
        private static void DrawProfileContentModern(Action content)
        {
            EditorGUILayout.Space(ProfileContentPadding);
            
            // Indent content slightly for hierarchy
            EditorGUI.indentLevel++;
            content();
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space(ProfileContentPadding);
        }
        
        /// <summary>
        /// Draw content inside a profile foldout with proper styling (legacy method)
        /// </summary>
        private static void DrawProfileContent(Action content)
        {
            var contentRect = EditorGUILayout.BeginVertical();
            
            // Draw content background
            var backgroundRect = new Rect(contentRect.x, contentRect.y - ProfileContentPadding, 
                contentRect.width, contentRect.height + ProfileContentPadding * 2f);
            EditorGUI.DrawRect(backgroundRect, ProfileBackgroundColor);
            
            // Draw side borders
            var leftBorder = new Rect(backgroundRect.x, backgroundRect.y, 1f, backgroundRect.height);
            var rightBorder = new Rect(backgroundRect.xMax - 1f, backgroundRect.y, 1f, backgroundRect.height);
            EditorGUI.DrawRect(leftBorder, ProfileBorderColor);
            EditorGUI.DrawRect(rightBorder, ProfileBorderColor);
            
            // Draw bottom border
            var bottomBorder = new Rect(backgroundRect.x, backgroundRect.yMax - 1f, backgroundRect.width, 1f);
            EditorGUI.DrawRect(bottomBorder, ProfileBorderColor);
            
            EditorGUILayout.Space(ProfileContentPadding);
            
            // Indent content slightly
            EditorGUI.indentLevel++;
            content();
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space(ProfileContentPadding);
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// Draw a simple profile-style foldout without header actions
        /// </summary>
        public static bool DrawSimpleProfileFoldout(string title, bool isExpanded, Action content)
        {
            return DrawProfileFoldout(title, isExpanded, content);
        }
        
        /// <summary>
        /// Draw an "Add" button with profile styling
        /// </summary>
        public static bool DrawAddButton(string text, float? width = null)
        {
            EditorGUILayout.Space(ProfileSpacing * 0.5f);
            
            var buttonHeight = 24f;
            var buttonRect = EditorGUILayout.GetControlRect(false, buttonHeight);
            
            if (width.HasValue)
            {
                var centeredRect = new Rect(
                    buttonRect.x + (buttonRect.width - width.Value) * 0.5f,
                    buttonRect.y,
                    width.Value,
                    buttonRect.height);
                buttonRect = centeredRect;
            }
            
            var buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 11;
            buttonStyle.fontStyle = FontStyle.Bold;
            
            var result = GUI.Button(buttonRect, text, buttonStyle);
            
            EditorGUILayout.Space(ProfileSpacing * 0.5f);
            return result;
        }
        
        /// <summary>
        /// Create a context menu for profile items
        /// </summary>
        public static void ShowProfileContextMenu(Action onRemove = null, Action onReset = null, Action onDuplicate = null)
        {
            var menu = new GenericMenu();
            
            if (onReset != null)
            {
                menu.AddItem(new GUIContent("Reset to Defaults"), false, () => onReset());
                menu.AddSeparator("");
            }
            
            if (onDuplicate != null)
            {
                menu.AddItem(new GUIContent("Duplicate"), false, () => onDuplicate());
            }
            
            if (onRemove != null)
            {
                if (menu.GetItemCount() > 0)
                    menu.AddSeparator("");
                menu.AddItem(new GUIContent("Remove"), false, () => onRemove());
            }
            
            menu.ShowAsContext();
        }
    }
}

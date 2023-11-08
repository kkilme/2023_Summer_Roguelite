using Kinemation.FPSFramework.Runtime.Core.Components;
using UnityEditor;
using UnityEngine;

namespace Kinemation.FPSFramework.Editor.FPSAnimator
{
    public class FPSAnimatorEditorWindow : EditorWindow
    {
        private GameObject targetObject;
        private Component targetComponent;
        private UnityEditor.Editor targetComponentEditor;
        
        private const float PADDING = 5f;

        [MenuItem("Window/FPS Animation Framework/FPS Animator Editor")]
        public static void ShowWindow()
        {
            GetWindow<FPSAnimatorEditorWindow>("FPSAnimator Editor");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("FPSAnimator Editor", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            targetObject = (GameObject)EditorGUILayout.ObjectField("Character", targetObject, typeof(GameObject), 
                true);

            if (GUILayout.Button("Refresh Editor"))
            {
                FindTargetComponent();
            }
            
            EditorGUILayout.EndHorizontal();

            if (targetComponent != null)
            {
                DrawTargetComponent();
            }
        }

        private void FindTargetComponent()
        {
            if (targetObject == null)
            {
                Debug.LogWarning("Target Object is null. Cannot find target component.");
                return;
            }
        
            targetComponent = targetObject.GetComponentInChildren<CoreAnimComponent>();

            if (targetComponent == null)
            {
                Debug.LogWarning("Target component of type 'YourComponentType' not found.");
            }
            else
            {
                if (targetComponentEditor != null)
                {
                    DestroyImmediate(targetComponentEditor);
                }
                targetComponentEditor = UnityEditor.Editor.CreateEditor(targetComponent);
            }
        }

        private Vector2 _scrollPosition;

        private void DrawTargetComponent()
        {
            if (targetComponentEditor != null)
            {
                // Vertical padding
                EditorGUILayout.Space(PADDING);
            
                // Horizontal padding
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(PADDING);
                EditorGUILayout.BeginVertical();
                
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
                targetComponentEditor.OnInspectorGUI();
                EditorGUILayout.EndScrollView();
                
                EditorGUILayout.EndVertical();
                GUILayout.Space(PADDING);
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
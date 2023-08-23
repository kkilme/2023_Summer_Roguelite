// Designed by KINEMATION, 2023

using System.IO;
using System.Reflection;
using Kinemation.FPSFramework.Runtime.Core.Types;
using Kinemation.FPSFramework.Runtime.FPSAnimator;
using UnityEditor;
using UnityEngine;

namespace Kinemation.FPSFramework.Editor.FPSAnimator
{
    [CustomEditor(typeof(FPSAnimWeapon), true)]
    [CanEditMultipleObjects]
    public class FPSAnimWeaponEditor : UnityEditor.Editor
    {
        private FPSAnimWeapon owner;
        private bool showAbstractProperties;

        private void OnEnable()
        {
            owner = (FPSAnimWeapon) target;
            // Load the foldout state from EditorPrefs
            showAbstractProperties = EditorPrefs.GetBool("MyAbstractClassEditor_showAbstractProperties", false);
        }

        private void OnDisable()
        {
            // Save the foldout state to EditorPrefs
            EditorPrefs.SetBool("MyAbstractClassEditor_showAbstractProperties", showAbstractProperties);
        }

        // Updates the weapon data to the latest API
        private void UpdateToTheLatestAPI()
        {
            var path = AssetDatabase.GetAssetPath(owner.gameObject);
            path = Path.GetDirectoryName(path);
            string fileName = "WAS_" + owner.gameObject.name;
            string fullPath = Path.Combine(path, fileName + ".asset");

            // Generate unique path
            fullPath = AssetDatabase.GenerateUniqueAssetPath(fullPath);

            var asset = ScriptableObject.CreateInstance<WeaponAnimAsset>();

            owner.weaponTransformData.aimPoint = owner.weaponAnimData.gunAimData.aimPoint;
            owner.weaponTransformData.pivotPoint = owner.weaponAnimData.gunAimData.pivotPoint;
            
            asset.rotationOffset = owner.weaponAnimData.rotationOffset;
            
            asset.adsData.target = owner.weaponAnimData.gunAimData.target;
            asset.adsData.aimSpeed = owner.weaponAnimData.gunAimData.aimSpeed;
            asset.adsData.changeSightSpeed = owner.weaponAnimData.gunAimData.changeSightSpeed;
            asset.adsData.pointAimOffset = owner.weaponAnimData.gunAimData.pointAimOffset;
            asset.adsData.pointAimSpeed = owner.weaponAnimData.gunAimData.pointAimSpeed;
            
            asset.viewOffset = owner.weaponAnimData.viewOffset;
            asset.springData = owner.weaponAnimData.springData;
            asset.moveSwayData = owner.weaponAnimData.moveSwayData;
            asset.blockData = owner.weaponAnimData.blockData;
            
            AssetDatabase.CreateAsset(asset, fullPath);
            owner.weaponAsset = asset;
            
            if (PrefabUtility.IsPartOfPrefabInstance(owner.gameObject))
            {
                PrefabUtility.ApplyPrefabInstance(owner.gameObject, InteractionMode.AutomatedAction);
            }
        }

        public override void OnInspectorGUI()
        {
            // Draw the foldout header for the abstract class properties
            
            GUIStyle foldoutHeaderStyle = new GUIStyle(EditorStyles.foldout);
            foldoutHeaderStyle.fontStyle = FontStyle.Bold;
            foldoutHeaderStyle.fontSize = 12;
            
            GUIContent foldoutHeaderContent = new GUIContent("FPSAnimWeapon Interface");
            Color previousColor = GUI.color;
            GUI.color = showAbstractProperties ? new Color(0.8f, 0.8f, 0.0f, 1.0f) : Color.yellow;
            GUI.color = previousColor;

            GUIStyle buttonStyle = new GUIStyle(EditorStyles.toolbarButton);
            buttonStyle.fontStyle = FontStyle.Bold;
            if (GUILayout.Button(foldoutHeaderContent, buttonStyle))
            {
                showAbstractProperties = !showAbstractProperties;
            }

            if (showAbstractProperties)
            {
                // Draw a colored box to highlight the abstract class fields
                EditorGUILayout.BeginVertical(GUI.skin.box);

                // Draw the abstract class fields using reflection
                BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
                FieldInfo[] fields = target.GetType().BaseType.GetFields(bindingFlags);

                foreach (FieldInfo field in fields)
                {
                    SerializedProperty property = serializedObject.FindProperty(field.Name);
                    if (property != null)
                    {
                        EditorGUILayout.PropertyField(property,
                            new GUIContent(ObjectNames.NicifyVariableName(field.Name)));
                    }
                }
                
                GUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Setup Weapon"))
                {
                    owner.SetupWeapon();

                    if (PrefabUtility.IsPartOfPrefabInstance(owner.gameObject))
                    {
                        PrefabUtility.ApplyPrefabInstance(owner.gameObject, InteractionMode.AutomatedAction);
                    }
                }
                
                if (GUILayout.Button("Generate Anim Asset"))
                {
                    UpdateToTheLatestAPI();
                }
                
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Save Weapon Position"))
                {
                    owner.SavePose();
                    
                    if (PrefabUtility.IsPartOfPrefabInstance(owner.gameObject))
                    {
                        // Apply the changes to the Prefab asset
                        PrefabUtility.ApplyPrefabInstance(owner.gameObject, InteractionMode.AutomatedAction);
                    }
                }

                // Reset the background color
                EditorGUILayout.EndVertical();
            }

            // Get all the abstract class field names to exclude them from the default inspector
            BindingFlags bindingFlagsForExclusion =
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            FieldInfo[] fieldsForExclusion = target.GetType().BaseType.GetFields(bindingFlagsForExclusion);
            string[] abstractFieldNames = new string[fieldsForExclusion.Length + 1];
            abstractFieldNames[0] = "m_Script";
            for (int i = 0; i < fieldsForExclusion.Length; i++)
            {
                abstractFieldNames[i + 1] = fieldsForExclusion[i].Name;
            }
                
            EditorGUILayout.Space();
            // Draw the default inspector for the derived class, excluding the abstract class fields
            DrawPropertiesExcluding(serializedObject, abstractFieldNames);
            serializedObject.ApplyModifiedProperties();
        }
    }
}

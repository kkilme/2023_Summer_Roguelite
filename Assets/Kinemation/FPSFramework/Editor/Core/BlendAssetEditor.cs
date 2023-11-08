// Designed by KINEMATION, 2023

using Kinemation.FPSFramework.Runtime.Core.Types;
using UnityEditor;
using UnityEngine;

namespace Kinemation.FPSFramework.Editor.Core
{
    [CustomEditor(typeof(BlendAsset), true)]
    public class BlendAssetEditor : UnityEditor.Editor
    {
        private BlendAsset _asset;
        private bool _showBones;
        private Vector2 _scrollPosition;

        private float test;

        private void OnEnable()
        {
            _asset = (BlendAsset) target;
        }

        private void RefreshProfile()
        {
            _asset.blendProfile.Clear();
            
            for (int i = 1; i < _asset.blendMask.transformCount; i++)
            {
                if(_asset.blendMask.GetTransformActive(i))
                {
                    _asset.blendProfile.Add(new BoneBlend()
                    {
                        boneIndex = i,
                        baseWeight = 0f,
                        animWeight = 0f
                    });
                }
            }
            
            EditorUtility.SetDirty(_asset);
            AssetDatabase.SaveAssets();
        }

        private void RenderBones()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            for (int i = 0; i < _asset.blendProfile.Count; i++)
            {
                var boneBlend = _asset.blendProfile[i];
                
                EditorGUILayout.BeginVertical("Box");

                var color = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f);

                string boneName = _asset.blendMask.GetTransformPath(boneBlend.boneIndex);
                boneName = boneName.Substring(boneName.LastIndexOf('/') + 1);
                
                EditorGUILayout.LabelField(boneName, EditorStyles.boldLabel);
                
                boneBlend.baseWeight = EditorGUILayout.Slider("Base Weight", boneBlend.baseWeight, 
                    0f, 1f);
                
                boneBlend.animWeight = EditorGUILayout.Slider("Anim Weight", boneBlend.animWeight, 
                    0f, 1f);

                EditorGUILayout.EndVertical();

                GUI.backgroundColor = color;

                _asset.blendProfile[i] = boneBlend;
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        public override void OnInspectorGUI()
        {
            _asset.blendMask = (AvatarMask) EditorGUILayout.ObjectField("Blend Mask", _asset.blendMask, 
                typeof(AvatarMask), false);
            
            _asset.pose = (AnimationClip) EditorGUILayout.ObjectField("Pose", _asset.pose, 
                typeof(AnimationClip), false);

            EditorGUILayout.BeginHorizontal();
            
            _showBones = EditorGUILayout.Foldout(_showBones, "Layered Blend");

            if (GUILayout.Button("Refresh"))
            {
                RefreshProfile();
            }
            
            if (GUILayout.Button("Save"))
            {
                EditorUtility.SetDirty(_asset);
                AssetDatabase.SaveAssets();
            }
            
            EditorGUILayout.EndHorizontal();

            if (_showBones)
            {
                EditorGUI.indentLevel++;
                RenderBones();
                EditorGUI.indentLevel--;
            }
        }
    }
}
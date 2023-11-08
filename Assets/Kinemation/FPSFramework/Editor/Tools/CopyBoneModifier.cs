// Designed by KINEMATION, 2023

using UnityEditor;
using UnityEngine;

namespace Kinemation.FPSFramework.Editor.Tools
{
    public class CopyBoneModifier : IKinemationTool
    {
        private AnimationClip _sourceClip;
        private AnimationClip _targetClip;

        private Transform _sourceBone;
        private Transform _sourceRoot;

        private Transform _targetBone;
        private Transform _targetRoot;
        
        private void RetargetAnimation()
        {
            // Get all curve bindings from the source clip
            EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(_sourceClip);
            
            foreach (var binding in curveBindings)
            {
                // If this curve belongs to the source transform
                if (binding.path.Equals(AnimationUtility.CalculateTransformPath(_sourceBone, _sourceRoot)))
                {
                    // Create a new binding that points to the target transform instead
                    EditorCurveBinding newBinding = new EditorCurveBinding()
                    {
                        path = AnimationUtility.CalculateTransformPath(_targetBone, _targetRoot),
                        type = binding.type,
                        propertyName = binding.propertyName
                    };
                    
                    // Copy the curve from the source clip to the target clip
                    AnimationCurve curve = AnimationUtility.GetEditorCurve(_sourceClip, binding);
                    AnimationUtility.SetEditorCurve(_targetClip, newBinding, curve);
                }
            }
        }

        public void Render()
        {
            _sourceClip = (AnimationClip) EditorGUILayout.ObjectField("Source Animation", _sourceClip,
                typeof(AnimationClip), false);

            _targetClip = (AnimationClip) EditorGUILayout.ObjectField("Target Animation", _targetClip,
                typeof(AnimationClip), false);

            _sourceRoot = (Transform) EditorGUILayout.ObjectField("Source Root", _sourceRoot, typeof(Transform),
                true);

            _sourceBone = (Transform) EditorGUILayout.ObjectField("Source Target", _sourceBone, typeof(Transform),
                true);

            _targetRoot = (Transform) EditorGUILayout.ObjectField("Target Root", _targetRoot, typeof(Transform),
                true);

            _targetBone = (Transform) EditorGUILayout.ObjectField("Target Bone", _targetBone, typeof(Transform),
                true);
            
            if (_sourceClip == null)
            {
                EditorGUILayout.HelpBox("Please, specify the Source Animation!", MessageType.Warning);
                return;
            }
            
            if (_targetClip == null)
            {
                EditorGUILayout.HelpBox("Please, specify the Target Animation!", MessageType.Warning);
                return;
            }
            
            if (_sourceRoot == null)
            {
                EditorGUILayout.HelpBox("Please, specify the Source Root!", MessageType.Warning);
                return;
            }
            
            if (_sourceBone == null)
            {
                EditorGUILayout.HelpBox("Please, specify the Source Target!", MessageType.Warning);
                return;
            }
            
            if (_targetRoot == null)
            {
                EditorGUILayout.HelpBox("Please, specify the Target Root!", MessageType.Warning);
                return;
            }
            
            if (_targetBone == null)
            {
                EditorGUILayout.HelpBox("Please, specify the Target Bone!", MessageType.Warning);
                return;
            }
            
            if (GUILayout.Button("Retarget Animation"))
            {
                RetargetAnimation();
            }
        }
    }
}
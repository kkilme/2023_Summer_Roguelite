// Designed by KINEMATION, 2023

using Kinemation.FPSFramework.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace Kinemation.FPSFramework.Editor.Tools
{
    public class IKAdditiveGenerator : IKinemationTool
    {
        private const string AdditiveBonePath = "rootBone/WeaponBoneAdditive";
        
        private string _extractBoneName;
        
        private AnimationClip _clip;
        private AnimationClip _refClip;

        private Vector3 _rotationOffset;
        
        private void ExtractAndSetAnimationData()
        {
            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(_clip);

            EditorCurveBinding[] tBindings = new EditorCurveBinding[3];
            EditorCurveBinding[] rBindings = new EditorCurveBinding[4];

            foreach (var binding in bindings)
            {
                if (!binding.path.ToLower().EndsWith(_extractBoneName))
                {
                    continue;
                }

                if (binding.propertyName.ToLower().Contains("m_localposition.x"))
                {
                    tBindings[0] = binding;
                    continue;
                }

                if (binding.propertyName.ToLower().Contains("m_localposition.y"))
                {
                    tBindings[1] = binding;
                    continue;
                }

                if (binding.propertyName.ToLower().Contains("m_localposition.z"))
                {
                    tBindings[2] = binding;
                    continue;
                }

                if (binding.propertyName.ToLower().Contains("m_localrotation.x"))
                {
                    rBindings[0] = binding;
                    continue;
                }

                if (binding.propertyName.ToLower().Contains("m_localrotation.y"))
                {
                    rBindings[1] = binding;
                    continue;
                }

                if (binding.propertyName.ToLower().Contains("m_localrotation.z"))
                {
                    rBindings[2] = binding;
                    continue;
                }

                if (binding.propertyName.ToLower().Contains("m_localrotation.w"))
                {
                    rBindings[3] = binding;
                }
            }

            Vector3 refTranslation = CurveEditorUtility.GetVectorValue(_refClip, tBindings, 0f);
            Quaternion refRotation =
                CurveEditorUtility.GetQuatValue(_refClip, rBindings, 0f) * Quaternion.Euler(_rotationOffset);

            AnimationCurve tX = new AnimationCurve();
            AnimationCurve tY = new AnimationCurve();
            AnimationCurve tZ = new AnimationCurve();

            AnimationCurve rX = new AnimationCurve();
            AnimationCurve rY = new AnimationCurve();
            AnimationCurve rZ = new AnimationCurve();
            AnimationCurve rW = new AnimationCurve();

            float playLength = _clip.length;
            float frameRate = 1f / _clip.frameRate;
            float playBack = 0f;

            while (playBack < playLength)
            {
                Vector3 translation = CurveEditorUtility.GetVectorValue(_clip, tBindings, playBack);
                Quaternion rotation = CurveEditorUtility.GetQuatValue(_clip, rBindings, playBack) *
                                      Quaternion.Euler(_rotationOffset);

                Vector3 deltaT = translation - refTranslation;
                Quaternion deltaR = Quaternion.Inverse(refRotation) * rotation;

                tX.AddKey(playBack, deltaT.x);
                tY.AddKey(playBack, deltaT.y);
                tZ.AddKey(playBack, deltaT.z);

                rX.AddKey(playBack, deltaR.x);
                rY.AddKey(playBack, deltaR.y);
                rZ.AddKey(playBack, deltaR.z);
                rW.AddKey(playBack, deltaR.w);

                playBack += frameRate;
            }

            _clip.SetCurve(AdditiveBonePath, typeof(Transform), tBindings[0].propertyName, tX);
            _clip.SetCurve(AdditiveBonePath, typeof(Transform), tBindings[1].propertyName, tY);
            _clip.SetCurve(AdditiveBonePath, typeof(Transform), tBindings[2].propertyName, tZ);

            _clip.SetCurve(AdditiveBonePath, typeof(Transform), rBindings[0].propertyName, rX);
            _clip.SetCurve(AdditiveBonePath, typeof(Transform), rBindings[1].propertyName, rY);
            _clip.SetCurve(AdditiveBonePath, typeof(Transform), rBindings[2].propertyName, rZ);
            _clip.SetCurve(AdditiveBonePath, typeof(Transform), rBindings[3].propertyName, rW);
        }
        
        public void Render()
        {
            EditorGUILayout.HelpBox("This tool extracts curves from `"
                                    + _extractBoneName
                                    + "`, and creates curve additive animation."
                                    + "\nUseful for procedural animations, like idle, walk or sprint.",
                MessageType.Info);
        
            GUILayout.Label("Settings", EditorStyles.boldLabel);

            _clip =
                EditorGUILayout.ObjectField("Target Animation", _clip, typeof(AnimationClip), true)
                    as AnimationClip;

            _refClip =
                EditorGUILayout.ObjectField("Reference Additive Animation", _refClip, 
                        typeof(AnimationClip), true) as AnimationClip;

            _extractBoneName = EditorGUILayout.TextField("Target Bone Name", _extractBoneName);

            _rotationOffset = EditorGUILayout.Vector3Field("Rotation Offset", _rotationOffset);
            
            if (_clip == null)
            {
                EditorGUILayout.HelpBox("Please, specify the Target Animation!", MessageType.Warning);
                return;
            }

            if (_refClip == null)
            {
                EditorGUILayout.HelpBox("Please, specify the Reference Animation!", MessageType.Warning);
                return;
            }

            if (string.IsNullOrEmpty(_extractBoneName))
            {
                EditorGUILayout.HelpBox("Please, specify the bone name!", MessageType.Warning);
                return;
            }

            if (GUILayout.Button("Extract"))
            {
                ExtractAndSetAnimationData();
            }
        }
    }
}
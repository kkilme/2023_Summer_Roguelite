// Designed by KINEMATION, 2023

using System.Collections.Generic;
using Kinemation.FPSFramework.Runtime.Attributes;
using Kinemation.FPSFramework.Runtime.Core.Components;
using Kinemation.FPSFramework.Runtime.Core.Types;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;

namespace Kinemation.FPSFramework.Runtime.Layers
{
    public class LeftHandIKLayer : AnimLayer
    {
        [Header("Left Hand IK Settings")]

        [AnimCurveName] [SerializeField] private string maskCurveName;
        [SerializeField] private AvatarMask leftHandMask;
        [SerializeField] private bool usePoseOverride = true;

        private LocRot _cache = LocRot.identity;
        private LocRot _final = LocRot.identity;
        
        private LocRot _defaultLeftHand = LocRot.identity;
        private List<BoneRef> _leftHandChain = new List<BoneRef>();

        public override void OnAnimStart()
        {
            if (leftHandMask == null)
            {
                Debug.LogWarning("LeftHandIKLayer: no mask for the left hand assigned!");
                return;
            }

            BoneRef.InitBoneChain(ref _leftHandChain, transform, leftHandMask);
        }

        public override void OnPoseSampled()
        {
            if (!usePoseOverride)
            {
                return;
            }
            
            _cache = _final;

            if (leftHandMask == null) return;

            for (int i = 0; i < _leftHandChain.Count; i++)
            {
                var bone = _leftHandChain[i];
                bone.CopyBone();
                _leftHandChain[i] = bone;
            }

            var rotOffset = GetGunAsset().rotationOffset;
            var gunBone = GetRigData().weaponBone;
            
            gunBone.rotation *= rotOffset;
            _defaultLeftHand.position = gunBone.InverseTransformPoint(GetLeftHandIK().target.position);
            _defaultLeftHand.rotation = Quaternion.Inverse(gunBone.rotation) * GetLeftHandIK().target.rotation;
            gunBone.rotation *= Quaternion.Inverse(rotOffset);
        }

        private void OverrideLeftHand(float weight)
        {
            weight = Mathf.Clamp01(weight);

            if (Mathf.Approximately(weight, 0f))
            {
                return;
            }
            
            foreach (var bone in _leftHandChain)
            {
                bone.Slerp(weight);
            }
        }
        
        private LocRot basePoseT = LocRot.identity;
        private LocRot handTransform = LocRot.identity;

        public void UpdateHandTransforms()
        {
            basePoseT.position = GetMasterPivot().InverseTransformPoint(GetLeftHand().position) + GetPivotOffset();
            basePoseT.rotation =
                Quaternion.Inverse(Quaternion.Inverse(GetMasterPivot().rotation) * GetLeftHand().rotation);
            
            if (GetTransforms().leftHandTarget == null)
            {
                handTransform = _defaultLeftHand;
            }
            else
            {
                var target = GetTransforms().leftHandTarget;
                handTransform = new LocRot(target.localPosition, target.localRotation);
            }
            
            handTransform.position -= basePoseT.position;
            handTransform.rotation *= basePoseT.rotation;
        }

        public override void OnAnimUpdate()
        {
            if (GetGunAsset() == null) return;
            
            UpdateHandTransforms();
            
            float alpha = (1f - GetCurveValue(maskCurveName)) * (1f - smoothLayerAlpha) * layerAlpha;
            float progress = core.animGraph.GetPoseProgress();

            _final = CoreToolkitLib.Lerp(_cache, handTransform, progress);

            if (usePoseOverride)
            {
                OverrideLeftHand(alpha);
            }
            
            GetLeftHandIK().Move(GetMasterPivot(), _final.position, alpha);
            GetLeftHandIK().Rotate(GetMasterPivot().rotation, _final.rotation, alpha);

            Vector3 ikOffset = new Vector3()
            {
                x = GetCurveValue(CurveLib.Curve_IK_LeftHand_X),
                y = GetCurveValue(CurveLib.Curve_IK_LeftHand_Y),
                z = GetCurveValue(CurveLib.Curve_IK_LeftHand_Z),
            };
            
            GetLeftHandIK().Move(GetRootBone(), ikOffset);
            
            ikOffset = new Vector3()
            {
                x = GetCurveValue(CurveLib.Curve_IK_X),
                y = GetCurveValue(CurveLib.Curve_IK_Y),
                z = GetCurveValue(CurveLib.Curve_IK_Z),
            };
            
            GetMasterIK().Move(GetRootBone(), ikOffset);
        }
    }
}
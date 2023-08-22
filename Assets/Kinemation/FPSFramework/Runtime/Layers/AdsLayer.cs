// Designed by KINEMATION, 2023

using System;
using Kinemation.FPSFramework.Runtime.Core.Components;
using Kinemation.FPSFramework.Runtime.Core.Types;
using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.Layers
{
    public class AdsLayer : AnimLayer
    {
        [Header("SightsAligner")] [SerializeField]
        private EaseMode adsEaseMode = new EaseMode(new[]
        {
            new Keyframe(0f, 0f),
            new Keyframe(1f, 0f)
        });

        [SerializeField] private EaseMode pointAimEaseMode = new EaseMode(new[]
        {
            new Keyframe(0f, 0f),
            new Keyframe(1f, 0f)
        });
        
        [Range(0f, 1f)] public float aimLayerAlphaLoc;
        [Range(0f, 1f)] public float aimLayerAlphaRot;
        [SerializeField] [Bone] protected Transform aimTarget;

        [SerializeField] private LocRot crouchPose;
        [SerializeField] [AnimCurveName(true)] private string crouchPoseCurve;

        protected bool bAds;
        protected float adsProgress;
        
        protected bool bPointAim;
        protected float pointAimProgress;
        
        protected float adsWeight;
        protected float pointAimWeight;
        
        protected LocRot interpAimPoint;
        protected LocRot viewOffsetCache;
        protected LocRot viewOffset;

        [Obsolete("use `SetAds(bool)` instead")]
        public void SetAdsAlpha(float weight)
        {
            weight = Mathf.Clamp01(weight);
            bAds = !Mathf.Approximately(weight, 0f);
        }
        
        [Obsolete("use `SetPointAim(bool)` instead")]
        public void SetPointAlpha(float weight)
        {
            weight = Mathf.Clamp01(weight);
            bPointAim = !Mathf.Approximately(weight, 0f);
        }
        
        public void SetAds(bool bAiming)
        {
            bAds = bAiming;
            interpAimPoint = bAds ? GetAdsOffset() : interpAimPoint;
        }
        
        public void SetPointAim(bool bAiming)
        {
            bPointAim = bAiming;
        }

        public override void OnAnimUpdate()
        {
            Vector3 baseLoc = GetMasterPivot().position;
            Quaternion baseRot = GetMasterPivot().rotation;

            ApplyCrouchPose();
            ApplyPointAiming();
            ApplyAiming();
            
            Vector3 postLoc = GetMasterPivot().position;
            Quaternion postRot = GetMasterPivot().rotation;

            GetMasterPivot().position = Vector3.Lerp(baseLoc, postLoc, smoothLayerAlpha);
            GetMasterPivot().rotation = Quaternion.Slerp(baseRot, postRot, smoothLayerAlpha);
        }

        public void CalculateAimData()
        {
            var aimData = GetGunAsset() == null ? GetGunData().gunAimData.target : GetGunAsset().adsData.target;

            var stateName = aimData.stateName.Length > 0
                ? aimData.stateName
                : aimData.staticPose.name;

            if (GetAnimator() != null)
            {
                GetAnimator().Play(stateName);
                GetAnimator().Update(0f);
            }
            
            // Cache the local data, so we can apply it without issues
            aimData.aimLoc = GetPivotPoint().InverseTransformPoint(aimTarget.position);
            aimData.aimRot = Quaternion.Inverse(GetPivotPoint().rotation) * GetRootBone().rotation;
        }

        protected void UpdateAimWeights(float adsRate = 1f, float pointAimRate = 1f)
        {
            adsWeight = CurveLib.Ease(0f, 1f, adsProgress, adsEaseMode);
            pointAimWeight = CurveLib.Ease(0f, 1f, pointAimProgress, pointAimEaseMode);
            
            adsProgress += Time.deltaTime * (bAds ? adsRate : -adsRate);
            pointAimProgress += Time.deltaTime * (bPointAim ? pointAimRate : -pointAimRate);

            adsProgress = Mathf.Clamp(adsProgress, 0f, 1f);
            pointAimProgress = Mathf.Clamp(pointAimProgress, 0f, 1f);
        }

        protected LocRot GetAdsOffset()
        {
            LocRot adsOffset = new LocRot(Vector3.zero, Quaternion.identity);

            if (GetAimPoint() != null)
            {
                adsOffset.rotation = Quaternion.Inverse(GetPivotPoint().rotation) * GetAimPoint().rotation;
                adsOffset.position = -GetPivotPoint().InverseTransformPoint(GetAimPoint().position);
            }

            return adsOffset;
        }

        protected virtual void ApplyAiming()
        {
            var aimData = GetGunData().gunAimData;
            var targetAimData = GetGunAsset() != null ? GetGunAsset().adsData.target : aimData.target;

            float aimSpeed = GetGunAsset() != null ? GetGunAsset().adsData.aimSpeed : aimData.aimSpeed;
            float pointAimSpeed = GetGunAsset() != null ? GetGunAsset().adsData.pointAimSpeed : aimData.pointAimSpeed;
            float changeSightSpeed = GetGunAsset() != null ? GetGunAsset().adsData.changeSightSpeed : aimData.changeSightSpeed;
            
            // Base Animation layer
            
            LocRot defaultPose = new LocRot(GetMasterPivot());
            ApplyHandsOffset();
            LocRot handsPose = new LocRot(GetMasterPivot());
            
            GetMasterPivot().position = defaultPose.position;
            GetMasterPivot().rotation = defaultPose.rotation;

            UpdateAimWeights(aimSpeed, pointAimSpeed);

            interpAimPoint = CoreToolkitLib.Glerp(interpAimPoint, GetAdsOffset(), changeSightSpeed);
            
            LocRot additiveAim = targetAimData != null ? new LocRot(targetAimData.aimLoc, targetAimData.aimRot) 
                : new LocRot(Vector3.zero, Quaternion.identity);
            
            Vector3 addAimLoc = additiveAim.position;
            Quaternion addAimRot = additiveAim.rotation;
            
            CoreToolkitLib.MoveInBoneSpace(GetMasterPivot(), GetMasterPivot(), addAimLoc, 1f);
            GetMasterPivot().rotation *= addAimRot;
            CoreToolkitLib.MoveInBoneSpace(GetMasterPivot(), GetMasterPivot(), interpAimPoint.position, 1f);

            addAimLoc = GetMasterPivot().position;
            addAimRot = GetMasterPivot().rotation;

            GetMasterPivot().position = handsPose.position;
            GetMasterPivot().rotation = handsPose.rotation;
            ApplyAbsAim(interpAimPoint.position, interpAimPoint.rotation);

            // Blend between Absolute and Additive
            GetMasterPivot().position = Vector3.Lerp(GetMasterPivot().position, addAimLoc, aimLayerAlphaLoc);
            GetMasterPivot().rotation = Quaternion.Slerp(GetMasterPivot().rotation, addAimRot, aimLayerAlphaRot);

            float aimWeight = Mathf.Clamp01(adsWeight - pointAimWeight);
            
            // Blend Between Non-Aiming and Aiming
            GetMasterPivot().position = Vector3.Lerp(handsPose.position, GetMasterPivot().position, aimWeight);
            GetMasterPivot().rotation = Quaternion.Slerp(handsPose.rotation, GetMasterPivot().rotation, aimWeight);
        }

        protected void ApplyCrouchPose()
        {
            float poseAlpha = GetAnimator().GetFloat(crouchPoseCurve) * (1f - adsWeight);
            GetMasterIK().Move(GetRootBone(), crouchPose.position, poseAlpha);
            GetMasterIK().Rotate(GetRootBone().rotation, crouchPose.rotation, poseAlpha);
        }

        protected virtual void ApplyPointAiming()
        {
            var pointAimOffset = GetGunAsset() != null ? GetGunAsset().adsData.pointAimOffset 
                : GetGunData().gunAimData.pointAimOffset;
            
            CoreToolkitLib.MoveInBoneSpace(GetRootBone(), GetMasterPivot(),
                pointAimOffset.position, pointAimWeight);
            CoreToolkitLib.RotateInBoneSpace(GetRootBone().rotation, GetMasterPivot(),
                pointAimOffset.rotation, pointAimWeight);
        }

        protected virtual void ApplyHandsOffset()
        {
            float progress = core.animGraph.GetPoseProgress();
            if (Mathf.Approximately(progress, 0f))
            {
                viewOffsetCache = viewOffset;
            }

            var targetViewOffset = GetGunAsset() != null ? GetGunAsset().viewOffset : GetGunData().viewOffset;
            viewOffset = CoreToolkitLib.Lerp(viewOffsetCache, targetViewOffset, progress);
            
            CoreToolkitLib.MoveInBoneSpace(GetRootBone(), GetMasterPivot(), 
                viewOffset.position, 1f);
            CoreToolkitLib.RotateInBoneSpace(GetRootBone().rotation, GetMasterPivot(), 
                viewOffset.rotation, 1f);
        }

        // Absolute aiming overrides base animation
        protected virtual void ApplyAbsAim(Vector3 loc, Quaternion rot)
        {
            Vector3 offset = -loc;
            GetMasterPivot().position = aimTarget.position;
            GetMasterPivot().rotation = GetRootBone().rotation * rot;
            CoreToolkitLib.MoveInBoneSpace(GetMasterPivot(),GetMasterPivot(), -offset, 1f);
        }
    }
}
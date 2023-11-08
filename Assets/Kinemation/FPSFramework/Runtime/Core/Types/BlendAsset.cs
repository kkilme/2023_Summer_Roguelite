// Designed by KINEMATION, 2023

using Kinemation.FPSFramework.Runtime.Attributes;

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.Core.Types
{
    [Serializable]
    public struct BoneBlend
    {
        // Bone index in the avatar mask
        public int boneIndex;
        // Blend factor between the cached and current poses.
        public float baseWeight;
        // Blend factor of the cached pose animation.
        public float animWeight;
        
        // Actual bone reference
        [NonSerialized] public Transform boneRef;
        [NonSerialized] public Quaternion targetPoseTo;
        
        [NonSerialized] public Quaternion localPose;
        [NonSerialized] public Quaternion basePoseFrom;
        [NonSerialized] public Quaternion basePoseTo;

        public Quaternion BlendBasePose(float alpha)
        {
            return Quaternion.Slerp(basePoseFrom, basePoseTo, alpha);
        }
    }

    [System.Serializable, CreateAssetMenu(fileName = "NewBlendAsset", menuName = "FPS Animator/Blend Asset")]
    public class BlendAsset : ScriptableObject
    {
        public AvatarMask blendMask;
        public AnimationClip pose;
        public List<BoneBlend> blendProfile = new List<BoneBlend>();

        public bool IsValid()
        {
            return blendMask != null && pose != null && blendProfile != null && blendProfile.Count != 0;
        }
    }
    
    [Serializable]
    public class PoseBlend
    {
        [AnimCurveName(true)] public string curveName;
        public BlendAsset blendAsset;

        public BoneBlend[] BlendProfile { get; private set; }
        public Quaternion PelvisRotation { get; private set; }
        public bool IsValid { get; private set; }
        public Transform SpineRoot { get; private set; }
        public Transform Pelvis { get; private set; }

        // Must be called when a game starts.
        public void Initialize(Transform root, Transform pelvis, Transform spineRoot)
        {
            IsValid = false;
            if (blendAsset == null || !blendAsset.IsValid() || root == null || pelvis == null) return;
            
            blendAsset.pose.SampleAnimation(root.gameObject, 0f);

            PelvisRotation = pelvis.localRotation;
            Pelvis = pelvis;
            SpineRoot = spineRoot;

            BlendProfile = blendAsset.blendProfile.ToArray();
            
            for (int i = 0; i < BlendProfile.Length; i++)
            {
                var profile = BlendProfile[i];
                
                var t = root.Find(blendAsset.blendMask.GetTransformPath(profile.boneIndex));
                profile.boneRef = t;
                profile.targetPoseTo = t.localRotation;
                profile.localPose = profile.basePoseFrom = profile.basePoseTo = Quaternion.identity;

                BlendProfile[i] = profile;
            }
            
            IsValid = true;
        }

        // Must be called when a base pose is sampled (item is equipped.)
        public void UpdateBasePose()
        {
            if (!IsValid) return;
            
            for (int i = 0; i < BlendProfile.Length; i++)
            {
                var profile = BlendProfile[i];

                profile.basePoseFrom = profile.basePoseTo;
                profile.basePoseTo = profile.boneRef.localRotation;

                BlendProfile[i] = profile;
            }
        }

        public void UpdateLocalPose()
        {
            if (!IsValid) return;
            
            for (int i = 0; i < BlendProfile.Length; i++)
            {
                var blendProfile = BlendProfile[i];
                blendProfile.localPose = blendProfile.boneRef.localRotation;
                BlendProfile[i] = blendProfile;
            }
        }

        public void Blend(Quaternion spineRoot, float alpha, float poseAlpha)
        {
            if (!IsValid) return;

            for (int i = 0; i < BlendProfile.Length; i++)
            {
                var boneBlend = BlendProfile[i];

                float weight = blendAsset.blendProfile[i].baseWeight * alpha;
                float animWeight = blendAsset.blendProfile[i].animWeight;
                
                // Target rotation
                Quaternion combinedRot = boneBlend.targetPoseTo;
                // Current animated bone local rotation.
                Quaternion baseRot = boneBlend.localPose;

                // Spine Root is used for upper body stabilization, so we must handle it separately
                bool bSpineRoot = boneBlend.boneRef == SpineRoot;

                if (bSpineRoot)
                {
                    baseRot = spineRoot;
                }

                // Apply animation
                Quaternion animationDelta = Quaternion.Inverse(boneBlend.BlendBasePose(poseAlpha)) * baseRot;
                combinedRot *= Quaternion.Slerp(Quaternion.identity, animationDelta, animWeight);
                
                if (bSpineRoot)
                {
                    var hipCache = Pelvis.rotation;
                    var spineCache = SpineRoot.rotation;

                    SpineRoot.localRotation = combinedRot;
                    Pelvis.localRotation = PelvisRotation;

                    Pelvis.rotation = hipCache;
                    combinedRot = SpineRoot.localRotation;
                    SpineRoot.rotation = spineCache;
                }

                Quaternion nextBoneRotation = Quaternion.identity;
                if (i < BlendProfile.Length - 1)
                {
                    nextBoneRotation = BlendProfile[i + 1].boneRef.rotation;
                }
                
                boneBlend.boneRef.localRotation = Quaternion.Slerp(boneBlend.boneRef.localRotation, combinedRot, 
                    weight);
                
                if (i < BlendProfile.Length - 1)
                {
                    BlendProfile[i + 1].boneRef.rotation = nextBoneRotation;
                }
            }
        }
    }
}
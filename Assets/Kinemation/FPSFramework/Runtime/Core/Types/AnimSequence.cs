// Designed by KINEMATION, 2023

using Kinemation.FPSFramework.Runtime.Core.Playables;

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.Core.Types
{
    [Serializable]
    public class AnimSequence : ScriptableObject
    {
        [Header("Animation")]
        [Tooltip("Select your animation.")]
        public AnimationClip clip = null;

        [Tooltip("What bones will be animated.")]
        public AvatarMask mask = null;
        
        [Tooltip("Applied to the spine root bone.")]
        public Quaternion spineRotation = Quaternion.identity;

        [Tooltip("This mask will define what parts will be excluded from additive motion.")]
        public AvatarMask overrideMask = null;
        public bool isAdditive = false;

        [Header("Blend In/Out")]
        [Tooltip("Smooth blend in/out parameters.")]
        public BlendTime blendTime = new BlendTime(0.15f, 0.15f);
        public List<AnimCurve> curves;

        public float GetTimeAtFrame(int frame)
        {
            if (clip == null)
            {
                return 0f;
            }

            frame = frame < 0 ? frame * -1 : frame;
            return frame / (clip.frameRate * clip.length);
        }

        public float GetNormTimeAtFrame(int frame)
        {
            if (clip == null)
            {
                return 0f;
            }
            
            return GetTimeAtFrame(frame) / clip.length;
        }
    }
}
// Designed by KINEMATION, 2023

using Kinemation.FPSFramework.Runtime.Core.Components;
using Kinemation.FPSFramework.Runtime.Core.Types;

using System.Collections.Generic;
using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.Layers
{
    public class PoseBlending : AnimLayer
    {
        [SerializeField] private List<PoseBlend> poseBlending;
        private Quaternion _spineRoot;

        public override void OnAnimStart()
        {
            if (!Application.isPlaying) return;
            
            foreach (var poseBlend in poseBlending)
            {
                poseBlend.Initialize(transform, GetPelvis(), GetRigData().spineRoot);
            }
        }

        public override void OnPreAnimUpdate()
        {
            base.OnPreAnimUpdate();
            _spineRoot = GetRigData().spineRoot.localRotation;
        }

        public override void OnAnimUpdate()
        {
            if (Mathf.Approximately(smoothLayerAlpha, 0f) || GetGunAsset() == null) return;

            float poseAlpha = core.animGraph.GetPoseProgress();

            foreach (var poseBlend in poseBlending)
            {
                poseBlend.UpdateLocalPose();
            }

            foreach (var poseBlend in poseBlending)
            {
                float curveBlend = string.IsNullOrEmpty(poseBlend.curveName)
                    ? 1f
                    : GetAnimator().GetFloat(poseBlend.curveName);
                
                if(Mathf.Approximately(curveBlend, 0f)) continue;
                poseBlend.Blend(_spineRoot, smoothLayerAlpha * curveBlend, poseAlpha);
            }
        }

        public override void OnPoseSampled()
        {
            base.OnPoseSampled();

            foreach (var blend in poseBlending)
            {
                blend.UpdateBasePose();
            }
        }
    }
}
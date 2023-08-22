// Designed by KINEMATION, 2023

using UnityEngine;
using Kinemation.FPSFramework.Runtime.Camera;
using Kinemation.FPSFramework.Runtime.Core.Components;
using Kinemation.FPSFramework.Runtime.Core.Types;
using Kinemation.FPSFramework.Runtime.Layers;
using Kinemation.FPSFramework.Runtime.Recoil;

namespace Kinemation.FPSFramework.Runtime.FPSAnimator
{
    // Animation Controller Interface
    // Make sure to derive your controller from this class
    public abstract class FPSAnimController : MonoBehaviour
    {
        private CoreAnimComponent fpsAnimator;
        private FPSCamera fpsCamera;
        private LookLayer internalLookLayer;
        protected RecoilAnimation recoilComponent;
        protected CharAnimData charAnimData;
        
        // Used primarily for function calls from Animation Events
        // Runs once at the beginning of the next update
        protected CoreToolkitLib.PostUpdateDelegate queuedAnimEvents;
        
        protected CoreAnimGraph GetAnimGraph()
        {
            return fpsAnimator.animGraph;
        }

        // Call this once when the character is initialized
        protected void InitAnimController()
        {
            fpsAnimator = GetComponentInChildren<CoreAnimComponent>();
            fpsAnimator.animGraph.InitPlayableGraph();
            fpsAnimator.InitializeLayers();

            recoilComponent = GetComponentInChildren<RecoilAnimation>();
            charAnimData = new CharAnimData();

            fpsCamera = GetComponentInChildren<FPSCamera>();
            internalLookLayer = GetComponentInChildren<LookLayer>();
        }

        // Call this once when the character is initialized
        protected void InitAnimController(CoreToolkitLib.PostUpdateDelegate cameraDelegate)
        {
            InitAnimController();
            fpsAnimator.OnPostAnimUpdate += cameraDelegate;

            if (fpsCamera == null) return;
            fpsAnimator.OnPostAnimUpdate += fpsCamera.UpdateCamera;
        }
        
        // Call this when equipping a new weapon
        protected void InitWeapon(FPSAnimWeapon weapon)
        {
            recoilComponent.Init(weapon.recoilData, weapon.fireRate, weapon.fireMode);

            if (weapon.weaponAsset != null)
            {
                fpsAnimator.OnGunEquipped(weapon.weaponAsset, weapon.weaponTransformData);
            }
            else
            {
                fpsAnimator.OnGunEquipped(weapon.weaponAnimData);
            }
            
            fpsAnimator.ikRigData.weaponTransform = weapon.weaponBone;
            internalLookLayer.SetAimOffsetTable(weapon.aimOffsetTable);

            fpsAnimator.OnPrePoseSampled();
            PlayPose(weapon.overlayPose);
            fpsAnimator.OnPoseSampled();
        }

        // Call this when changing sights
        protected void InitAimPoint(FPSAnimWeapon weapon)
        {
            fpsAnimator.OnSightChanged(weapon.GetAimPoint());
        }

        // Call this during Update after all the gameplay logic
        protected void UpdateAnimController()
        {
            
            if (queuedAnimEvents != null)
            {
                queuedAnimEvents.Invoke();
                queuedAnimEvents = null;
            }
            
            charAnimData.recoilAnim = new LocRot(recoilComponent.OutLoc, Quaternion.Euler(recoilComponent.OutRot));
            fpsAnimator.SetCharData(charAnimData);
            
            fpsAnimator.animGraph.UpdateGraph();
            fpsAnimator.UpdateCoreComponent();
        }

        // Call this to play a Camera shake
        protected void PlayCameraShake(FPSCameraShake shake)
        {
            if (fpsCamera != null)
            {
                fpsCamera.PlayShake(shake.shakeInfo);
            }
        }
        
        protected void PlayController(RuntimeAnimatorController controller, AnimSequence motion)
        {
            if (motion == null) return;
            fpsAnimator.animGraph.PlayController(controller, motion.clip, motion.blendTime.blendInTime);
        }
        
        // Call this to play a static pose on the character upper body
        protected void PlayPose(AnimSequence motion)
        {
            if (motion == null) return;
            fpsAnimator.animGraph.PlayPose(motion.clip, motion.blendTime.blendInTime);
        }

        // Call this to play an animation on the character upper body
        protected void PlayAnimation(AnimSequence motion)
        {
            if (motion == null) return;
            fpsAnimator.animGraph.PlayAnimation(motion.clip, motion.blendTime, motion.spineRotation,
                motion.curves.ToArray());
        }
        
        protected void StopAnimation(float blendTime = 0f)
        {
            fpsAnimator.animGraph.StopAnimation(blendTime);
        }
    }
}
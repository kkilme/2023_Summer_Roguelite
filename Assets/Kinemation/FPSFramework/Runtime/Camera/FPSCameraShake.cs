// Designed by KINEMATION, 2023

using Kinemation.FPSFramework.Runtime.Attributes;
using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.Camera
{
    [CreateAssetMenu(fileName = "NewCameraShake", menuName = "FPS Animator/FPSCameraShake")]
    public class FPSCameraShake : ScriptableObject
    {
        [Fold(false)] public CameraShakeInfo shakeInfo
            = new CameraShakeInfo(new[] {new Keyframe(0f, 0f), new Keyframe(1f, 0f)},
                1f, 1f);
    }
}
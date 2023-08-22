// Designed by KINEMATION, 2023

using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.Core.Types
{
    [System.Serializable, CreateAssetMenu(fileName = "NewAimData", menuName = "FPS Animator/TargetAimData")]
    public class TargetAimData : ScriptableObject
    {
        public Vector3 aimLoc;
        public Quaternion aimRot;
        public AnimationClip staticPose;
        public string stateName;
    }
}
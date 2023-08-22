// Designed by KINEMATION, 2023

using System;
using UnityEngine;
using Random = UnityEngine.Random;

using Kinemation.FPSFramework.Runtime.Core.Types;

namespace Kinemation.FPSFramework.Runtime.Camera
{
    [Serializable]
    public struct SpringShakeProfile
    {
        [SerializeField] public VectorSpringData springData;
        [SerializeField] public float dampSpeed;
        [SerializeField] public Vector2 pitch;
        [SerializeField] public Vector2 yaw;
        [SerializeField] public Vector2 roll;

        public Vector3 GetRandomTarget()
        {
            return new Vector3(Random.Range(pitch.x, pitch.y), Random.Range(yaw.x, yaw.y),
                Random.Range(roll.x, roll.y));
        }
    }
    
    [Obsolete("use FPSCamera instead")]
    public class SpringCameraShake : MonoBehaviour
    {
        public SpringShakeProfile shakeProfile;
        private Vector3 _dampedTarget;
        private Vector3 _target;
        private Quaternion _deltaDampedTarget;

        // Should be applied after camera stabilization logic
        private void LateUpdate()
        {
            Debug.LogWarning("SpringCameraShake is obsolete!");
        }

        public void PlayCameraShake()
        {
            Debug.LogWarning("SpringCameraShake is obsolete!");
        }
    }
}

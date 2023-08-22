// Designed by KINEMATION, 2023

using System;
using UnityEngine;
using Random = UnityEngine.Random;

using Kinemation.FPSFramework.Runtime.Core.Types;

namespace Kinemation.FPSFramework.Runtime.Camera
{
    [Serializable]
    public struct CameraShakeInfo
    {
        public VectorCurve shakeCurve;
        public Vector4 pitch;
        public Vector4 yaw;
        public Vector4 roll;
        public float smoothSpeed;
        public float playRate;

        public CameraShakeInfo(Keyframe[] frames, float playRate, float smooth)
        {
            shakeCurve = new VectorCurve(frames);
            pitch = new Vector4(0f, 0f, 0f, 0f);
            yaw = new Vector4(0f, 0f, 0f, 0f);
            roll = new Vector4(0f, 0f, 0f, 0f);
            smoothSpeed = smooth;
            this.playRate = playRate;
        }
    }
    
    public class FPSCamera : MonoBehaviour
    {
        private CameraShakeInfo _shake;
        private Vector3 _target;
        private Vector3 _out;
        private float _playBack = 0f;

        private float GetRandomTarget(Vector4 target)
        {
            return Random.Range(Random.Range(target.x, target.y), Random.Range(target.z, target.w));
        }
        
        private void UpdateShake()
        {
            if (!_shake.shakeCurve.IsValid())
            {
                return;
            }

            _playBack += _shake.playRate * Time.deltaTime;
            _playBack = Mathf.Min(_playBack, _shake.shakeCurve.GetLastTime());

            Vector3 curveValue = _shake.shakeCurve.Evaluate(_playBack);

            _out.x = CoreToolkitLib.Glerp(_out.x, curveValue.x * _target.x, _shake.smoothSpeed);
            _out.y = CoreToolkitLib.Glerp(_out.y, curveValue.y * _target.y, _shake.smoothSpeed);
            _out.z = CoreToolkitLib.Glerp(_out.z, curveValue.z * _target.z, _shake.smoothSpeed);

            Quaternion rot = Quaternion.Euler(_out);
            transform.rotation *= rot;
        }

        public void UpdateCamera()
        {
            UpdateShake();
        }

        public void PlayShake(CameraShakeInfo shake)
        {
            _shake = shake;
            _target.x = GetRandomTarget(_shake.pitch);
            _target.y = GetRandomTarget(_shake.yaw);
            _target.z = GetRandomTarget(_shake.roll);

            _playBack = 0f;
        }
    }
}
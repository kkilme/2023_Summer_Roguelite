// Designed by KINEMATION, 2023

using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.FPSAnimator
{
    public static class FPSAnimLib
    {
        public static float ExpDecayAlpha(float speed, float deltaTime)
        {
            return 1 - Mathf.Exp(-speed * deltaTime);
        }
    }
}
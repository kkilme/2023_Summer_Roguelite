// Designed by KINEMATION, 2023

using System;
using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class AnimCurveName : PropertyAttribute
    {
        public bool isAnimator;
        public AnimCurveName(bool isAnimator = false)
        {
            this.isAnimator = isAnimator;
        }
    }
        
    public class FoldAttribute : PropertyAttribute
    {
        public bool useDefaultDisplay;

        public FoldAttribute(bool useDefaultDisplay = true)
        {
            this.useDefaultDisplay = useDefaultDisplay;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class TabAttribute : PropertyAttribute
    {
        public readonly string tabName;

        public TabAttribute(string tabName)
        {
            this.tabName = tabName;
        }
    }

    public class BoneAttribute : PropertyAttribute
    {
    }

    public class ReadOnlyAttribute : PropertyAttribute
    {
    }
    
    public class CustomAttributes
    {
    }
}
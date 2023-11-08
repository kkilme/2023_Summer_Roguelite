// Designed by KINEMATION, 2023

using Kinemation.FPSFramework.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace Kinemation.FPSFramework.Editor.FPSAnimator
{
    public class FPSAnimatorUtilityEditorWindow : EditorWindow
    {
        private IKAdditiveGenerator _ikAdditiveGenerator;
        private CopyBoneModifier _copyBoneModifier;
        private AvatarMaskModifier _avatarMaskModifier;
        private RootBoneRotator _rootBoneRotator;
        
        private int tab = 0;

        [MenuItem("Window/FPS Animation Framework/FPS Animator Utility")]
        public static void ShowWindow()
        {
            GetWindow<FPSAnimatorUtilityEditorWindow>("FPS Animator Utility");
        }

        private void OnEnable()
        {
            _ikAdditiveGenerator = new IKAdditiveGenerator();
            _copyBoneModifier = new CopyBoneModifier();
            _avatarMaskModifier = new AvatarMaskModifier();
            _rootBoneRotator = new RootBoneRotator();
        }

        private void OnGUI()
        {
            tab = GUILayout.Toolbar(tab, new string[]
            {
                "IK Additive Generator",
                "Copy Bone Modifier",
                "Avatar Mask Modifier", 
                "Root Bone Rotator"
            });

            switch (tab)
            {
                case 0:
                    _ikAdditiveGenerator.Render();
                    break;
                case 1:
                    _copyBoneModifier.Render();
                    break;
                case 2:
                    _avatarMaskModifier.Render();
                    break;
                case 3:
                    _rootBoneRotator.Render();
                    break;
            }
        }
    }
}
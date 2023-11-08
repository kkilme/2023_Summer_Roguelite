// Designed by KINEMATION, 2023

using Kinemation.FPSFramework.Runtime.Core.Types;
using UnityEditor;
using UnityEngine;

namespace Kinemation.FPSFramework.Editor.Tools
{
    public class RootBoneRotator : IKinemationTool
    {
        private GameObject character;
        private Transform rootBone;
        private AnimationClip targetClip;
        private Vector3 offset;
        
        struct AnimCurves
        {
            public AnimationCurve[] tCurves;
            public AnimationCurve[] rCurves;
        }

        public void Render()
        {
            character = EditorGUILayout.ObjectField("Character", character, typeof(GameObject), true)
                as GameObject;
            
            rootBone = EditorGUILayout.ObjectField("Root Bone", rootBone, typeof(Transform), true)
                as Transform;
            
            targetClip = EditorGUILayout.ObjectField("Animation", targetClip, typeof(AnimationClip), true)
                as AnimationClip;
            
            offset = EditorGUILayout.Vector3Field("Offset", offset);
            
            if (GUILayout.Button("Rotate"))
            {
                var bones = rootBone.GetComponentsInChildren<Transform>();
                LocRot[] rotations = new LocRot[bones.Length - 1];
                AnimCurves[] curves = new AnimCurves[bones.Length - 1];
                
                AnimCurves rootCurves = new AnimCurves();
                var animRootRotation = rootBone.localRotation * Quaternion.Euler(offset);
                
                rootCurves.tCurves = new[]
                {
                    new AnimationCurve(new Keyframe(0f, 0f)),
                    new AnimationCurve(new Keyframe(0f, 0f)),
                    new AnimationCurve(new Keyframe(0f, 0f))
                };
                
                rootCurves.rCurves = new[]
                {
                    new AnimationCurve(new Keyframe(0f, animRootRotation.x)),
                    new AnimationCurve(new Keyframe(0f, animRootRotation.y)),
                    new AnimationCurve(new Keyframe(0f, animRootRotation.z)),
                    new AnimationCurve(new Keyframe(0f, animRootRotation.w))
                };

                var rootRotation = rootBone.rotation;

                float playback = 0f;
                float sampleRate = 1f / targetClip.frameRate;

                bool isInitialized = false;
                while (playback < targetClip.length)
                {
                    targetClip.SampleAnimation(character, playback);

                    for (int i = 0; i < rotations.Length; i++)
                    {
                        if (!isInitialized)
                        {
                            curves[i] = new AnimCurves()
                            {
                                tCurves = new []
                                {
                                    new AnimationCurve(),
                                    new AnimationCurve(),
                                    new AnimationCurve()
                                },
                                rCurves = new []
                                {
                                    new AnimationCurve(),
                                    new AnimationCurve(),
                                    new AnimationCurve(),
                                    new AnimationCurve()
                                },
                            };
                        }
                        
                        rotations[i] = new LocRot(bones[i + 1], true);
                    }

                    isInitialized = true;
                    rootBone.rotation *= Quaternion.Euler(offset);
                    
                    for (int i = 0; i < rotations.Length; i++)
                    {
                        var bone = bones[i + 1];
                        bone.rotation = rotations[i].rotation;
                        bone.position = rotations[i].position;
                        
                        curves[i].tCurves[0].AddKey(playback, bone.localPosition.x);
                        curves[i].tCurves[1].AddKey(playback, bone.localPosition.y);
                        curves[i].tCurves[2].AddKey(playback, bone.localPosition.z);
                        
                        curves[i].rCurves[0].AddKey(playback, bone.localRotation.x);
                        curves[i].rCurves[1].AddKey(playback, bone.localRotation.y);
                        curves[i].rCurves[2].AddKey(playback, bone.localRotation.z);
                        curves[i].rCurves[3].AddKey(playback, bone.localRotation.w);
                    }
                    
                    rootBone.rotation = rootRotation;
                    playback += sampleRate;
                }
                
                string posX = "m_LocalPosition.x";
                string posY = "m_LocalPosition.y";
                string posZ = "m_LocalPosition.z";
                    
                string rotX = "m_LocalRotation.x";
                string rotY = "m_LocalRotation.y";
                string rotZ = "m_LocalRotation.z";
                string posW = "m_LocalRotation.w";

                string path;
                for (int i = 1; i < bones.Length; i++)
                {
                    var sourceBone = bones[i];
                    path = AnimationUtility.CalculateTransformPath(sourceBone, character.transform);
                    
                    targetClip.SetCurve(path, typeof(Transform), posX, curves[i - 1].tCurves[0]);
                    targetClip.SetCurve(path, typeof(Transform), posY, curves[i - 1].tCurves[1]);
                    targetClip.SetCurve(path, typeof(Transform), posZ, curves[i - 1].tCurves[2]);
                    
                    targetClip.SetCurve(path, typeof(Transform), rotX, curves[i - 1].rCurves[0]);
                    targetClip.SetCurve(path, typeof(Transform), rotY, curves[i - 1].rCurves[1]);
                    targetClip.SetCurve(path, typeof(Transform), rotZ, curves[i - 1].rCurves[2]);
                    targetClip.SetCurve(path, typeof(Transform), posW, curves[i - 1].rCurves[3]);
                }
                
                path = AnimationUtility.CalculateTransformPath(rootBone, character.transform);
                
                targetClip.SetCurve(path, typeof(Transform), posX, rootCurves.tCurves[0]);
                targetClip.SetCurve(path, typeof(Transform), posY, rootCurves.tCurves[1]);
                targetClip.SetCurve(path, typeof(Transform), posZ, rootCurves.tCurves[2]);
                
                targetClip.SetCurve(path, typeof(Transform), rotX, rootCurves.rCurves[0]);
                targetClip.SetCurve(path, typeof(Transform), rotY, rootCurves.rCurves[1]);
                targetClip.SetCurve(path, typeof(Transform), rotZ, rootCurves.rCurves[2]);
                targetClip.SetCurve(path, typeof(Transform), posW, rootCurves.rCurves[3]);
            }
        }
    }
}
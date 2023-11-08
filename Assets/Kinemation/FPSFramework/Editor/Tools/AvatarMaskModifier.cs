using UnityEditor;
using UnityEngine;

namespace Kinemation.FPSFramework.Editor.Tools
{
    public class AvatarMaskModifier : IKinemationTool
    {
        private Transform _boneToAdd;
        private AvatarMask _maskToModify;
        
        public void Render()
        {
            EditorGUILayout.HelpBox("This tool adds a Transform to the Avatar Mask. " 
                                    + "Useful if you need to include a non-skeletal object in your mask.", 
                MessageType.Info);
            
            _boneToAdd =
                EditorGUILayout.ObjectField("Bone To Add", _boneToAdd, typeof(Transform), true)
                    as Transform;

            _maskToModify =
                EditorGUILayout.ObjectField("Upper Body Mask", _maskToModify, typeof(AvatarMask), true) 
                    as AvatarMask;

            if (_boneToAdd == null)
            {
                EditorGUILayout.HelpBox("Select the Bone transform", MessageType.Warning);
                return;
            }
            
            if (_maskToModify == null)
            {
                EditorGUILayout.HelpBox("Select the Avatar Mask", MessageType.Warning);
                return;
            }

            if (GUILayout.Button("Add Bone"))
            {
                for (int i = _maskToModify.transformCount - 1; i >= 0; i--)
                {
                    if (_maskToModify.GetTransformPath(i).EndsWith(_boneToAdd.name))
                    {
                        return;
                    }
                }

                _maskToModify.AddTransformPath(_boneToAdd, false);
                string path = _maskToModify.GetTransformPath(_maskToModify.transformCount - 1);
                int slashIndex = path.IndexOf("/");
                if (slashIndex >= 0)
                {
                    path = path.Substring(slashIndex + 1);
                }

                _maskToModify.SetTransformPath(_maskToModify.transformCount - 1, path);
            }
        }
    }
}
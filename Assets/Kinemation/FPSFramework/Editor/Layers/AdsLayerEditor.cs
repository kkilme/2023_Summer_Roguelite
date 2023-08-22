// Designed by KINEMATION, 2023

using Kinemation.FPSFramework.Runtime.Layers;
using UnityEditor;
using UnityEngine;

namespace Kinemation.FPSFramework.Editor.Layers
{
    [CustomEditor(typeof(AdsLayer), true)]
    public class AdsLayerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var layer = (AdsLayer) target;
            
            if (GUILayout.Button("Calculate Aim Data"))
            {
                layer.CalculateAimData();
            }
        }
    }
}
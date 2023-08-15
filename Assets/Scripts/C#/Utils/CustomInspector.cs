using UnityEditor;
using UnityEngine;
//#if UNITY_EDITOR
//[CustomEditor(typeof(GunData))]
//public class GunDataEditor : Editor
//{

//    private GUIStyle boldLabelStyle;

//    private void OnEnable()
//    {
//        // bold체
//        boldLabelStyle = new GUIStyle(EditorStyles.label);
//        boldLabelStyle.fontStyle = FontStyle.Bold;
//    }


//    public override void OnInspectorGUI()
//    {
//        GunData gunData = (GunData)target;

//        // 기본 Inspector 표시
//        DrawDefaultInspector();

//        // 한 줄 공백
//        EditorGUILayout.Space();
//        // attachments 딕셔너리 내용 표시
//        EditorGUILayout.LabelField("Attachments", boldLabelStyle);
//        foreach (var attachment in gunData.attachments)
//        {
//            EditorGUILayout.LabelField(attachment.Key.ToString(), attachment.Value?.name ?? "null");
//        }
//    }
//}
//#endif
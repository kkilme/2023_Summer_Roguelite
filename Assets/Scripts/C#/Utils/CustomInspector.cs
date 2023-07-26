using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GunData))]
public class GunDataEditor : Editor
{

    private GUIStyle boldLabelStyle;

    private void OnEnable()
    {
        // boldü
        boldLabelStyle = new GUIStyle(EditorStyles.label);
        boldLabelStyle.fontStyle = FontStyle.Bold;
    }


    public override void OnInspectorGUI()
    {
        GunData gunData = (GunData)target;

        // �⺻ Inspector ǥ��
        DrawDefaultInspector();

        // �� �� ����
        EditorGUILayout.Space();
        // attachments ��ųʸ� ���� ǥ��
        EditorGUILayout.LabelField("Attachments", boldLabelStyle);
        foreach (var attachment in gunData.attachments)
        {
            EditorGUILayout.LabelField(attachment.Key.ToString(), attachment.Value?.name ?? "null");
        }
    }
}
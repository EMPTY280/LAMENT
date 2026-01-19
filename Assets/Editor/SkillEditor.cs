using UnityEditor;

[CustomEditor(typeof(LAMENT.Skill), true)]
public class SkillEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(7);
        EditorGUILayout.LabelField("주석");

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextArea(((LAMENT.Skill)target).Comment);
        EditorGUI.EndDisabledGroup();
    }
}

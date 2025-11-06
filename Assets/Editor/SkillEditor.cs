using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Skill), true)]
public class SkillEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(7);
        EditorGUILayout.LabelField("¡÷ºÆ");

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextArea(((Skill)target).Comment);
        EditorGUI.EndDisabledGroup();
    }
}

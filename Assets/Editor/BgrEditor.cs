using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ParallaxBackground))]
public class BgrEditor : Editor
{
    private float scrollSpeedBase = 0;
    private float scrollSpeedStep = 0;
    private SerializedProperty layers;


    private void OnEnable()
    {
        layers = serializedObject.FindProperty("layers");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(7);
        GUILayout.Label("===== 에디터 =====", EditorStyles.boldLabel);

        scrollSpeedBase = EditorGUILayout.FloatField("스크롤 속도 베이스", scrollSpeedBase);
        scrollSpeedStep = EditorGUILayout.FloatField("스크롤 속도 편차", scrollSpeedStep);
        if (GUILayout.Button("값 자동 설정"))
            AutoLayering();
    }

    private void AutoLayering()
    {
        int size = layers.arraySize;
        for (int i = 0; i < size; i++)
        {
            SerializedProperty scrSpd = layers.GetArrayElementAtIndex(i).FindPropertyRelative("scrollSpeed");
            scrSpd.floatValue = scrollSpeedBase + scrollSpeedStep * i;
        }
        serializedObject.ApplyModifiedProperties();
    }
}

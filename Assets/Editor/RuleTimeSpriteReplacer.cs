using UnityEditor;
using UnityEngine;
using System.Linq;

public class RuleTileSpriteReplacer : EditorWindow
{
    private RuleTile ruleTile;
    private Texture2D newSpriteTexture;

    [MenuItem("Tools/RuleTile/Sprite 일괄 교체")]
    public static void Open()
    {
        GetWindow<RuleTileSpriteReplacer>("RuleTile Sprite Replacer");
    }

    private void OnGUI()
    {
        ruleTile = (RuleTile)EditorGUILayout.ObjectField(
            "RuleTile",
            ruleTile,
            typeof(RuleTile),
            false
        );

        newSpriteTexture = (Texture2D)EditorGUILayout.ObjectField(
            "새 Sprite Sheet",
            newSpriteTexture,
            typeof(Texture2D),
            false
        );

        GUI.enabled = ruleTile != null && newSpriteTexture != null;

        if (GUILayout.Button("스프라이트 교체"))
        {
            ReplaceSprites();
        }

        GUI.enabled = true;
    }

    private void ReplaceSprites()
    {
        // 새 스프라이트 배열 로드
        string path = AssetDatabase.GetAssetPath(newSpriteTexture);
        Sprite[] newSprites = AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<Sprite>()
            .OrderBy(s =>
            {
                string[] split = s.name.Split('_');
                return int.Parse(split[split.Length - 1]);
            })
            .ToArray();

        var rules = ruleTile.m_TilingRules;

        if (rules.Count != newSprites.Length)
        {
            Debug.LogError($"Rule 수({rules.Count})와 Sprite 수({newSprites.Length})가 다릅니다.");
            return;
        }

        ruleTile.m_DefaultSprite = newSprites[0];

        Undo.RecordObject(ruleTile, "Replace RuleTile Sprites");

        for (int i = 0; i < rules.Count; i++)
        {
            var rule = rules[i];

            // Single 출력 기준
            rule.m_Sprites[0] = newSprites[i];
        }

        EditorUtility.SetDirty(ruleTile);
        AssetDatabase.SaveAssets();

        Debug.Log("RuleTile 스프라이트 교체 완료");
    }
}
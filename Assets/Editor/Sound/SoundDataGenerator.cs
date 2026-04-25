using System.IO;
using LAMENT;
using UnityEditor;
using UnityEngine;

public static class SoundDataGenerator
{
    private const string ROOT_PATH = "Assets/Resources/Sounds";

    [MenuItem("Tools/Sound/Generate SoundData From Clips")]
    public static void Generate()
    {
        if (!Directory.Exists(ROOT_PATH))
        {
            Directory.CreateDirectory(ROOT_PATH);
            AssetDatabase.Refresh();
        }

        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { ROOT_PATH });

        int createdCount = 0;
        int skippedCount = 0;

        for (int i = 0; i < guids.Length; i++)
        {
            string clipPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);

            if (!clip)
                continue;

            string directory = Path.GetDirectoryName(clipPath);
            string fileName = Path.GetFileNameWithoutExtension(clipPath);
            string dataPath = $"{directory}/{fileName}_SoundData.asset";

            SoundData existing = AssetDatabase.LoadAssetAtPath<SoundData>(dataPath);

            if (existing)
            {
                skippedCount++;
                continue;
            }

            SoundData data = ScriptableObject.CreateInstance<SoundData>();

            SerializedObject so = new SerializedObject(data);

            so.FindProperty("id").stringValue = CreateSoundId(clipPath, fileName);
            so.FindProperty("clip").objectReferenceValue = clip;
            so.FindProperty("category").enumValueIndex = (int)GetCategoryByPath(clipPath);
            so.FindProperty("loop").boolValue = GetCategoryByPath(clipPath) == ESoundCategory.BGM;
            so.FindProperty("volume").floatValue = 1f;
            so.FindProperty("overlapPolicy").enumValueIndex = (int)GetDefaultOverlapPolicy(clipPath);
            so.FindProperty("overlapLimit").intValue = 3;
            so.FindProperty("cooldown").floatValue = GetDefaultCooldown(clipPath);

            so.ApplyModifiedProperties();

            AssetDatabase.CreateAsset(data, dataPath);
            createdCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[SOUND] SoundData 생성 완료 / 생성: {createdCount}, 스킵: {skippedCount}");
    }

    private static string CreateSoundId(string path, string fileName)
    {
        ESoundCategory category = GetCategoryByPath(path);

        string prefix = "";

        switch (category)
        {
            case ESoundCategory.BGM:
                prefix = "BGM";
                break;

            case ESoundCategory.SFX:
                prefix = "SFX";
                break;

            case ESoundCategory.UI:
                prefix = "UI";
                break;

            case ESoundCategory.Ambient:
                prefix = "AMB";
                break;
        }

        string normalizedName = fileName.ToUpper().Replace(" ", "_").Replace("-", "_");

        if (normalizedName.StartsWith(prefix + "_"))
            return normalizedName;

        return $"{prefix}_{normalizedName}";
    }

    private static ESoundCategory GetCategoryByPath(string path)
    {
        string upper = path.ToUpper();

        if (upper.Contains("/BGM/"))
            return ESoundCategory.BGM;

        if (upper.Contains("/UI/"))
            return ESoundCategory.UI;

        if (upper.Contains("/AMBIENT/"))
            return ESoundCategory.Ambient;

        return ESoundCategory.SFX;
    }

    private static ESoundOverlapPolicy GetDefaultOverlapPolicy(string path)
    {
        ESoundCategory category = GetCategoryByPath(path);

        switch (category)
        {
            case ESoundCategory.BGM:
                return ESoundOverlapPolicy.RestartSame;

            case ESoundCategory.UI:
                return ESoundOverlapPolicy.IgnoreIfPlaying;

            case ESoundCategory.Ambient:
                return ESoundOverlapPolicy.IgnoreIfPlaying;

            default:
                return ESoundOverlapPolicy.LimitCount;
        }
    }

    private static float GetDefaultCooldown(string path)
    {
        ESoundCategory category = GetCategoryByPath(path);

        switch (category)
        {
            case ESoundCategory.UI:
                return 0.05f;

            case ESoundCategory.SFX:
                return 0.03f;

            default:
                return 0f;
        }
    }
}
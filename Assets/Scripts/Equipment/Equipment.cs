using System;
using UnityEngine;

/// <summary>
/// 장비 = 사지 (팔다리)
/// </summary>
[Serializable]
public abstract class Equipment : ScriptableObject
{
    [Header("장비")]
    [SerializeField]
    [Tooltip("아이콘")]
    private Sprite skillIcon;
    [SerializeField]
    [Tooltip("배정된 스킬")]
    private Skill[] skills;
    [SerializeField]
    [Tooltip("재사용 대기시간")]
    private float cooldown = 0.5f;

    public Sprite SkillIcon => skillIcon;
    public Skill[] Skills => skills;
    public float Cooldown => cooldown;
}

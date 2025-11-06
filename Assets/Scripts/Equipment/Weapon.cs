using UnityEngine;


/// <summary>
/// 무기 = 팔
/// </summary>
[CreateAssetMenu(fileName = "New Weapon", menuName = "ScriptableObjects/Weapon")]
public class Weapon : Equipment
{
    [Header("무기")]
    [SerializeField]
    [Tooltip("폭파 스킬")]
    private Skill burstSkill;
    [SerializeField]
    [Tooltip("폭파 아이콘")]
    private Sprite burstSkillIcon;

    public Skill BurstSkill => burstSkill;
    public Sprite BurstSkillIcon => burstSkillIcon;

    // TODO: 이 무기의 외형
}

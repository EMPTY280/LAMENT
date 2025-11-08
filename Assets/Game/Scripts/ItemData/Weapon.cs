using UnityEngine;


namespace LAMENT
{
    /// <summary> Weapon == Arm </summary>
    [CreateAssetMenu(fileName = "New Weapon", menuName = "ScriptableObjects/Weapon")]
    public class Weapon : EquipmentData
    {
        [Header("Weapon")]
        [SerializeField]
        private Skill burstSkill;
        [SerializeField]
        private Sprite burstSkillIcon;

        public Skill BurstSkill => burstSkill;
        public Sprite BurstSkillIcon => burstSkillIcon;


    }
}
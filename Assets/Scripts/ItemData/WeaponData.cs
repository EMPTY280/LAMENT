using System.Collections.Generic;
using UnityEngine;


namespace LAMENT
{
    /// <summary> Weapon == Arm </summary>
    [CreateAssetMenu(fileName = "New Weapon", menuName = "ScriptableObjects/Weapon")]
    public class WeaponData : EquipmentData
    {
        [Header("Weapon")]
        [SerializeField] private Skill burstSkill;
        [SerializeField] private Sprite burstSkillIcon;

        public Skill BurstSkill => burstSkill;
        public Sprite BurstSkillIcon => burstSkillIcon;

        public bool TryGetOffensiveSkill(EOffensiveSkillType type, out OffensiveSkill offensiveSkill)
        {
            Skill[] skills = Skills;

            if (skills != null)
            {
                for (int i = 0; i < skills.Length; i++)
                {
                    if (skills[i] is OffensiveSkill candidate && candidate.OffensiveType == type)
                    {
                        offensiveSkill = candidate;
                        return true;
                    }
                }
            }

            if (burstSkill is OffensiveSkill burstCandidate && burstCandidate.OffensiveType == type)
            {
                offensiveSkill = burstCandidate;
                return true;
            }

            offensiveSkill = null;
            return false;
        }

        
        [Header("Player Animation")]
        [SerializeField] private Sprite sprIdle;
        [SerializeField] private List<Sprite> sprDelay;
        
        [SerializeField] private List<Sprite> sprAttack;

        public Sprite SprIdle => sprIdle; 
        public List<Sprite> SprDelay => sprDelay;
        public List<Sprite> SprAttack => sprAttack;
    }
}

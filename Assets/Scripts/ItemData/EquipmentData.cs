using System;
using UnityEngine;

namespace LAMENT
{
    /// <summary> Equipment == Limbs (Arms / Legs) </summary>
    [Serializable]
    public abstract class EquipmentData : ItemData
    {
        [Header("Equipmnet")]
        [SerializeField]
        [Tooltip("Skill icon")]
        private Sprite skillIcon;

        [SerializeField]
        [Tooltip("배정된 스킬")]
        private Skill[] skills;

        [SerializeField]
        [Tooltip("Cooldown")]
        private float cooldown = 0.5f;

        public EEquipSlotType Slot; // TODO: 나중에 없애야됨

        public Sprite SkillIcon => skillIcon;
        public Skill[] Skills => skills;
        public float Cooldown => cooldown;

        [Header("Sound")]
        [SerializeField]
        [Tooltip("기본 공격 사운드")]
        private string attackSoundId = "";

        public string AttackSoundId => attackSoundId;
    }
}
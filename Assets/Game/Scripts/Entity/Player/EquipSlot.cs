using System;
using UnityEngine;

namespace LAMENT
{
    public enum EEquipSlotType
    {
        LEFT,
        RIGHT,
        LEG
    }

    [Serializable]
    public class EquipSlot
    {
        [SerializeField] private EquipmentData equipment;
        [SerializeField] private EEquipSlotType type;

        public EquipmentData Equipment
        {
            get => equipment;
            set => equipment = value;
        }
        public EEquipSlotType Type => type;

        private float cooldownCurr = 0;
        public float Cooldown => equipment.Cooldown;


        public void UpdateCooldown(float dt)
        {
            if (Cooldown <= 0)
                return;

            cooldownCurr -= dt;
        }

        public void StartCooldown()
        {
            cooldownCurr = Cooldown;
        }

        public bool IsReady() => cooldownCurr <= 0;
    }
}
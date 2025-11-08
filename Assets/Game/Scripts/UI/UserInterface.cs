using UnityEngine;

namespace LAMENT
{
    public class UserInterface : MonoBehaviour // TODO: юс╫ц
    {
        [SerializeField] private CooldownBox[] cooldownBoxes;

        private void Awake()
        {
            GameManager.Eventbus.Subscribe<GEOnEquipmentEquipped>(OnPlayerEquipmentChanged);
            GameManager.Eventbus.Subscribe<GEOnSkillFinished>(OnPlayerSkillFinished);
        }

        private void OnDestroy()
        {
            GameManager.Eventbus.Subscribe<GEOnEquipmentEquipped>(OnPlayerEquipmentChanged);
            GameManager.Eventbus.Unsubscribe<GEOnSkillFinished>(OnPlayerSkillFinished);
        }

        public void OnPlayerEquipmentChanged(GEOnEquipmentEquipped e)
        {
            EquipmentData equipment = e.Equipped;

            switch (e.SlotType)
            {
                case EEquipSlotType.LEFT:
                    if (equipment)
                    {
                        cooldownBoxes[1].SetIcon(equipment.SkillIcon);
                        cooldownBoxes[3].SetIcon(((Weapon)equipment).BurstSkillIcon);
                    }
                    else
                    {
                        cooldownBoxes[1].SetIcon(null);
                        cooldownBoxes[3].SetIcon(null);
                    }
                    break;
                case EEquipSlotType.RIGHT:
                    if (equipment)
                    {
                        cooldownBoxes[2].SetIcon(equipment.SkillIcon);
                        cooldownBoxes[4].SetIcon(((Weapon)equipment).BurstSkillIcon);
                    }
                    else
                    {
                        cooldownBoxes[2].SetIcon(null);
                        cooldownBoxes[4].SetIcon(null);
                    }
                    break;
                case EEquipSlotType.LEG:
                    if (equipment)
                        cooldownBoxes[0].SetIcon(equipment.SkillIcon);
                    else
                        cooldownBoxes[0].SetIcon(null);
                    break;
            }
        }

        public void OnPlayerSkillFinished(GEOnSkillFinished e)
        {
            EEquipSlotType equipSlot = e.Slot.Type;

            switch (equipSlot)
            {
                case EEquipSlotType.LEFT:
                    cooldownBoxes[1].SetCooldown(e.Slot.Equipment.Cooldown);
                    break;
                case EEquipSlotType.RIGHT:
                    cooldownBoxes[2].SetCooldown(e.Slot.Equipment.Cooldown);
                    break;
                case EEquipSlotType.LEG:
                    cooldownBoxes[0].SetCooldown(e.Slot.Equipment.Cooldown);
                    break;
            }
        }
    }
}
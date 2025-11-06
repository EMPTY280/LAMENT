using UnityEngine;

public class UserInterface : MonoBehaviour
{
    [SerializeField] private CooldownBox[] cooldownBoxes;

    private void Awake()
    {
        OnEquipmentChanged.Event += OnPlayerEquipmentChanged;
        OnSkillFinished.Event += OnPlayerSkillFinished;
    }

    public void OnPlayerEquipmentChanged(EquipSlot slot)
    {
        Equipment equipment = slot.Equipment;

        switch (slot.Type)
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

    public void OnPlayerSkillFinished(EquipSlot slot)
    {
        Equipment equipment = slot.Equipment;

        switch (slot.Type)
        {
            case EEquipSlotType.LEFT:
                cooldownBoxes[1].SetCooldown(equipment.Cooldown);
                break;
            case EEquipSlotType.RIGHT:
                cooldownBoxes[2].SetCooldown(equipment.Cooldown);
                break;
            case EEquipSlotType.LEG:
                cooldownBoxes[0].SetCooldown(equipment.Cooldown);
                break;
        }
    }
}

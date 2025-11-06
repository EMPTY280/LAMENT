using System;
using UnityEngine;

public enum EEquipSlotType
{
    LEFT,
    RIGHT,
    LEG
}

[Serializable]
public class EquipSlot
{
    [SerializeField] private Equipment equipment;
    [SerializeField] private EEquipSlotType type;

    public Equipment Equipment
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

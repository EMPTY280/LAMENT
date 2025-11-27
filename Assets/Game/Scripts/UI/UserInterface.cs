using UnityEngine;

namespace LAMENT
{
    public class UserInterface : MonoBehaviour // TODO: �ӽ�
    {
        [SerializeField] private CooldownBox[] cooldownBoxes;

        [Header("체력 / 위 게이지")]
        [SerializeField] private HeartIcon[] heartIcons; // 최대 5개
        [SerializeField] private StomachBar stomachBar;

        private void Awake()
        {
            GameManager.Eventbus.Subscribe<GEOnEquipmentEquipped>(OnPlayerEquipmentChanged);
            GameManager.Eventbus.Subscribe<GEOnSkillFinished>(OnPlayerSkillFinished);

            GameManager.Eventbus.Subscribe<GEOnPlayerHealthChanged>(OnPlayerHealthChanged);
            GameManager.Eventbus.Subscribe<GEOnStomachGaugeChanged>(OnStomachGaugeChanged);
        }

        private void OnDestroy()
        {
            GameManager.Eventbus.Subscribe<GEOnEquipmentEquipped>(OnPlayerEquipmentChanged);
            GameManager.Eventbus.Unsubscribe<GEOnSkillFinished>(OnPlayerSkillFinished);
        
            GameManager.Eventbus.Unsubscribe<GEOnPlayerHealthChanged>(OnPlayerHealthChanged);
            GameManager.Eventbus.Unsubscribe<GEOnStomachGaugeChanged>(OnStomachGaugeChanged);

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

        #region 체력 / 위 게이지 UI

        private void OnPlayerHealthChanged(GEOnPlayerHealthChanged e)
        {
            if (heartIcons == null || heartIcons.Length == 0)
                return;

            for (int i = 0; i < heartIcons.Length; i++)
            {
                var icon = heartIcons[i];
                if (icon == null) continue;

                bool visible = i < e.InitialMaxHp;
                bool filled = i < e.CurrentHp;
                bool inMaxRange = i < e.CurrentMaxHp;

                icon.SetVisible(visible);

                if (visible)
                {
                    icon.SetFilled(filled);
                }
            }
        }

        private void OnStomachGaugeChanged(GEOnStomachGaugeChanged e)
        {
            if (stomachBar == null)
                return;

            float ratio = (float)e.Current / e.Max;
            stomachBar.SetFill(ratio);
        }

        #endregion
    }
}
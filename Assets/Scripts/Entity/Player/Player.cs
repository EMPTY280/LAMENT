using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Player : Entity
{
    [Header("장비")]
    [SerializeField] private EquipSlot leftArmSlot;
    [SerializeField] private EquipSlot rightArmSlot;
    [SerializeField] private EquipSlot legSlot;

    private EquipSlot lastUsedEquipment; // 마지막으로 사용한 장비 슬롯

    public EquipSlot LeftArmSlot => leftArmSlot;
    public EquipSlot RightArmSlot => rightArmSlot;
    public EquipSlot LegSlot => legSlot;



    protected override void Awake()
    {
        base.Awake();

        // 이미 배정된 장비가 있다면 이펙터 생성 후 등록
        TryCreateEffector(leftArmSlot.Equipment, true);
        TryCreateEffector(rightArmSlot.Equipment, true);
        TryCreateEffector(legSlot.Equipment);
    }

    private void Start()
    {
        OnEquipmentChanged.Notify(leftArmSlot);
        OnEquipmentChanged.Notify(rightArmSlot);
        OnEquipmentChanged.Notify(legSlot);
    }

    protected override void Update()
    {
        base.Update();

        if (leftArmSlot.Equipment)
            leftArmSlot.UpdateCooldown(Time.deltaTime);
        if (rightArmSlot.Equipment)
            rightArmSlot.UpdateCooldown(Time.deltaTime);
        if (legSlot.Equipment)
            legSlot.UpdateCooldown(Time.deltaTime);
    }

    #region 장비

    /// <summary> 장비 사용 시도 </summary>
    public bool TryUseEquipment(EquipSlot slot, Skill skill, Action cbOnSkillEnd = null, bool isBurst = false)
    {
        if (!slot.IsReady() && !isBurst)
            return false;

        lastUsedEquipment = slot;
        StartSkill(skill, cbOnSkillEnd);

        // 폭파 스킬이면 즉시 장비 파괴
        if (isBurst)
        {
            slot.Equipment = null;
            OnEquipmentChanged.Notify(slot);
        }

        return true;
    }

    /// <summary> 스킬 사용 종료 </summary>
    public void FinishSkill()
    {
        TrySetCooldown();
    }

    /// <summary> 마지막으로 사용한 장비에 쿨다운 적용 후 null </summary>
    protected bool TrySetCooldown()
    {
        if (lastUsedEquipment == null)
            return false;

        // 폭파 스킬이면 제거됨
        if (!lastUsedEquipment.Equipment)
            return false;

        OnSkillFinished.Notify(lastUsedEquipment);

        lastUsedEquipment.StartCooldown();
        lastUsedEquipment = null;

        return true;
    }

    /// <summary> 장비 이펙터 생성 시도 </summary>
    protected bool TryCreateEffector(Equipment e, bool isWeapon = false)
    {
        if (!e)
            return false;

        if (e.Skills == null || e.Skills.Length <= 0)
            return false;

        void TryCreateFromSkill(Skill skill)
        {
            if (!skill)
            {
                GameManager.Logger.LogError("배정된 스킬이 없습니다.");
                return;
            }

            if (!skill.Effector)
                return;

            if (Effectors.ContainsKey(skill.Effector.name))
                return;

            SkillEffector eff;
            if (!Instantiate(skill.Effector, transform).TryGetComponent(out eff))
            {
                GameManager.Logger.LogError("이펙터에서 컴포넌트를 찾을 수 없습니다.");
                return;
            }

            Effectors.Add(skill.Effector.name, eff);
        }

        for (int i = 0; i < e.Skills.Length; i++)
            TryCreateFromSkill(e.Skills[i]);

        if (isWeapon)
            TryCreateFromSkill(((Weapon)e).BurstSkill);

        return true;
    }

    #endregion
}

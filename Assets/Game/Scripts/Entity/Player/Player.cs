using System;
using UnityEngine;

namespace LAMENT
{
    public class Player : Entity
    {
        [Header("���")]
        [SerializeField] private EquipSlot leftArmSlot;
        [SerializeField] private EquipSlot rightArmSlot;
        [SerializeField] private EquipSlot legSlot;

        [Header("장기")]
        [SerializeField] private PlayerGutsController gutsController;

        private EquipSlot lastUsedEquipment; // ���������� ����� ��� ����
        
        private PlayerHealth health;
        public EquipSlot LeftArmSlot => leftArmSlot;
        public EquipSlot RightArmSlot => rightArmSlot;
        public EquipSlot LegSlot => legSlot;

          public PlayerGutRuntime GutRuntime => gutsController != null ? gutsController.Runtime : null;

         private readonly GutData[] appliedGuts = new GutData[(int)EGutType._LENGTH];



        protected override void Awake()
        {
            base.Awake();

            if (!gutsController)
                gutsController = GetComponent<PlayerGutsController>();



            // �̹� ������ ��� �ִٸ� ������ ���� �� ���
            health = GetComponent<PlayerHealth>();
            TryCreateEffector(leftArmSlot.Equipment, true);
            TryCreateEffector(rightArmSlot.Equipment, true);
            TryCreateEffector(legSlot.Equipment);

            GameManager.Eventbus.Subscribe<GEOnEquipmentEquipped>(OnPlayerEquipmentChanged);
        }

        private void Start()
        {
            // TODO: �κ��� ����ǰ� �����Ұ�

            GameManager.Eventbus.Publish(new GEOnEquipmentEquipped(
                leftArmSlot.Equipment,
                null,
                EEquipSlotType.LEFT));

            GameManager.Eventbus.Publish(new GEOnEquipmentEquipped(
                rightArmSlot.Equipment,
                null,
                EEquipSlotType.RIGHT));

            GameManager.Eventbus.Publish(new GEOnEquipmentEquipped(
                legSlot.Equipment,
                null,
                EEquipSlotType.LEG));
        }


        protected override void OnDestroy()
        {
            base.Awake();
            GameManager.Eventbus.Unsubscribe<GEOnEquipmentEquipped>(OnPlayerEquipmentChanged);
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

            Animator.SetBool("IsGrounded", MoveComponent.IsGrounded);
            Animator.SetFloat("HSpeedMagnitude", Math.Abs(MoveComponent.HSpeed));
            Animator.SetFloat("VSpeed", MoveComponent.VSpeed);
        }

        #region 스킬 및 장비

        /// <summary> ��� ��� �õ� </summary>
        public bool TryUseEquipment(EquipSlot slot, Skill skill, Action cbOnSkillEnd = null, bool isBurst = false)
        {
            if (!slot.IsReady() && !isBurst)
                return false;

            lastUsedEquipment = slot;
            StartSkill(skill, cbOnSkillEnd);

            // ���� ��ų�̸� ��� ��� �ı�
            if (isBurst)
            {
                Debug.Log("���߿� ��� �ı� ����"); // TODO
                // slot.Equipment = null;
                // OnEquipmentChanged.Notify(slot);
                 TryConsumeBurstEquipment(slot);
            }

            return true;
        }

         private void TryConsumeBurstEquipment(EquipSlot slot)
        {
            if (slot == null || slot.Equipment == null)
                return;

            if (GutRuntime != null && GutRuntime.RollPreventConsume())
            {
                Debug.Log("[GUT][PLAY][BURST] consume prevented");
                return;
            }

            Debug.Log($"[GUT][PLAY][BURST] consumed = {slot.Equipment.name}");

            EquipmentData replaced = slot.Equipment;
            slot.Equipment = null;

            EEquipSlotType slotType = EEquipSlotType.LEG;
            if (slot == leftArmSlot) slotType = EEquipSlotType.LEFT;
            else if (slot == rightArmSlot) slotType = EEquipSlotType.RIGHT;
            else if (slot == legSlot) slotType = EEquipSlotType.LEG;

            GameManager.Eventbus.Publish(new GEOnEquipmentEquipped(
                null,
                replaced,
                slotType));
        }

        /// <summary> ��ų ��� ���� </summary>
        public void FinishSkill()
        {
            TrySetCooldown();
        }

        /// <summary> ���������� ����� ��� ��ٿ� ���� �� null </summary>
        protected bool TrySetCooldown()
        {
            if (lastUsedEquipment == null)
                return false;

            // ���� ��ų�̸� ���ŵ�
            if (!lastUsedEquipment.Equipment)
                return false;

            GameManager.Eventbus.Publish(new GEOnSkillFinished(lastUsedEquipment));

            lastUsedEquipment.StartCooldown();
            lastUsedEquipment = null;

            return true;
        }

        /// <summary> 스킬 이펙터 생성 시도 </summary>
        protected bool TryCreateEffector(EquipmentData e, bool isWeapon = false)
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
                if (!Instantiate(skill.Effector, effectorRoot ?? transform).TryGetComponent(out eff))
                {
                    GameManager.Logger.LogError("스킬 이펙터 프리팹에서 스크립트를 찾을 수 없습니다.");
                    return;
                }

                eff.transform.localPosition = Vector3.zero;
                eff.SetOwner(this);
                eff.CB_OnHitTarget = OnHitTarget;
                Effectors.Add(skill.Effector.name, eff);
            }

            for (int i = 0; i < e.Skills.Length; i++)
                TryCreateFromSkill(e.Skills[i]);

            if (isWeapon)
                TryCreateFromSkill(((Weapon)e).BurstSkill);

            return true;
        }

        #endregion
    
        public void OnPlayerEquipmentChanged(GEOnEquipmentEquipped e)
        {
            switch (e.SlotType)
            {
                case EEquipSlotType.LEFT:
                    leftArmSlot.Equipment = e.Equipped;
                break;
                case EEquipSlotType.RIGHT:
                    rightArmSlot.Equipment = e.Equipped;
                break;
                case EEquipSlotType.LEG:
                    legSlot.Equipment = e.Equipped;
                break;
            }
        }

        #region 타격 및 피격

        public override void OnDamageHandled(DamageResponse rsp)
        {
            if (health == null)
                return;

            // 한 번 맞으면 하트 1개 감소
            health.TakeHit(1);
        }

        private void OnHitTarget(IHittable target)
        {
            if (health != null)
            {
                // 적을 맞출 때마다 위 게이지 증가
                health.OnAttackLanded();
            }
        }

        #endregion

    }
}
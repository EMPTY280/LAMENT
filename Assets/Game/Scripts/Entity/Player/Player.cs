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

        private EquipSlot lastUsedEquipment; // ���������� ����� ��� ����
        
        private PlayerHealth health;
        public EquipSlot LeftArmSlot => leftArmSlot;
        public EquipSlot RightArmSlot => rightArmSlot;
        public EquipSlot LegSlot => legSlot;



        protected override void Awake()
        {
            base.Awake();

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
            }

            return true;
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
                    GameManager.Logger.LogError("������ ��ų�� �����ϴ�.");
                    return;
                }

                if (!skill.Effector)
                    return;

                if (Effectors.ContainsKey(skill.Effector.name))
                    return;

                SkillEffector eff;
                if (!Instantiate(skill.Effector, effectorRoot ?? transform).TryGetComponent(out eff))
                {
                    GameManager.Logger.LogError("�����Ϳ��� ������Ʈ�� ã�� �� �����ϴ�.");
                    return;
                }

                eff.transform.localPosition = Vector3.zero;
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

        public override void OnDamageHandled(Entity src)
        {
            if (health == null)
                return;

            // 한 번 맞으면 하트 1개 감소
            health.TakeHit(1);
        }

        public override void OnHitTarget(IDamageable target)
        {
            base.OnHitTarget(target);

            if (health != null)
            {
                // 적을 맞출 때마다 위 게이지 증가
                health.OnAttackLanded();
            }
        }

        #endregion
    }
}
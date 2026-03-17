using System;
using Unity.Mathematics;
using UnityEngine;

namespace LAMENT
{
    public class Player : Entity
    {
        // ===== 체력 =====
        [Header("최대 체력 감소")]
        [SerializeField] private int hpDecay = 0; // 줄어든 체력

        [Header("장비 슬롯")] // =====
        [SerializeField] public EquipSlot leftArmSlot;
        [SerializeField] private EquipSlot rightArmSlot;
        [SerializeField] private EquipSlot legSlot;

        public EquipSlot LeftArmSlot => leftArmSlot;
        public EquipSlot RightArmSlot => rightArmSlot;
        public EquipSlot LegSlot => legSlot;

        private EquipSlot lastUsedEquipment; // 마지막으로 사용된 장비

        // ===== 위 게이지 =====
        private float energyMax = 100;
        private float energyCurr = 0;
        private float energyGainPerHit = 5;

        // ===== 추가 능력치 =====
        private float energyMult = 1.0f;
        private float consumeChance = 1.0f;


        protected override void Awake()
        {
            base.Awake();

            InitEquipments();
            InitEvents();
            InitGuts();
        }

        private void Start()
        {
            PublishInitStates();
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
        }

        #region 초기화

        /// <summary> 초기 장착된 장비 준비 </summary>
        private void InitEquipments()
        {
            TryCreateEffector(leftArmSlot.Equipment, true);
            TryCreateEffector(rightArmSlot.Equipment, true);
            TryCreateEffector(legSlot.Equipment);
        }

        /// <summary> 이벤트 등록 </summary>
        private void InitEvents()
        {
            GameManager.Eventbus.Subscribe<GEOnEquipmentEquipped>(OnPlayerEquipmentChanged);
        }

        /// <summary> 초기 상태 전파 </summary>
        private void PublishInitStates()
        {
            GameManager.Eventbus.Publish(new GEOnPlayerHealthChanged((int)hpCurr, (int)hpMax, 0, 0));
            GameManager.Eventbus.Publish(new GEOnPlayerEnergyChanged(energyCurr, energyMax));

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

        private void InitGuts()
        {
            for (int i = 0; i < (int)EGutType._LENGTH; i++)
            {
                GutData data = GameManager.Player.GetGutData((EGutType)i);
                if (!data)
                    continue;
                
                foreach (GutEffectData eff in data.Effects)
                    eff.Apply(this);
            }
        }

        #endregion

        #region 스킬 및 장비

        /// <summary> 스킬 사용 </summary>
        public bool TryUseEquipment(EquipSlot slot, Skill skill, Action cbOnSkillEnd = null, bool isBurst = false)
        {
            if (!slot.IsReady() && !isBurst)
                return false;

            lastUsedEquipment = slot;
            TryStartSkill(skill, cbOnSkillEnd);

            GameManager.Eventbus.Publish(new GEOnPlayerUsedEquiment(slot.Type, lastUsedEquipment.Equipment, skill));

            // 폭파 스킬이었다면 파괴 판정
            if (isBurst && BurstRoll())
                BurstEquipment(slot);

            return true;
        }

        /// <summary> 장비 파괴 확률 판정 </summary>
        public bool BurstRoll()
        {
            return UnityEngine.Random.value < consumeChance;
        }

        /// <summary> 장비 파괴 판정 </summary>
        private void BurstEquipment(EquipSlot slot)
        {
            if (slot == null || slot.Equipment == null)
                return;

            EquipmentData replaced = slot.Equipment;
            slot.Equipment = null;

            EEquipSlotType slotType = EEquipSlotType.LEG;
            if (slot == leftArmSlot)
                slotType = EEquipSlotType.LEFT;
            else if (slot == rightArmSlot)
                slotType = EEquipSlotType.RIGHT;
            else if (slot == legSlot)
                slotType = EEquipSlotType.LEG;

            // 장비 교환 이벤트
            GameManager.Eventbus.Publish(new GEOnEquipmentEquipped(
                null,
                replaced,
                slotType));
        }

        /// <summary> 스킬 사용 종료 시 호출 </summary>
        public void FinishSkill()
        {
            TrySetCooldown();
        }

        protected bool TrySetCooldown()
        {
            if (lastUsedEquipment == null)
                return false;

            if (!lastUsedEquipment.Equipment)
                return false;

            GameManager.Eventbus.Publish(new GEOnPlayerSkillFinished(lastUsedEquipment));

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

            for (int i = 0; i < e.Skills.Length; i++)
                TryAddEffector(e.Skills[i]);

            if (isWeapon)
                TryAddEffector(((WeaponData)e).BurstSkill);

            return true;
        }

        private void OnPlayerEquipmentChanged(GEOnEquipmentEquipped e)
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

        #endregion

        #region 에너지

        /// <summary> 위 게이지 획득 </summary>
        /// <param name="isRelative"> true면 +-, false면 즉시 지정 </param>
        public void SetEnergy(float amount, bool isRelative)
        {
            if (isRelative)
                energyCurr = math.min(energyMax, energyCurr + amount);
            else
                energyCurr = math.min(energyMax, amount);

            GameManager.Eventbus.Publish(new GEOnPlayerEnergyChanged(energyCurr, energyMax));

            TryRestoreDecay();
        }

        public void ClearEnergy()
        {
            energyCurr = 0;

            GameManager.Eventbus.Publish(new GEOnPlayerEnergyChanged(energyCurr, energyMax));
        }

        #endregion

        #region 체력 

        protected override void TakeDamage(DamageResponse rsp)
        {
            hpCurr--;

            if (hpCurr <= 0 && !isDead)
            {
                isDead = true;
                OnDied();
            }

            GameManager.Eventbus.Publish(new GEOnPlayerHealthChanged((int)hpCurr, (int)hpMax, -1, hpDecay));
        }

        protected override void OnDied()
        {
            if (!TryResurrect())
            {
                Debug.Log("최대 체력 0, 구현 바람!");
                GameManager.Eventbus.Publish(new GEOnPlayerGameOver());
                return;
            }
        }

        public override void SetHP(float amount, bool isRelative)
        {
            float hpFrom = hpCurr;
            base.SetHP(amount, isRelative);
            float hpTo = hpCurr;

            GameManager.Eventbus.Publish(new GEOnPlayerHealthChanged((int)hpCurr, (int)hpMax, (int)(hpTo - hpFrom), hpDecay));
        }

        /// <summary> 부활 시도 </summary>
        private bool TryResurrect()
        {
            // 최대 체력이 0이 되면 부활 실패
            if (hpMax - 1 <= 0)
                return false;

            // 아니면 최대 체력 1 감소 후 부활
            isDead = false;
            hpMax--;
            hpDecay++;
            SetHP(hpMax, false);

            GameManager.Eventbus.Publish(new GEOnPlayerResurrected());

            TryRestoreDecay();

            return true;
        }

        /// <summary> 체력 및 위 게이지 상태에 따라 감소된 최대 체력 복구 시도 </summary>
        private bool TryRestoreDecay()
        {
            if (energyCurr < energyMax)
                return false;

            if (hpDecay <= 0)
                return false;

            ClearEnergy();
            hpMax++;
            hpDecay--;
            SetHP(1, true);

            return true;
        }

        #endregion

        #region 공격

        protected override void OnHitTarget(IHittable target, Skill skill)
        {
            SetEnergy(energyGainPerHit * energyMult, true);
        }

        #endregion

        #region 추가 능력치

        public void AddEnergyMultAttribute(float v)
        {
            energyMult += v;
        }

        public void AddConsumeChanceAttribute(float v)
        {
            consumeChance += v;
        }

        public void AddMaxHPAttribute(float v)
        {
            hpMax = math.max(1.0f, hpMax + v);
            hpCurr = hpMax;
        }

        #endregion

    }
}
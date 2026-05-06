using System;
using Unity.Mathematics;
using UnityEngine;

namespace LAMENT
{
    public class Player : Entity
    {
        [Header("최대 체력 감소")]
        [SerializeField] private int hpDecay = 0;

        [Header("장비 슬롯")]
        [SerializeField] public EquipSlot leftArmSlot;
        [SerializeField] private EquipSlot rightArmSlot;
        [SerializeField] private EquipSlot legSlot;

        public EquipSlot LeftArmSlot => leftArmSlot;
        public EquipSlot RightArmSlot => rightArmSlot;
        public EquipSlot LegSlot => legSlot;

        private EquipSlot lastUsedEquipment;

        // ===== 위 게이지 =====
        private float energyMax = 100;
        private float energyCurr = 0;
        private float energyGainPerHit = 5;

        // ===== 장기 런타임 추가 능력치 =====
        private float energyMult = 1.0f;
        private float consumeChance = 1.0f;

        // 장기 효과로 증가한 최대 체력 보정치
        private float gutBonusMaxHp = 0f;

        // 장기 보정 제외 기본 최대 체력
        private float baseHpMaxWithoutGut = 1f;

        private PlayerGutRuntime gutRuntime;

        protected override void Awake()
        {
            base.Awake();

            baseHpMaxWithoutGut = hpMax;
            gutRuntime = new PlayerGutRuntime(this);

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
            base.OnDestroy();

            GameManager.Eventbus.Unsubscribe<GEOnEquipmentEquipped>(OnPlayerEquipmentChanged);
            GameManager.Eventbus.Unsubscribe<GEOnGutLoadoutChanged>(OnGutLoadoutChanged);
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

        private void InitEquipments()
        {
            TryCreateEffector(leftArmSlot.Equipment, true);
            TryCreateEffector(rightArmSlot.Equipment, true);
            TryCreateEffector(legSlot.Equipment);
        }

        private void InitEvents()
        {
            GameManager.Eventbus.Subscribe<GEOnEquipmentEquipped>(OnPlayerEquipmentChanged);
            GameManager.Eventbus.Subscribe<GEOnGutLoadoutChanged>(OnGutLoadoutChanged);
        }

        private void PublishInitStates()
        {
            GameManager.Eventbus.Publish(new GEOnPlayerHealthChanged((int)hpCurr, (int)hpMax, 0, hpDecay));
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
            gutRuntime.Rebuild();
        }

        private void OnGutLoadoutChanged(GEOnGutLoadoutChanged e)
        {
            gutRuntime.Rebuild();
        }

        #endregion

        #region 장기 런타임

        public void ResetGutRuntimeAttributes()
        {
            energyMult = 1.0f;
            consumeChance = 1.0f;
            gutBonusMaxHp = 0f;

            RefreshEffectiveMaxHp();
        }

        public void OnGutRuntimeRebuilt()
        {
            RefreshEffectiveMaxHp();

            GameManager.Eventbus.Publish(new GEOnPlayerHealthChanged((int)hpCurr, (int)hpMax, 0, hpDecay));
            GameManager.Eventbus.Publish(new GEOnPlayerEnergyChanged(energyCurr, energyMax));
        }

        private void RefreshEffectiveMaxHp()
        {
            float oldHpMax = hpMax;
            float oldHpCurr = hpCurr;

            hpMax = math.max(1.0f, baseHpMaxWithoutGut + gutBonusMaxHp);

            if (oldHpMax <= 0f)
            {
                hpCurr = math.min(hpCurr, hpMax);
                return;
            }

            float ratio = oldHpCurr / oldHpMax;
            hpCurr = math.clamp(hpMax * ratio, 0, hpMax);
        }

        #endregion

        #region 스킬 및 장비

        public bool TryUseEquipment(EquipSlot slot, Skill skill, Action cbOnSkillEnd = null, bool isBurst = false, QTEResultContext qteContext = default)
        {
            if (!slot.IsReady() && !isBurst)
                return false;

            if (qteContext.DamageMultiplier <= 0f)
                qteContext = QTEResultContext.None;

            lastUsedEquipment = slot;
            if (!TryStartSkill(skill, cbOnSkillEnd, qteContext.DamageMultiplier <= 0f ? 1f : qteContext.DamageMultiplier))
                return false;

            GameManager.Eventbus.Publish(new GEOnPlayerUsedEquiment(slot.Type, lastUsedEquipment.Equipment, skill));

            if (isBurst)
            {
                bool shouldConsume = !qteContext.PreventBurstConsume && BurstRoll();
                if (shouldConsume)
                    BurstEquipment(slot);
            }

            return true;
        }

        public bool BurstRoll()
        {
            return UnityEngine.Random.value < consumeChance;
        }

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

            GameManager.Eventbus.Publish(new GEOnEquipmentEquipped(
                null,
                replaced,
                slotType));
        }

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

        private bool TryResurrect()
        {
            if (baseHpMaxWithoutGut - 1 <= 0)
                return false;

            isDead = false;
            baseHpMaxWithoutGut--;
            hpDecay++;

            RefreshEffectiveMaxHp();
            SetHP(hpMax, false);

            GameManager.Eventbus.Publish(new GEOnPlayerResurrected());

            TryRestoreDecay();

            return true;
        }

        private bool TryRestoreDecay()
        {
            if (energyCurr < energyMax)
                return false;

            if (hpDecay <= 0)
                return false;

            ClearEnergy();

            baseHpMaxWithoutGut++;
            hpDecay--;

            RefreshEffectiveMaxHp();
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
            gutBonusMaxHp += v;
            RefreshEffectiveMaxHp();
        }

        #endregion
    }
}
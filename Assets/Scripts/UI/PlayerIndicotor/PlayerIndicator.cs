using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace LAMENT
{
    public class PlayerIndocator : MonoBehaviour
    {
        [Header("HP")]
        [SerializeField] private RectTransform hpRoot;
        [SerializeField] private List<HPIcon> hpIcons;
        [SerializeField] private GameObject hpIconPrefab;
        [SerializeField] private float hpAnimSpeedStable = 5f; // 최대 체력일 때 애니 속도
        [SerializeField] private float hpAnimSpeedLethal = 11f; // 최소 체력일 때 애니 속도

        private float hitJitterCurr = 0f;
        [Space(5), SerializeField] private float hitJitter = 5f; // 피격시 흔들림 정도
        [SerializeField] private float hitJitterAnimSpeed = 1f;

        [Space(5), SerializeField] private float throbScale = 1.2f;

        [Header("에너지")]
        [SerializeField] private Image energyImg;
        [SerializeField] private Text energyText;
        [SerializeField] private float energyAnimSpeed = 5f;
        private float energyMax = 1;
        private float energyCurr = 0; // 게이지가 연속적으로 오르는 애니메이션에 사용

        [Header("스킬")]
        [SerializeField] private CooldownBox[] skillBoxes;

        private void Awake()
        {
            // 이벤트 등록
            GameManager.Eventbus.Subscribe<GEOnPlayerHealthChanged>(OnPlayerHealthChanged);
            GameManager.Eventbus.Subscribe<GEOnPlayerEnergyChanged>(OnPlayerEnergyChanged);
            GameManager.Eventbus.Subscribe<GEOnEquipmentEquipped>(OnPlayerEquipmentChanged);
            GameManager.Eventbus.Subscribe<GEOnPlayerSkillFinished>(OnPlayerSkillFinished);
        }

        private void OnDestroy()
        {
            // 이벤트 삭제
            GameManager.Eventbus.Unsubscribe<GEOnPlayerHealthChanged>(OnPlayerHealthChanged);
            GameManager.Eventbus.Unsubscribe<GEOnPlayerEnergyChanged>(OnPlayerEnergyChanged);
            GameManager.Eventbus.Unsubscribe<GEOnEquipmentEquipped>(OnPlayerEquipmentChanged);
            GameManager.Eventbus.Unsubscribe<GEOnPlayerSkillFinished>(OnPlayerSkillFinished);
        }

        private void Update()
        {
            UpdateHPJitter();
            UpdateEnergy();
        }

        #region HP

        private void OnPlayerHealthChanged(GEOnPlayerHealthChanged e)
        {
            // 초기화 여부 확인
            if (hpIcons == null || hpIcons.Count == 0)
                return;

            // 애니 속도 계산
            float animSpeed = math.lerp(hpAnimSpeedLethal, hpAnimSpeedStable, (float)e.Curr / (e.Max + e.Decay));

            // 아이콘 개수가 모자라면 새로 생성
            while (hpIcons.Count < e.Max + e.Decay)
                hpIcons.Add(Instantiate(hpIconPrefab, hpRoot).GetComponent<HPIcon>());

            // 최대 체력 복구 시 꿈틀 애니
            bool bThrobAni = false;
            if (0< e.Delta)
                bThrobAni = true;

            // 모든 아이콘을 순회
            for (int i = 0; i < hpIcons.Count; i++)
            {
                HPIcon ele = hpIcons[i];

                bool isVisible = i < e.Max + e.Decay;
                bool isFaded = e.Max <= i;
                bool isFilled = i < e.Curr;

                ele.SetVisible(isVisible);
                ele.SetFaded(isFaded);
                ele.SetFilled(isFilled);
                ele.SetAnimSpeed(animSpeed);

                if (bThrobAni)
                    ele.SetThrobScale(throbScale);
            }

            // 피격이라면 흔들기
            if (e.Delta < 0)
                hitJitterCurr = hitJitter;
        }

        private void UpdateHPJitter()
        {
            if (hitJitterCurr <= 0f)
                return;

            hpRoot.localPosition = new Vector3(
                UnityEngine.Random.Range(-hitJitterCurr, hitJitterCurr),
                UnityEngine.Random.Range(-hitJitterCurr, hitJitterCurr)
            );

            hitJitterCurr -= Time.unscaledDeltaTime * hitJitterAnimSpeed;
            if (hitJitterCurr <= 0)
                hpRoot.localPosition = Vector3.zero;
        }

        #endregion
    
        #region 에너지

        private void OnPlayerEnergyChanged(GEOnPlayerEnergyChanged e)
        {
            energyCurr = e.Current;
            energyMax = e.Max;
        }

        private void UpdateEnergy()
        {
            float proportion = energyCurr / energyMax;
            energyImg.fillAmount = math.lerp(energyImg.fillAmount, proportion, Time.deltaTime * energyAnimSpeed);

            if (0.01f < proportion)
                energyText.text = $"{math.ceil(energyImg.fillAmount * 100)}%";
            else
                energyText.text = $"{math.floor(energyImg.fillAmount * 100)}%";
        }

        #endregion
    
        #region 스킬 및 장비

        public void OnPlayerEquipmentChanged(GEOnEquipmentEquipped e)
        {
            EquipmentData equipment = e.Equipped;

            switch (e.SlotType)
            {
                case EEquipSlotType.LEFT:
                    if (equipment)
                    {
                        skillBoxes[1].SetIcon(equipment.SkillIcon);
                        skillBoxes[3].SetIcon(((WeaponData)equipment).BurstSkillIcon);
                    }
                    else
                    {
                        skillBoxes[1].SetIcon(null);
                        skillBoxes[3].SetIcon(null);
                    }
                    break;
                case EEquipSlotType.RIGHT:
                    if (equipment)
                    {
                        skillBoxes[2].SetIcon(equipment.SkillIcon);
                        skillBoxes[4].SetIcon(((WeaponData)equipment).BurstSkillIcon);
                    }
                    else
                    {
                        skillBoxes[2].SetIcon(null);
                        skillBoxes[4].SetIcon(null);
                    }
                    break;
                case EEquipSlotType.LEG:
                    if (equipment)
                        skillBoxes[0].SetIcon(equipment.SkillIcon);
                    else
                        skillBoxes[0].SetIcon(null);
                    break;
            }
        }

        public void OnPlayerSkillFinished(GEOnPlayerSkillFinished e)
        {
            EEquipSlotType equipSlot = e.Slot.Type;

            switch (equipSlot)
            {
                case EEquipSlotType.LEFT:
                    skillBoxes[1].SetCooldown(e.Slot.Equipment.Cooldown);
                    break;
                case EEquipSlotType.RIGHT:
                    skillBoxes[2].SetCooldown(e.Slot.Equipment.Cooldown);
                    break;
                case EEquipSlotType.LEG:
                    skillBoxes[0].SetCooldown(e.Slot.Equipment.Cooldown);
                    break;
            }
        }


        #endregion
    }
}
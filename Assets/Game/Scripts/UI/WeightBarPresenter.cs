using UnityEngine;
using UnityEngine.UI;

namespace LAMENT
{
    /// <summary>
    /// 장착된 3부위(왼/오/다리)의 무게 합계를 바 형태로 표시.
    /// 초록=정상, 노랑=느려짐, (선택) 빨강=과중
    /// </summary>
    public sealed class WeightBarPresenter : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private EquipmentLoadoutService _loadout;
        [SerializeField] private Image _fill;
        [SerializeField] private Image _iconWeight;
        [SerializeField] private Image _decorLeft;
        [SerializeField] private Image _decorRight;

        [Header("Config")]
        [Tooltip("허용 중량 (예: 100). 합계/허용중량 = 바 비율")]
        [SerializeField] private float _capacity = 100f;

        [Tooltip("이 비율 이상이면 '느려짐(노랑)'")]
        [Range(0f, 1f)]
        [SerializeField] private float _slowThreshold = 0.50f;

        [Tooltip("(선택) 이 비율 초과면 '과중(빨강)'")]
        [Range(0f, 2f)]
        [SerializeField] private float _overThreshold = 1.00f;

        [Header("Colors")]
        [SerializeField] private Color _green = new Color(0.2f, 1f, 0.2f);
        [SerializeField] private Color _yellow = new Color(1f, 0.9f, 0.2f);
        [SerializeField] private Color _red = new Color(1f, 0.3f, 0.3f);

        public float CurrentWeight { get; private set; }
        public float Capacity => Mathf.Max(1f, _capacity);
        public float Ratio01 => Mathf.Clamp01(CurrentWeight / Capacity);


        private void Awake()
        {
            GameManager.Eventbus.Subscribe<GEOnEquipmentEquipped>(OnEquipped);
            GameManager.Eventbus.Subscribe<GEOnOverlayStateChanged>(OnOverlayStateChanged);
            Refresh();
        }

        private void OnDestroy()
        {
            GameManager.Eventbus.Unsubscribe<GEOnEquipmentEquipped>(OnEquipped);
            GameManager.Eventbus.Unsubscribe<GEOnOverlayStateChanged>(OnOverlayStateChanged);
        }

        private void OnEquipped(GEOnEquipmentEquipped e)
        {
            Refresh();
        }

        private void OnOverlayStateChanged(GEOnOverlayStateChanged e)
        {
            if (e.isOpened)
                Refresh();
        }

        private void Refresh()
        {
            if (_loadout == null || _fill == null) return;

            CurrentWeight = SumEquippedWeights(_loadout);
            var ratio = Ratio01;

            _fill.fillAmount = ratio;
            _fill.color = CalcColor(ratio);

            Debug.Log($"[WeightBar] weight={CurrentWeight:F1} / {Capacity:F1} ({ratio:P0})");
        }

        private static float SumEquippedWeights(EquipmentLoadoutService loadout)
        {
            float W(ItemData item)
            {
                return item != null ? item.Weight : 0f;
            }

            return W(loadout.CurrentLeft) + W(loadout.CurrentRight) + W(loadout.CurrentLeg);
        }

        private Color CalcColor(float ratio)
        {
            if (ratio > _overThreshold) return _red;      // 선택: 100% 초과
            if (ratio >= _slowThreshold) return _yellow;  // 50~100%
            return _green;                                 // 0~49%
        }
    }
}
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
        [SerializeField] private Player _player;
        [SerializeField] private Image _fill;
        [SerializeField] private Image _iconWeight;
        [SerializeField] private Image _decorLeft;
        [SerializeField] private Image _decorRight;

        [Header("Config")]
        [SerializeField] private float _capacity = 100f;

        [Range(0f, 1f)]
        [SerializeField] private float _slowThreshold = 0.50f;

        [Range(0f, 2f)]
        [SerializeField] private float _overThreshold = 1.00f;

        [Header("Colors")]
        [SerializeField] private Color _green = new Color(0.2f, 1f, 0.2f);
        [SerializeField] private Color _yellow = new Color(1f, 0.9f, 0.2f);
        [SerializeField] private Color _red = new Color(1f, 0.3f, 0.3f);

        public float CurrentWeight { get; private set; }

        public float Capacity
        {
            get
            {
                int bonus = 0;
                if (_player && _player.GutRuntime != null)
                    bonus = _player.GutRuntime.WeightCapacityBonus;

                return Mathf.Max(1f, _capacity + bonus);
            }
        }

        public float Ratio01 => Mathf.Clamp01(CurrentWeight / Capacity);

        private void Awake()
        {
            if (!_player)
                _player = FindObjectOfType<Player>();

            GameManager.Eventbus.Subscribe<GEOnEquipmentEquipped>(OnEquipped);
            GameManager.Eventbus.Subscribe<GEOnOverlayStateChanged>(OnOverlayStateChanged);
            GameManager.Eventbus.Subscribe<GEOnGutsRuntimeChanged>(OnGutsRuntimeChanged);

            Refresh();
        }

        private void OnDestroy()
        {
            GameManager.Eventbus.Unsubscribe<GEOnEquipmentEquipped>(OnEquipped);
            GameManager.Eventbus.Unsubscribe<GEOnOverlayStateChanged>(OnOverlayStateChanged);
            GameManager.Eventbus.Unsubscribe<GEOnGutsRuntimeChanged>(OnGutsRuntimeChanged);
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

        private void OnGutsRuntimeChanged(GEOnGutsRuntimeChanged e)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (_loadout == null || _fill == null)
                return;

            CurrentWeight = SumEquippedWeights(_loadout);
            float ratio = Ratio01;

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
            if (ratio > _overThreshold) return _red;
            if (ratio >= _slowThreshold) return _yellow;
            return _green;
        }
    }
}
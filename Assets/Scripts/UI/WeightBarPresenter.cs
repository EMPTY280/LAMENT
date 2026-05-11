using UnityEngine;
using UnityEngine.UI;

namespace LAMENT
{
    /// <summary>
    /// 인벤토리에 소지 중인 아이템의 총무게를 바 형태로 표시하고 이동 감속을 적용.
    /// 초록=정상, 노랑=느려짐, (선택) 빨강=과중
    /// </summary>
    public sealed class WeightBarPresenter : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private InventoryService _inventory;
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

        [Header("Movement Slow")]
        [Range(0f, 1f)]
        [SerializeField] private float _minMoveSpeedMultiplier = 0.5f;

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
                return Mathf.Max(1f, _capacity + bonus);
            }
        }

        public float Ratio01 => Mathf.Clamp01(CurrentWeight / Capacity);

        private void Awake()
        {
            if (!_player)
                _player = FindObjectOfType<Player>();

            if (!_inventory && _player)
                _inventory = _player.GetComponent<InventoryService>();

            if (!_inventory)
                _inventory = FindObjectOfType<InventoryService>();

            GameManager.Eventbus.Subscribe<GEOnInventorySlotChanged>(OnInventorySlotChanged);
            GameManager.Eventbus.Subscribe<GEOnOverlayStateChanged>(OnOverlayStateChanged);

            Refresh();
        }

        private void OnDestroy()
        {
            GameManager.Eventbus.Unsubscribe<GEOnInventorySlotChanged>(OnInventorySlotChanged);
            GameManager.Eventbus.Unsubscribe<GEOnOverlayStateChanged>(OnOverlayStateChanged);
        }

        private void OnInventorySlotChanged(GEOnInventorySlotChanged e)
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
            if (_inventory == null)
                return;

            CurrentWeight = _inventory.TotalWeight;
            float ratio = Ratio01;

            if (_fill)
            {
                _fill.fillAmount = ratio;
                _fill.color = CalcColor(ratio);
            }

            ApplyMovementSlow(ratio);

            Debug.Log($"[WeightBar] weight={CurrentWeight:F1} / {Capacity:F1} ({ratio:P0})");
        }

        private void ApplyMovementSlow(float ratio)
        {
            if (!_player || _player.MoveComponent == null)
                return;

            float multiplier = 1f;
            if (ratio >= _slowThreshold)
            {
                float slowRange = Mathf.Max(0.0001f, _overThreshold - _slowThreshold);
                float slowT = Mathf.Clamp01((ratio - _slowThreshold) / slowRange);
                multiplier = Mathf.Lerp(1f, _minMoveSpeedMultiplier, slowT);
            }

            _player.MoveComponent.SetSpeedMultiplier(multiplier);
        }

        private Color CalcColor(float ratio)
        {
            if (ratio > _overThreshold) return _red;
            if (ratio >= _slowThreshold) return _yellow;
            return _green;
        }
    }
}

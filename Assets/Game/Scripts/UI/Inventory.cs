using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LAMENT
{
    /// <summary>
    /// SRP: 인벤토리 데이터를 읽어서 UI 반영만 수행.
    /// DIP: 데이터 접근은 IInventoryService와 EventBus에 의존.
    /// </summary>
    public sealed class InventoryGridPresenter : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private InventoryService _inventory; // IInventoryService 구현체(InventoryService)
        [SerializeField] private Transform _gridParent;            // GridLayoutGroup가 붙은 오브젝트
        [SerializeField] private GameObject _slotPrefab;           // 내부에 "Icon"(Image), "Count"(TMP) 자식 필요

        // 캐시: 성능/간결성
        private Image[] _icons;
        private TextMeshProUGUI[] _counts;

        private void Awake()
        {
            if (_inventory == null)
            {
                enabled = false;
                return;
            }

            BuildGrid();
            SubscribeEvents();
            FullSync();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        private void BuildGrid()
        {
            // 기존 자식 정리
            for (int i = _gridParent.childCount - 1; i >= 0; i--)
                Destroy(_gridParent.GetChild(i).gameObject);

            _icons = new Image[_inventory.SlotCount];
            _counts = new TextMeshProUGUI[_inventory.SlotCount];

            for (int i = 0; i < _inventory.SlotCount; i++)
            {
                var go = Instantiate(_slotPrefab, _gridParent);
                _icons[i]  = go.transform.Find("Icon").GetComponent<Image>();
                _counts[i] = go.transform.Find("Count").GetComponent<TextMeshProUGUI>();
            }
        }

        private void SubscribeEvents()
        {
            GameManager.Eventbus.Subscribe<GEOnInventorySlotChanged>(OnSlotChanged);
        }

        private void UnsubscribeEvents()
        {
            GameManager.Eventbus.Unsubscribe<GEOnInventorySlotChanged>(OnSlotChanged);
        }

        private void FullSync()
        {
            for (int i = 0; i < _inventory.SlotCount; i++)
                ApplySlotToUI(i, _inventory.GetSlot(i));
        }

        private void OnSlotChanged(GEOnInventorySlotChanged evt)
        {
            ApplySlotToUI(evt.Index, evt.Stack);
        }

        private void ApplySlotToUI(int index, ItemStack st)
        {
            var icon = _icons[index];
            var count = _counts[index];

            if (st.IsEmpty)
            {
                icon.sprite = null;
                icon.color = new Color(1f, 1f, 1f, 0.15f);
                count.text = "";
                return;
            }

            icon.sprite = st.Item.Icon;
            icon.color = Color.white;

            // 장비(=1스택)는 개수 숨김, 스택형은 숫자 표시
            var showCount = st.Item.StackCount > 1 && st.Count > 1;
            count.text = showCount ? st.Count.ToString() : "";
        }
    }
}

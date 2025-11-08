using UnityEngine;

namespace LAMENT
{
    /// <summary>
    /// SRP: 인벤토리 데이터/로직만 담당.
    /// OCP: 아이템 타입 추가해도 이 로직은 그대로(스택 규칙만 참조).
    /// </summary>
    public sealed class InventoryService : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private int _slotCount = 20;
        [SerializeField] private ItemStack[] _slots;


        public int SlotCount => _slotCount;

        private void Awake()
        {
            // 내부 배열 준비
            if (_slots == null || _slots.Length != _slotCount)
                _slots = new ItemStack[_slotCount];
        }

        public int AddItem(ItemData item, int amount)
        {
            if (item == null || amount <= 0) return 0;

            var remaining = amount;

            // 1) 기존 동일 아이템 스택에 합치기
            for (var i = 0; i < _slots.Length && remaining > 0; i++)
            {
                if (_slots[i].IsEmpty) continue;
                if (_slots[i].Item != item) continue;

                var canPush = Mathf.Min(remaining, _slots[i].SpaceLeft);
                if (canPush > 0)
                {
                    _slots[i].Add(canPush);
                    remaining -= canPush;
                    PublishSlotChanged(i);
                }
            }

            // 2) 빈칸에 새 스택 생성
            for (var i = 0; i < _slots.Length && remaining > 0; i++)
            {
                if (!_slots[i].IsEmpty)
                    continue;

                var put = Mathf.Min(remaining, Mathf.Max(1, item.StackCount));
                _slots[i] = new ItemStack(item, put);
                remaining -= put;
                PublishSlotChanged(i);
            }

            var added = amount - remaining;
            GameManager.Eventbus.Publish(new GEOnInventoryItemAdded(item.ID, amount, added));
            return added;
        }

        public bool RemoveAt(int index, int count)
        {
            if (!IsValidIndex(index) || count <= 0) return false;

            var st = _slots[index];
            if (st.IsEmpty) return false;

            st.Remove(count);
            if (st.Count <= 0) st.Clear();

            _slots[index] = st;
            PublishSlotChanged(index);
            return true;
        }

        public ItemStack GetSlot(int index)
        {
            return IsValidIndex(index) ? _slots[index] : default;
        }

        private bool IsValidIndex(int index) => index >= 0 && index < _slots.Length;

        private void PublishSlotChanged(int index)
        {
            GameManager.Eventbus.Publish(new GEOnInventorySlotChanged(index, _slots[index]));
        }
    }
}
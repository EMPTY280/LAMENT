using UnityEngine;

namespace LAMENT
{
    /// <summary>한 슬롯에 들어가는 아이템 스택 정보(SRP).</summary>
    [System.Serializable]
    public struct ItemStack
    {
        [SerializeField] private ItemData item;
        [SerializeField] private int _count;

        public ItemData Item => item;
        public int Count => _count;

        public bool IsEmpty => item == null || _count <= 0;
        public int Capacity => item == null ? 0 : Mathf.Max(1, item.StackCount);
        public int SpaceLeft => Mathf.Max(0, Capacity - _count);

        public ItemStack(ItemData item, int count)
        {
            this.item = item;
            _count = count;
        }

        public void Add(int amount) => _count += amount;
        public void Remove(int amount) => _count = Mathf.Max(0, _count - amount);
        public void Clear() { item = null; _count = 0; }
    }
}
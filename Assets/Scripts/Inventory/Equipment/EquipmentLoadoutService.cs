using UnityEngine;

namespace LAMENT
{
    /// <summary>
    /// 현재 장착(왼팔/오른팔/다리) 보유 + 확정 장착 시: 새 장비 1개 소비, 이전 장비 인벤토리 반환.
    /// </summary>
    public sealed class EquipmentLoadoutService : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private InventoryService _inventoryProvider; // InventoryService drag
        [SerializeField] private Player _playerProvider;

        private InventoryService _inventory;
        private Player _player;

        public EquipmentData CurrentRight => GetEquipped(EEquipSlotType.RIGHT);
        public EquipmentData CurrentLeft => GetEquipped(EEquipSlotType.LEFT);
        public EquipmentData CurrentLeg => GetEquipped(EEquipSlotType.LEG);

        private void Awake()
        {
            _inventory = _inventoryProvider
                      ?? GetComponent<InventoryService>()
                      ?? GetComponentInParent<InventoryService>()
                      ?? GetComponentInChildren<InventoryService>();
            _player = _playerProvider
                   ?? GetComponent<Player>()
                   ?? GetComponentInParent<Player>()
                   ?? GetComponentInChildren<Player>()
                   ?? FindObjectOfType<Player>();

            if (_inventory == null)
            {
                enabled = false;
                Debug.LogError("[EquipmentLoadoutService] IInventoryService 미연결");
            }
        }

        /// <summary>해당 부위에 next 장착. prev는 인벤토리로, next는 인벤토리에서 1개 제거.</summary>
        public void Equip(EquipmentData next)
        {
            if (next == null) return;

            var slot = next.Slot;
            var prev = GetEquipped(slot);

            bool RemoveOne(InventoryService inv, ItemData item)
            {
                for (int i = 0; i < inv.SlotCount; i++)
                {
                    var st = inv.GetSlot(i);
                    if (st.IsEmpty || st.Item != item) continue;
                    return inv.RemoveAt(i, 1);
                }
                return false;
            }

            // 새 장비 1개 소비
            RemoveOne(_inventory, next);

            // 이전 장비 반환
            if (prev != null) _inventory.AddItem(prev, 1);

            GameManager.Eventbus.Publish(new GEOnEquipmentEquipped(next, prev, slot));
        }

        private EquipmentData GetEquipped(EEquipSlotType slot)
        {
            if (_player == null)
                return null;

            switch (slot)
            {
                case EEquipSlotType.LEFT:
                    return _player.LeftArmSlot.Equipment;
                case EEquipSlotType.RIGHT:
                    return _player.RightArmSlot.Equipment;
                case EEquipSlotType.LEG:
                    return _player.LegSlot.Equipment;
            }

            return null;
        }
    }
}

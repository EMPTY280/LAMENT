
namespace LAMENT
{
    public interface IGameEvent { }

    /// <summary> 오버레이(장비창 등) 열림/닫힘 신호. </summary>
    public readonly struct GEOnOverlayStateChanged : IGameEvent
    {
        public readonly bool isOpened;

        public GEOnOverlayStateChanged(bool isOpend)
        {
            isOpened = isOpend;
        }
    }

    /// <summary> 장착 확정 시 발행 (연출/사운드/스탯이 구독) </summary>
    public readonly struct GEOnEquipmentEquipped : IGameEvent
    {
        public EquipmentData Equipped { get; }
        public EquipmentData Replaced { get; }
        public EEquipSlotType SlotType { get; }

        public GEOnEquipmentEquipped(EquipmentData equipped, EquipmentData replaced, EEquipSlotType slotType)
        {
            Equipped = equipped;
            Replaced = replaced;
            SlotType = slotType;
        }
    }

    public readonly struct GEOnEquipmentPreview : IGameEvent
    {
        public EEquipSlotType Slot { get; }   // LeftArm / RightArm / Legs
        public EquipmentData Current { get; }   // 중앙
        public EquipmentData Prev { get; }   // 위/왼(이전)
        public EquipmentData Next { get; }   // 아래/오른(다음)
        public int Index { get; }   // 후보 리스트 내 현재 인덱스

        public GEOnEquipmentPreview(
            EEquipSlotType slot,
            EquipmentData current,
            EquipmentData prev,
            EquipmentData next,
            int index)
        {
            Slot = slot;
            Current = current;
            Prev = prev;
            Next = next;
            Index = index;
        }
    }

    /// <summary> 인벤토리 슬롯 변화 </summary>
    public readonly struct GEOnInventorySlotChanged : IGameEvent
    {
        public int Index { get; }
        public ItemStack Stack { get; }

        public GEOnInventorySlotChanged(int index, ItemStack stack)
        {
            Index = index;
            Stack = stack;
        }
    }

    /// <summary> 인벤토리에 아이템 추가 시도 결과 </summary>
    public readonly struct GEOnInventoryItemAdded : IGameEvent
    {
        public string ItemId { get; }
        public int Requested { get; }
        public int Added { get; }

        public GEOnInventoryItemAdded(string itemId, int requested, int added)
        {
            ItemId = itemId;
            Requested = requested;
            Added = added;
        }
    }

    public readonly struct GEOnSkillFinished: IGameEvent
    {
        public EquipSlot Slot { get; }

        public GEOnSkillFinished(EquipSlot slot)
        {
            Slot = slot;
        }
    }
}
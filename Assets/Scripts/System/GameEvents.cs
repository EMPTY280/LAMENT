namespace LAMENT
{
    public interface IGameEvent { }

    public readonly struct GEOnOverlayStateChanged : IGameEvent
    {
        public readonly bool isOpened;

        public GEOnOverlayStateChanged(bool isOpend)
        {
            isOpened = isOpend;
        }
    }

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
        public EquipmentData Current { get; }
        public EquipmentData Prev { get; }
        public EquipmentData Next { get; }
        public int Index { get; }

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

    /// <summary> 인벤토리의 슬롯이 변경되었을 때 </summary>
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

    /// <summary> 인벤토리에 아이템이 추가되었을 때 </summary>
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

     public readonly struct GEOnPlayerHealthChanged : IGameEvent
    {
        public int Curr { get; } // 현재 체력
        public int Max { get; } // 최대 체력
        public int Delta { get; } // 변화량
        public int Decay { get; } // 최대 체력 감소량

        public GEOnPlayerHealthChanged(int curr, int max, int delta, int decay)
        {
            Curr = curr;
            Max = max;
            Delta = delta;
            Decay = decay;
        }
    }

    public readonly struct GEOnPlayerEnergyChanged : IGameEvent
    {
        public float Current { get; }
        public float Max { get; }

        public GEOnPlayerEnergyChanged(float current, float max)
        {
            Current = current;
            Max = max;
        }
    }

    /// <summary> 플레이어 부활 </summary>
    public readonly struct GEOnPlayerResurrected : IGameEvent
    {
    }
    
    /// <summary> 플레이어 사망 </summary>
    public readonly struct GEOnPlayerDied : IGameEvent
    {
        public int RemainingMaxHp { get; }

        public GEOnPlayerDied(int remainingMaxHp)
        {
            RemainingMaxHp = remainingMaxHp;
        }
    }

    /// <summary> 플레이어 게임 오버 </summary>
    public readonly struct GEOnPlayerGameOver : IGameEvent
    {
        
    }
}
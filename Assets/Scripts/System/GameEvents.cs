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
        public EEquipSlotType Slot { get; }
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

    /// <summary> 플레이어 스킬 사용 시작 </summary>
    public readonly struct GEOnPlayerUsedEquiment : IGameEvent
    {
        public EEquipSlotType SlotType { get; }
        public EquipmentData Equipment { get; }
        public Skill Skill { get; }

        public GEOnPlayerUsedEquiment(EEquipSlotType slotType, EquipmentData equipment, Skill skill)
        {
            SlotType = slotType;
            Equipment = equipment;
            Skill = skill;
        }
    }

    /// <summary> 플레이어 스킬 사용 종료 </summary>
    public readonly struct GEOnPlayerSkillFinished : IGameEvent
    {
        public EquipSlot Slot { get; }

        public GEOnPlayerSkillFinished(EquipSlot slot)
        {
            Slot = slot;
        }
    }

    public readonly struct GEOnPlayerHealthChanged : IGameEvent
    {
        public int Curr { get; }
        public int Max { get; }
        public int Delta { get; }
        public int Decay { get; }

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

    public readonly struct GEOnMoneyChanged : IGameEvent
    {
        public int Current { get; }
        public int Delta { get; }

        public GEOnMoneyChanged(int current, int delta)
        {
            Current = current;
            Delta = delta;
        }
    }

    public readonly struct GEOnGutPurchased : IGameEvent
    {
        public GutData Gut { get; }
        public int Price { get; }

        public GEOnGutPurchased(GutData gut, int price)
        {
            Gut = gut;
            Price = price;
        }
    }

    public readonly struct GEOnGutShopPurchased : IGameEvent
    {
        public GutData Gut { get; }
        public int Price { get; }

        public GEOnGutShopPurchased(GutData gut, int price)
        {
            Gut = gut;
            Price = price;
        }
    }

    /// <summary> 특정 장기 슬롯의 장착 상태가 변경되었을 때 </summary>
    public readonly struct GEOnGutEquipped : IGameEvent
    {
        public EGutType GutType { get; }
        public GutData Equipped { get; }
        public GutData Replaced { get; }

        public GEOnGutEquipped(EGutType gutType, GutData equipped, GutData replaced)
        {
            GutType = gutType;
            Equipped = equipped;
            Replaced = replaced;
        }
    }

    /// <summary> 장기 전체 로드아웃이 변경되었을 때 </summary>
    public readonly struct GEOnGutLoadoutChanged : IGameEvent
    {
    }

    public readonly struct GEOnQTEStarted : IGameEvent
    {
        public EEquipSlotType SlotType { get; }
        public EquipmentData Equipment { get; }
        public Skill Skill { get; }
        public EQTEDirection[] Sequence { get; }
        public float TimeLimit { get; }
        public string QteId { get; }

        public GEOnQTEStarted(
            EEquipSlotType slotType,
            EquipmentData equipment,
            Skill skill,
            EQTEDirection[] sequence,
            float timeLimit,
            string qteId)
        {
            SlotType = slotType;
            Equipment = equipment;
            Skill = skill;
            Sequence = sequence;
            TimeLimit = timeLimit;
            QteId = qteId;
        }
    }

    public readonly struct GEOnQTEProgress : IGameEvent
    {
        public int CurrentIndex { get; }
        public int TotalCount { get; }
        public EQTEDirection LastInput { get; }

        public GEOnQTEProgress(int currentIndex, int totalCount, EQTEDirection lastInput)
        {
            CurrentIndex = currentIndex;
            TotalCount = totalCount;
            LastInput = lastInput;
        }
    }

    public readonly struct GEOnQTESucceeded : IGameEvent
    {
        public string QteId { get; }
        public float DamageMultiplier { get; }
        public bool PreventBurstConsume { get; }

        public GEOnQTESucceeded(string qteId, float damageMultiplier, bool preventBurstConsume)
        {
            QteId = qteId;
            DamageMultiplier = damageMultiplier;
            PreventBurstConsume = preventBurstConsume;
        }
    }

    public readonly struct GEOnQTEFailed : IGameEvent
    {
        public string QteId { get; }

        public GEOnQTEFailed(string qteId)
        {
            QteId = qteId;
        }
    }

    public readonly struct GEOnQTEFinished : IGameEvent
    {
        public string QteId { get; }
        public bool IsSuccess { get; }

        public GEOnQTEFinished(string qteId, bool isSuccess)
        {
            QteId = qteId;
            IsSuccess = isSuccess;
        }
    }
}
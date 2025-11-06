using System.Collections.Generic;

public enum EComboInputTypes
{
    NONE,  // 입력 없음

    LEFT,
    RIGHT,

    UTILITY,

    LEFT_BURST,
    RIGHT_BURST
}

public class ComboNode
{
    private EComboInputTypes inputType;
    public EComboInputTypes Type => inputType;

    protected List<ComboNode> children; // 연결될 수 있는 다음 노드
    public List<ComboNode> Children => children;

    public ComboNode(EComboInputTypes inputType = EComboInputTypes.NONE)
    {
        this.inputType = inputType;
        children = new List<ComboNode>();
    }

    public void AddChild(ComboNode node)
    {
        children.Add(node);
    }

    public void ClearChild()
    {
        children.Clear();
    }
}

public class ComboNodeInput : ComboNode
{
    private EquipSlot equipSlot; // 배정된 장비 슬롯
    private Skill skill; // 배정된 스킬
    private bool isBurst = false; // 폭파 스킬 여부

    public EquipSlot Equipment => equipSlot;
    public Skill Skill => skill;
    public bool IsBurst => isBurst;


    public ComboNodeInput(EComboInputTypes inputType) : base(inputType) { }

    public void Set(EquipSlot e, Skill s, bool isBurst = false)
    {
        equipSlot = e;
        skill = s;
        this.isBurst = isBurst;
    }
}
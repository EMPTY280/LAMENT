// TODO: 임시로 간단히 static 클래스 형태로 이벤트 구현함...

public static class OnEquipmentChanged
{
    public delegate void GEvent(EquipSlot equipment);

    private static event GEvent e;


    public static event GEvent Event
    {
        add
        {
            e -= value;
            e += value;
        }
        remove
        {
            e -= value;
        }
    }

    public static void Notify(EquipSlot equipment)
    {
        e?.Invoke(equipment);
    }
}

public static class OnSkillFinished
{
    public delegate void GEvent(EquipSlot equipment);

    private static event GEvent e;


    public static event GEvent Event
    {
        add
        {
            e -= value;
            e += value;
        }
        remove
        {
            e -= value;
        }
    }

    public static void Notify(EquipSlot equipment)
    {
        e?.Invoke(equipment);
    }
}

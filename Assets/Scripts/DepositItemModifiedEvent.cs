using IEvent = GreenAbyss.Events.IEvent;

public struct DepositItemModifiedEvent<ItemType> : IEvent where ItemType : Item
{
    public uint previousValue;
    public uint currentValue;

    public void Assign(params object[] parameters)
    {
        previousValue = (uint)parameters[0];
        currentValue = (uint)parameters[1];
    }

    public void Reset()
    {
        previousValue = default(uint);
        currentValue = default(uint);
    }
}

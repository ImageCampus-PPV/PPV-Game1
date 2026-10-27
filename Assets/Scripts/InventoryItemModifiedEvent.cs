using IEvent = GreenAbyss.Events.IEvent;

public struct InventoryItemModifiedEvent<ItemType> : IEvent where ItemType : Item
{
    public int slotIndex;
    public uint previousValue;
    public uint currentValue;

    public void Assign(params object[] parameters)
    {
        slotIndex = (int)parameters[0];
        previousValue = (uint)parameters[1];
        currentValue = (uint)parameters[2];
    }

    public void Reset()
    {
        slotIndex = default(int);
        previousValue = default(uint);
        currentValue = default(uint);
    }
}
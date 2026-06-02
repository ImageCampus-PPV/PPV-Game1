

[System.Serializable]
public class InventoryStack
{
    public ItemType Type;
    public int Count;

    public Item Sample;

    public InventoryStack(Item item)
    {
        Type = item.Type;
        Count = 1;
        Sample = item;
    }
}

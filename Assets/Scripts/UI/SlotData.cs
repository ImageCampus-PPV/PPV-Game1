

public enum SlotOwner { Inventory, Deposit }


public class SlotData
{
    public InventoryStack Stack;
    public SlotOwner Owner;
    public int Index;
}

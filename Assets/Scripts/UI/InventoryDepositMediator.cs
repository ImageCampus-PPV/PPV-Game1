using UnityEngine;


public class InventoryDepositMediator : MonoBehaviour
{
    private MyInventory _inventory;
    private DepositData _depositData;

    public void Initialize(MyInventory inventory, DepositData depositData)
    {
        _inventory = inventory;
        _depositData = depositData;
    }

    public void HandleDrop(SlotData source, SlotData target)
    {
        if (source.Stack == null) 
            return;

        if (source.Owner == SlotOwner.Inventory && target.Owner == SlotOwner.Deposit)
            HandleInventoryToDeposit(source, target);
        else if (source.Owner == SlotOwner.Deposit && target.Owner == SlotOwner.Inventory)
            HandleDepositToInventory(source, target);
        else if (source.Owner == SlotOwner.Inventory && target.Owner == SlotOwner.Inventory)
            HandleInventoryToInventory(source, target);
        else if (source.Owner == SlotOwner.Deposit && target.Owner == SlotOwner.Deposit)
            HandleDepositToDeposit(source, target);
    }

    private void HandleInventoryToDeposit(SlotData source, SlotData target)
    {
        InventoryStack fromInventory = source.Stack;
        InventoryStack inDeposit = target.Stack;

        if (inDeposit == null)
        {
            _depositData.SetStackAt(target.Index, fromInventory);
            _inventory.RemoveStackAt(source.Index);
        }
        else if (inDeposit.Type == fromInventory.Type)
        {
            inDeposit.Count += fromInventory.Count;
            _depositData.NotifyChanged();
            _inventory.RemoveStackAt(source.Index);
        }
        else
        {
            _depositData.SetStackAt(target.Index, fromInventory);
            _inventory.SetStackAt(source.Index, inDeposit);
        }
    }

    private void HandleDepositToInventory(SlotData source, SlotData target)
    {
        InventoryStack fromDeposit = source.Stack;
        InventoryStack inInventory = target.Stack;

        if (inInventory == null)
        {
            _inventory.SetStackAt(target.Index, fromDeposit);
            _depositData.SetStackAt(source.Index, null);
        }
        else if (inInventory.Type == fromDeposit.Type)
        {
            inInventory.Count += fromDeposit.Count;
            _inventory.NotifyChanged();
            _depositData.SetStackAt(source.Index, null);
        }
        else
        {
            _inventory.SetStackAt(target.Index, fromDeposit);
            _depositData.SetStackAt(source.Index, inInventory);
        }
    }

    private void HandleInventoryToInventory(SlotData source, SlotData target)
    {
        InventoryStack a = source.Stack;
        InventoryStack b = target.Stack;

        if (a != null && b != null && a.Type == b.Type)
        {
            b.Count += a.Count;
            _inventory.RemoveStackAt(source.Index);
        }
        else
        {
            _inventory.SetStackAt(source.Index, b);
            _inventory.SetStackAt(target.Index, a);
        }

        _inventory.NotifyChanged();
    }

    private void HandleDepositToDeposit(SlotData source, SlotData target)
    {
        InventoryStack a = source.Stack;
        InventoryStack b = target.Stack;

        if (a != null && b != null && a.Type == b.Type)
        {
            b.Count += a.Count;
            _depositData.SetStackAt(source.Index, null);
        }
        else
        {
            _depositData.SetStackAt(source.Index, b);
            _depositData.SetStackAt(target.Index, a);
        }
    }
}

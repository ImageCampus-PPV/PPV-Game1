using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;

public class Inventory : IInitiable, IService
{
    private Dictionary<Type, uint> _amountOfitemsByType;

    public bool IsPersistance => false;

    public const uint MaxSlots = 5;
    public const uint MaxItemCount = 50;
    public bool IsInventoryFull => _amountOfitemsByType.Count == MaxSlots;

    public void Init()
    {
        _amountOfitemsByType = new Dictionary<Type, uint>();
    }

    public void LateInit()
    {
    }

    public bool IsItemAtMax<ItemType>() where ItemType : Item
    {
        if (!_amountOfitemsByType.ContainsKey(typeof(ItemType)))
            return false;

        return _amountOfitemsByType[typeof(ItemType)] == MaxItemCount;
    }

    public bool TryAddItemOfType<ItemType>() where ItemType : Item
    {
        if (IsInventoryFull)
            return false;

        if (!_amountOfitemsByType.TryAdd(typeof(ItemType), 1))
        {
            if (!IsItemAtMax<ItemType>())
            {
                _amountOfitemsByType[typeof(ItemType)]++;
            }
            else
                return false;
        }

        return true;
    }

    public bool TryRemoveItemOfType<ItemType>() where ItemType : Item
    {
        if (!_amountOfitemsByType.ContainsKey(typeof(ItemType)))
            return false;

        if (_amountOfitemsByType[typeof(ItemType)] <= 0)
        {
            _amountOfitemsByType[typeof(ItemType)] = 0;
            return false;
        }

        _amountOfitemsByType[typeof(ItemType)]--;
        return true;
    }
}
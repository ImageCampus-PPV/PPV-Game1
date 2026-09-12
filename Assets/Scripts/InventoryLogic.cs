using Assets.Scripts.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;

public class InventoryLogic : IInitiable, IService
{
    private Dictionary<Type, uint> _amountOfitemsByType;

    public const uint MaxSlots = 5;
    public const uint MaxItemCount = 50;
    public bool IsPersistance => false;
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

        _amountOfitemsByType[typeof(ItemType)]--;

        if (_amountOfitemsByType[typeof(ItemType)] <= 0)
        {
            _amountOfitemsByType.Remove(typeof(ItemType));
            return true;
        }

        return true;
    }

    public IEnumerable<(Type, uint)> FlushItems()
    {
        foreach (KeyValuePair<Type, uint> itemByType in _amountOfitemsByType)
        {
            yield return (itemByType.Key, itemByType.Value);
        }

        _amountOfitemsByType.Clear();
    }
}

public sealed class InventoryController : IInitiable
{
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
    private InventoryLogic InventoryLogic => ServiceProvider.Instance.GetService<InventoryLogic>();
    private Wallet Wallet => ServiceProvider.Instance.GetService<Wallet>();
    public bool IsPersistance => false;

    public void Init()
    {
        EventBus.Subscribe<PlayerRequestInteractionAccepted<Deposit>>(OnCharacterInteract);
    }

    private void OnCharacterInteract(in PlayerRequestInteractionAccepted<Deposit> depositInteraction)
    {
        foreach ((Type resourceType, uint resourceAmount) item in InventoryLogic.FlushItems())
        {
            Wallet.AddResource(item.resourceAmount, item.resourceType.Name);
        }
    }

    public void LateInit()
    {
    }
}
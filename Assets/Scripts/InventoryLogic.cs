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
        return _amountOfitemsByType[typeof(ItemType)] == MaxItemCount;
    }

    public bool TryAddItemOfType<ItemType>(uint amount = 1) where ItemType : Item
    {
        if (!_amountOfitemsByType.ContainsKey(typeof(ItemType)) && IsInventoryFull)
            return false;

        if (!_amountOfitemsByType.ContainsKey(typeof(ItemType)))
            _amountOfitemsByType.Add(typeof(ItemType), 0);

        if (!IsItemAtMax<ItemType>())
        {
            _amountOfitemsByType[typeof(ItemType)] += amount;
            return true;
        }

        return false;
    }

    public bool TryRemoveItemOfType<ItemType>(uint amount = 1) where ItemType : Item
    {
        if (!_amountOfitemsByType.ContainsKey(typeof(ItemType)))
            return false;

        _amountOfitemsByType[typeof(ItemType)] -= amount;

        if (_amountOfitemsByType[typeof(ItemType)] <= 0)
            _amountOfitemsByType.Remove(typeof(ItemType));

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
    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
    private Wallet Wallet => ServiceProvider.Instance.GetService<Wallet>();
    public bool IsPersistance => false;

    public void Init()
    {
        EventBus.Subscribe<PlayerRequestInteractionAccepted<Deposit>>(OnCharacterInteract);
        EventBus.Subscribe<PlayerRequestInteractionAccepted<DragonItem>>(OnDragonItemInteraction);
        EventBus.Subscribe<PlayerRequestInteractionAccepted<MechaItem>>(OnMechaItemInteraction);
    }

    private void OnCharacterInteract(in PlayerRequestInteractionAccepted<Deposit> depositInteraction)
    {
        foreach ((Type resourceType, uint resourceAmount) item in InventoryLogic.FlushItems())
        {
            Wallet.AddResource(item.resourceAmount, item.resourceType.Name);
        }
    }

    private void OnDragonItemInteraction(in PlayerRequestInteractionAccepted<DragonItem> dragonItemInteractionEvent)
    {
        if (!EntityRegistry.Has(dragonItemInteractionEvent.interactableID))
            return;

        if (InventoryLogic.TryAddItemOfType<DragonItem>())
        {
            BaseEntity entity = EntityRegistry.GetAs<BaseEntity>(dragonItemInteractionEvent.interactableID);
            EntityRegistry.Remove(entity);
        }
    }

    private void OnMechaItemInteraction(in PlayerRequestInteractionAccepted<MechaItem> mechaItemInteractionEvent)
    {
        if (!EntityRegistry.Has(mechaItemInteractionEvent.interactableID))
            return;

        if (InventoryLogic.TryAddItemOfType<MechaItem>())
        {
            BaseEntity entity = EntityRegistry.GetAs<BaseEntity>(mechaItemInteractionEvent.interactableID);
            EntityRegistry.Remove(entity);
        }
    }

    public void LateInit()
    {
    }
}
using Assets.Scripts.Entities;
using GreenAbyss.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;
using System.Reflection;

public class InventoryLogic : IInitiable, IService
{
    private Dictionary<Type, uint> _amountOfitemsByType;

    public const uint MAX_SLOTS = 5;
    public const uint MAX_ITEMS_COUNT = 50;
    public bool IsPersistance => false;
    public bool IsInventoryFull => _amountOfitemsByType.Count == MAX_SLOTS;

    public void Init()
    {
        _amountOfitemsByType = new Dictionary<Type, uint>();
    }

    public void LateInit()
    {
    }

    public bool IsItemAtMax<ItemType>() where ItemType : Item
    {
        return _amountOfitemsByType[typeof(ItemType)] == MAX_ITEMS_COUNT;
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

public sealed class InventoryController : IInitiable, IDisposable
{
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
    private InventoryLogic InventoryLogic => ServiceProvider.Instance.GetService<InventoryLogic>();
    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
    private Wallet Wallet => ServiceProvider.Instance.GetService<Wallet>();
    public bool IsPersistance => false;

    private Dictionary<Type, object> _onInteractionCallBack;
    private MethodInfo _subscribeToInteractionEvent;
    private MethodInfo _unsuscribeEventSystem;

    public void Init()
    {
        _onInteractionCallBack = new Dictionary<Type, object>();

        _unsuscribeEventSystem = EventBus.GetType().GetMethod(nameof(EventBus.Subscribe), BindingFlags.Public | BindingFlags.Instance);
        _subscribeToInteractionEvent = GetType().GetMethod(nameof(SubscribeToInteract), BindingFlags.NonPublic | BindingFlags.Instance);

        EventBus.Subscribe<PlayerRequestInteractionAccepted<Deposit>>(OnCharacterInteract);

        SubscribeToEvents();
    }

    private void OnCharacterInteract(in PlayerRequestInteractionAccepted<Deposit> depositInteraction)
    {
        foreach ((Type resourceType, uint resourceAmount) item in InventoryLogic.FlushItems())
        {
            Wallet.AddResource(item.resourceAmount, item.resourceType.Name);
        }
    }

    private void OnItemInteraction<EntityType>(PlayerRequestInteractionAccepted<EntityType> dragonItemInteractionEvent) where EntityType : Interactable
    {
        if (!EntityRegistry.Has(dragonItemInteractionEvent.interactableID))
            return;

        if (InventoryLogic.TryAddItemOfType<DragonItem>())
        {
            BaseEntity entity = EntityRegistry.GetAs<BaseEntity>(dragonItemInteractionEvent.interactableID);
            EntityRegistry.Remove(entity);
        }
    }

    private void SubscribeToEvents()
    {
        foreach (Type type in GetType().Assembly.GetTypes())
        {
            if (!type.IsClass || type.IsAbstract)
                continue;

            if (!typeof(Interactable).IsAssignableFrom(type) || type == typeof(Deposit))
                continue;

            _subscribeToInteractionEvent.MakeGenericMethod(type).Invoke(this, new object[0]);
        }
    }

    private void SubscribeToInteract<EntityType>() where EntityType : Interactable
    {
        EventBus.EventCallback<PlayerRequestInteractionAccepted<EntityType>> callback =
            EventBus.SubscribeAndReturn<PlayerRequestInteractionAccepted<EntityType>>(OnObjectInteract);

        _onInteractionCallBack.Add(typeof(PlayerRequestInteractionAccepted<EntityType>), callback);

        void OnObjectInteract(in PlayerRequestInteractionAccepted<EntityType> callback)
        {
            OnItemInteraction<EntityType>(callback);
        }
    }

    public void LateInit()
    {
    }

    public void Dispose()
    {
        foreach (KeyValuePair<Type, object> eventSub in _onInteractionCallBack)
            _unsuscribeEventSystem.MakeGenericMethod(eventSub.Key).Invoke(EventBus, new object[] { eventSub.Value });
    }
}
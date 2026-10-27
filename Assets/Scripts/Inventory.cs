using System;
using System.Collections.Generic;
using System.Reflection;
using EntityRegistry = GreenAbyss.Entities.EntityRegistry;
using ServiceProvider = GreenAbyss.Services.ServiceProvider;
using EventBus = GreenAbyss.Events.EventBus;
using IService = GreenAbyss.Services.IService;

public class Inventory : IService
{
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    private readonly Dictionary<Type, uint> _amountOfitemsByType;
    private readonly Type[] _slots;
    private readonly MethodInfo raiseEventModified;

    public const uint MAX_SLOTS = 5;
    public const uint MAX_ITEMS_COUNT = 50;
    public bool IsPersistance => false;
    public bool IsInventoryFull => _amountOfitemsByType.Count >= MAX_SLOTS;

    public Inventory()
    {
        _amountOfitemsByType = new Dictionary<Type, uint>();
        _slots = new Type[MAX_SLOTS];

        raiseEventModified = GetType().GetMethod(
            nameof(RaiseItemModified),
            BindingFlags.Instance | BindingFlags.NonPublic);
    }

    private void RaiseItemModified<ItemType>(int slot, uint previousAmount, uint currentAmount) where ItemType : Item
    {
        EventBus.Raise<InventoryItemModifiedEvent<ItemType>>(slot, previousAmount, currentAmount);
    }

    private void RaiseItemModifiedType(Type itemType, int slot, uint previousAmount, uint currentAmount)
    {
        raiseEventModified.MakeGenericMethod(itemType).Invoke(this, new object[] { slot, previousAmount, currentAmount });
    }

    public bool IsItemAtMax<ItemType>() where ItemType : Item
    {
        return _amountOfitemsByType.TryGetValue(typeof(ItemType), out uint amount) && amount >= MAX_ITEMS_COUNT;
    }

    public bool TryAddItemOfType<ItemType>(uint amount = 1) where ItemType : Item
    {
        Type type = typeof(ItemType);
        bool exists = _amountOfitemsByType.TryGetValue(type, out uint previous);

        if (!exists && IsInventoryFull)
            return false;

        if (exists && previous >= MAX_ITEMS_COUNT)
            return false;

        int slot;
        if (exists)
        {
            slot = Array.IndexOf(_slots, type);
        }
        else
        {
            slot = Array.IndexOf(_slots, null);
            _slots[slot] = type;
        }

        uint current = Math.Min(previous + amount, MAX_ITEMS_COUNT);
        _amountOfitemsByType[type] = current;

        RaiseItemModified<ItemType>(slot, previous, current);
        return true;
    }

    public bool TryRemoveItemOfType<ItemType>(uint amount = 1) where ItemType : Item
    {
        Type type = typeof(ItemType);

        if (!_amountOfitemsByType.TryGetValue(type, out uint previous))
            return false;

        int slot = Array.IndexOf(_slots, type);
        uint current = amount >= previous ? 0 : previous - amount;

        if (current == 0)
        {
            _amountOfitemsByType.Remove(type);
            _slots[slot] = null;
        }
        else
        {
            _amountOfitemsByType[type] = current;
        }

        RaiseItemModified<ItemType>(slot, previous, current);
        return true;
    }

    public List<(Type, uint)> FlushItems()
    {
        List<(Type, uint)> flushed = new List<(Type, uint)>(_amountOfitemsByType.Count);

        for (int slot = 0; slot < _slots.Length; slot++)
        {
            Type type = _slots[slot];
            if (type == null)
                continue;

            uint previous = _amountOfitemsByType[type];
            flushed.Add((type, previous));
        }

        _amountOfitemsByType.Clear();

        for (int i = 0; i < _slots.Length; ++i)
        {
            Type type = _slots[i];
            if (type == null)
                continue;

            _slots[i] = null;
            RaiseItemModifiedType(type, i, flushed.Find(f => f.Item1 == type).Item2, 0);
        }

        return flushed;
    }
}

public sealed class InventoryController : IInitiable, IDisposable
{
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
    private Inventory InventoryLogic => ServiceProvider.Instance.GetService<Inventory>();
    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
    private Wallet Wallet => ServiceProvider.Instance.GetService<Wallet>();
    public bool IsPersistance => false;

    private Dictionary<Type, object> _onInteractionCallBack;
    private MethodInfo _subscribeToInteractionEvent;
    private MethodInfo _unsuscribeEventSystem;

    private Dictionary<Type, Type> _mineToResource;
    private MethodInfo _tryAddToInventory;
    private MethodInfo _addToWallet;


    public void Init()
    {
        _mineToResource = new Dictionary<Type, Type>();
        _onInteractionCallBack = new Dictionary<Type, object>();

        _unsuscribeEventSystem = EventBus.GetType().GetMethod(nameof(EventBus.Subscribe), BindingFlags.Public | BindingFlags.Instance);
        _subscribeToInteractionEvent = GetType().GetMethod(nameof(SubscribeToInteract), BindingFlags.NonPublic | BindingFlags.Instance);
        _addToWallet = Wallet.GetType().GetMethod(nameof(Wallet.AddResourceByType), BindingFlags.Public | BindingFlags.Instance);

        _tryAddToInventory = InventoryLogic.GetType().GetMethod(nameof(InventoryLogic.TryAddItemOfType), BindingFlags.Public | BindingFlags.Instance);

        EventBus.Subscribe<PlayerRequestInteractionAccepted<Deposit>>(OnCharacterInteract);

        SubscribeToEvents();

        foreach (Type type in GetType().Assembly.GetTypes())
        {
            if (!type.IsClass || type.IsAbstract)
                continue;

            List<MineOfAttribute> attributes = new List<MineOfAttribute>(type.GetCustomAttributes<MineOfAttribute>());

            if (attributes.Count == 0)
                continue;

            _mineToResource.Add(type, attributes[0].itemType);
        }
    }

    private void OnCharacterInteract(in PlayerRequestInteractionAccepted<Deposit> depositInteraction)
    {
        foreach ((Type resourceType, uint resourceAmount) item in InventoryLogic.FlushItems())
        {
            _addToWallet.MakeGenericMethod(item.resourceType).Invoke(Wallet, new object[] { item.resourceAmount });
        }
    }

    private void OnItemInteraction<EntityType>(PlayerRequestInteractionAccepted<EntityType> dragonItemInteractionEvent) where EntityType : Interactable
    {
        if (!EntityRegistry.Has(dragonItemInteractionEvent.interactableID))
            return;

        _tryAddToInventory.MakeGenericMethod(_mineToResource[typeof(EntityType)]).Invoke(InventoryLogic, new object[] { (uint)1 });
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
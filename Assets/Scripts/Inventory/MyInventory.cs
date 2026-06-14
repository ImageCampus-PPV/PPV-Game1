using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.InputSystem;
using ImageCampus.ToolBox.Services;
using ImageCampus.ToolBox.Events;
using Inventory.Events;


public class MyInventory : MonoBehaviour, IService
{
    [SerializeField] private int _capacity = 5;
    [SerializeField] private InventorySlotUI _slotPrefab;
    [SerializeField] private Transform _slotContainer;

    private readonly List<InventoryStack> _stacks = new();
    private InventorySlotUI[] _slots;
    private EventBus _eventBus;

    public bool IsPersistance => false;

    public ReadOnlyCollection<InventoryStack> Stacks => _stacks.AsReadOnly();
    public int StackCount => _stacks.Count;
    public bool IsFull => _stacks.Count >= _capacity;

    private void Awake()
    {
        ServiceProvider.Instance.AddService<MyInventory>(this);

        if (ServiceProvider.Instance.ContainsService<EventBus>())
            _eventBus = ServiceProvider.Instance.GetService<EventBus>();

        InitSlots();
    }

    private void OnDestroy()
    {
        ServiceProvider.Instance.RemoveService<MyInventory>();
    }

    public bool TryAdd(Item item)
    {
        if (item == null) 
            return false;

        InventoryStack existing = _stacks.Find(s => s.Type == item.Type);

        if (existing != null)
        {
            existing.Count++;
        }
        else
        {
            if (IsFull) return 
                    false;

            _stacks.Add(new InventoryStack(item));
        }

        item.gameObject.SetActive(false);

        RefreshUI();

        _eventBus?.Raise<ItemAddedEvent>(item, this);

        return true;
    }

    private void InitSlots()
    {
        if (_slotPrefab == null || _slotContainer == null) 
            return;

        _slots = new InventorySlotUI[_capacity];

        for (int i = 0; i < _capacity; i++)
        {
            _slots[i] = Instantiate(_slotPrefab, _slotContainer);
            _slots[i].Clear();
        }
    }

    private void RefreshUI()
    {
        if (_slots == null) 
            return;

        for (int i = 0; i < _slots.Length; i++)
        {
            if (i < _stacks.Count)
                _slots[i].SetStack(_stacks[i]);
            else
                _slots[i].Clear();
        }
    }
}

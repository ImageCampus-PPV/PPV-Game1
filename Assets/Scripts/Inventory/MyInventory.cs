using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.InputSystem;
using ImageCampus.ToolBox.Services;
using ImageCampus.ToolBox.Events;
using Inventory.Events;


public class MyInventory : MonoBehaviour, IService
{
    [SerializeField] private int _capacity = 8;
    [SerializeField] private InventorySlotUI _slotPrefab;
    [SerializeField] private Transform _slotContainer;

    private readonly List<Item> _items = new();
    private InventorySlotUI[] _slots;
    private EventBus _eventBus;

    public bool IsPersistance => false;

    public ReadOnlyCollection<Item> Items => _items.AsReadOnly();
    public int Count => _items.Count;
    public bool IsFull => _items.Count >= _capacity;

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
        if (item == null || IsFull)
            return false;

        _items.Add(item);
        item.gameObject.SetActive(false);

        RefreshUI();
        _eventBus?.Raise<ItemAddedEvent>(item, this);

        return true;
    }

    public void OnUseItem(InputAction.CallbackContext context)
    {
        if (!context.performed || _items.Count == 0) return;
        UseItemAt(0);
    }

    public void UseItemAt(int index)
    {
        if (index < 0 || index >= _items.Count) return;

        Item item = _items[index];
        _items.RemoveAt(index);

        item.UseItem();

        RefreshUI();
        _eventBus?.Raise<ItemRemovedEvent>(item, this);
    }

    public void DropItemAt(int index, Vector3 dropPosition)
    {
        if (index < 0 || index >= _items.Count) return;

        Item item = _items[index];
        _items.RemoveAt(index);

        item.gameObject.SetActive(true);
        item.transform.position = dropPosition;

        RefreshUI();
        _eventBus?.Raise<ItemRemovedEvent>(item, this);
    }

    private void InitSlots()
    {
        if (_slotPrefab == null || _slotContainer == null) return;

        _slots = new InventorySlotUI[_capacity];
        for (int i = 0; i < _capacity; i++)
        {
            _slots[i] = Instantiate(_slotPrefab, _slotContainer);
            _slots[i].Clear();
        }
    }

    private void RefreshUI()
    {
        if (_slots == null) return;

        for (int i = 0; i < _slots.Length; i++)
        {
            if (i < _items.Count)
                _slots[i].SetItem(_items[i]);
            else
                _slots[i].Clear();
        }
    }
}

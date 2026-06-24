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
    [SerializeField] private DragVisual _dragVisual; // arrastrá el DragVisual del Canvas

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
            if (IsFull) return false;
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

            DraggableSlot draggable = _slots[i].GetComponent<DraggableSlot>();

            if (draggable != null)
                draggable.SetDragVisual(_dragVisual);
        }
    }

    private void RefreshUI()
    {
        if (_slots == null)
            return;

        for (int i = 0; i < _slots.Length; i++)
        {
            if (i < _stacks.Count)
            {
                _slots[i].SetStack(_stacks[i]);

                DraggableSlot draggable = _slots[i].GetComponentInChildren<DraggableSlot>();
                DroppableSlot droppable = _slots[i].GetComponentInChildren<DroppableSlot>();

                SlotData data = new SlotData { Stack = _stacks[i], Owner = SlotOwner.Inventory, Index = i };

                if (draggable != null) 
                    draggable.SlotData = data;

                if (droppable != null) 
                    droppable.SlotData = data;
            }
            else
            {
                _slots[i].Clear();

                DraggableSlot draggable = _slots[i].GetComponentInChildren<DraggableSlot>();
                DroppableSlot droppable = _slots[i].GetComponentInChildren<DroppableSlot>();

                SlotData data = new SlotData { Stack = null, Owner = SlotOwner.Inventory, Index = i };

                if (draggable != null) 
                    draggable.SlotData = data;

                if (droppable != null) 
                    droppable.SlotData = data;
            }
        }
    }

    public bool RemoveStack(ItemType type)
    {
        InventoryStack stack = _stacks.Find(s => s.Type == type);

        if (stack == null) 
            return false;

        stack.Count--;

        if (stack.Count <= 0)
            _stacks.Remove(stack);

        RefreshUI();
        return true;
    }

    public InventoryStack RemoveStackAt(int index)
    {
        if (index < 0 || index >= _stacks.Count)
            return null;

        InventoryStack stack = _stacks[index];
        _stacks.RemoveAt(index);
        RefreshUI();
        return stack;
    }

    public void SetStackAt(int index, InventoryStack stack)
    {
        if (index < 0 || index >= _stacks.Count)
        {
            if (stack != null)
                _stacks.Add(stack);
        }
        else
        {
            if (stack == null)
                _stacks.RemoveAt(index);
            else
                _stacks[index] = stack;
        }

        RefreshUI();
    }

    public void NotifyChanged()
    {
        RefreshUI();
    }

    public void SetMediator(InventoryDepositMediator mediator)
    {
        if (_slots == null) 
            return;

        foreach (InventorySlotUI slot in _slots)
        {
            DroppableSlot droppable = slot.GetComponent<DroppableSlot>();

            if (droppable != null)
                droppable.Mediator = mediator;
        }
    }
}

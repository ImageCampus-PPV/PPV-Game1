using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class ItemCollector : MonoBehaviour
{
    [SerializeField] private string _collectActionName = "Collect";

    public Action OnItemCollected;

    private readonly List<ItemPickupArea> _itemsInRange = new();
    private ItemPickupArea _closestItem;
    private PlayerInput _playerInput;
    private bool _subscribed;

    private PlayerInput PlayerInput
    {
        get
        {
            if (_playerInput == null)
                _playerInput = GetComponent<PlayerInput>();
            return _playerInput;
        }
    }

    private void Update()
    {
        TrySubscribe();
        UpdateClosest();
    }

    private void OnDisable()
    {
        UnsubscribePickupAction();
    }

    private void TrySubscribe()
    {
        if (_subscribed) return;
        if (PlayerInput == null || !PlayerInput.isActiveAndEnabled) return;

        InputAction action = PlayerInput.actions.FindAction(_collectActionName);
        if (action == null)
        {
            Debug.LogWarning($"[ItemCollector] Action '{_collectActionName}' not found {gameObject.name}");
            return;
        }

        action.performed += OnPickupPressed;
        _subscribed = true;
    }

    private void UnsubscribePickupAction()
    {
        if (!_subscribed || PlayerInput == null) return;

        InputAction action = PlayerInput.actions.FindAction(_collectActionName);
        if (action != null)
            action.performed -= OnPickupPressed;

        _subscribed = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<ItemPickupArea>(out var area)) return;
        if (!_itemsInRange.Contains(area))
            _itemsInRange.Add(area);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.TryGetComponent<ItemPickupArea>(out var area)) return;

        _itemsInRange.Remove(area);

        if (_closestItem == area)
        {
            _closestItem.HidePrompt();
            _closestItem = null;
        }
    }

    private void UpdateClosest()
    {
        _itemsInRange.RemoveAll(a => a == null);

        if (_itemsInRange.Count == 0)
        {
            if (_closestItem != null)
            {
                _closestItem.HidePrompt();
                _closestItem = null;
            }
            return;
        }

        ItemPickupArea newClosest = FindClosest();
        if (newClosest == _closestItem) return;

        _closestItem?.HidePrompt();
        _closestItem = newClosest;
        _closestItem.ShowPrompt(GetPickupButtonName());
    }

    private ItemPickupArea FindClosest()
    {
        ItemPickupArea closest = _itemsInRange[0];
        float minDist = Vector2.Distance(transform.position, closest.transform.position);

        for (int i = 1; i < _itemsInRange.Count; i++)
        {
            float dist = Vector2.Distance(transform.position, _itemsInRange[i].transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = _itemsInRange[i];
            }
        }

        return closest;
    }

    private void OnPickupPressed(InputAction.CallbackContext context)
    {
        if (_closestItem == null) return;

        if (ServiceProvider_HasInventoryFull()) return;

        Item item = _closestItem.Item;
        _closestItem.HidePrompt();
        _itemsInRange.Remove(_closestItem);
        _closestItem = null;

        item.Collect();
        OnItemCollected?.Invoke();
    }

    private bool ServiceProvider_HasInventoryFull()
    {
        if (!ImageCampus.ToolBox.Services.ServiceProvider.Instance.ContainsService<MyInventory>())
            return false;

        var inventory = ImageCampus.ToolBox.Services.ServiceProvider.Instance.GetService<MyInventory>();
        return inventory.IsFull;
    }

    private string GetPickupButtonName()
    {
        string notFound = "?";
        if (PlayerInput == null) return notFound;

        InputAction action = PlayerInput.actions.FindAction(_collectActionName);
        if (action == null) return notFound;

        string scheme = PlayerInput.currentControlScheme;
        foreach (InputBinding binding in action.bindings)
        {
            if (binding.isComposite || binding.isPartOfComposite) continue;
            if (!string.IsNullOrEmpty(scheme) && !binding.groups.Contains(scheme)) continue;

            string display = InputControlPath.ToHumanReadableString(
                binding.effectivePath,
                InputControlPath.HumanReadableStringOptions.UseShortNames);

            if (!string.IsNullOrEmpty(display))
                return display;
        }

        return notFound;
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ImageCampus.ToolBox.Services;
using ImageCampus.ToolBox.Events;
using Inventory.Events;


public class MyInventory : MonoBehaviour, IService
{
    [Header("UI")]
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private GameObject _emptyIndicator;

    private Item _currentItem;
    private EventBus _eventBus;

    public bool IsPersistance => false;
    public Item CurrentItem => _currentItem;
    public bool IsFull => _currentItem != null;

    private void Awake()
    {
        ServiceProvider.Instance.AddService<MyInventory>(this);

        if (ServiceProvider.Instance.ContainsService<EventBus>())
            _eventBus = ServiceProvider.Instance.GetService<EventBus>();

        RefreshUI();
    }

    private void OnDestroy()
    {
        ServiceProvider.Instance.RemoveService<MyInventory>();
    }


    public bool TryAdd(Item item)
    {
        if (item == null || IsFull) return false;

        _currentItem = item;
        item.gameObject.SetActive(false);

        RefreshUI();
        _eventBus?.Raise<ItemAddedEvent>(item, this);

        return true;
    }

    public Item TakeItem()
    {
        if (_currentItem == null) return null;

        Item item = _currentItem;
        _currentItem = null;

        RefreshUI();
        _eventBus?.Raise<ItemRemovedEvent>(item, this);

        return item;
    }

    private void RefreshUI()
    {
        if (_currentItem == null)
        {
            if (_icon != null) { _icon.sprite = null; _icon.color = new Color(1, 1, 1, 0); }
            if (_nameText != null) _nameText.text = string.Empty;
            if (_emptyIndicator != null) _emptyIndicator.SetActive(true);
            return;
        }

        if (_emptyIndicator != null) _emptyIndicator.SetActive(false);

        if (_nameText != null)
            _nameText.text = _currentItem.Type != null ? _currentItem.Type.name : _currentItem.name;

        if (_icon != null)
        {
            var sr = _currentItem.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                _icon.sprite = sr.sprite;
                _icon.color = Color.white;
            }
        }
    }
}

using GreenAbyss.Events;
using GreenAbyss.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryView : MonoBehaviour, IInitiable, IDisposable
{
    private PrefabsRegistry PrefabsRegistry => ServiceProvider.Instance.GetService<PrefabsRegistry>();
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    private const float CellWidth = 150f;
    private const float CellHeight = 64f;
    private const float IconSize = 48f;
    private const float Spacing = 10f;
    private const float Margin = 20f;

    private class Slot
    {
        public Image icon;
        public TextMeshProUGUI label;
        public Type itemType;
    }

    private Dictionary<Type, object> _itemModifiedEvents;
    private MethodInfo subscribeToEvents;
    private MethodInfo unsubscribe;

    private Slot[] _slots;
    private Dictionary<Type, Sprite> _spriteCache;

    private Canvas canvas = null;
    private RectTransform itemsContainer = null;

    public void Init()
    {
        subscribeToEvents = GetType().GetMethod(nameof(SubscribeToEvent), BindingFlags.NonPublic | BindingFlags.Instance);
        unsubscribe = EventBus.GetType().GetMethod(nameof(EventBus.Unsubscribe), BindingFlags.Public | BindingFlags.Instance);

        _itemModifiedEvents = new Dictionary<Type, object>();
        _spriteCache = new Dictionary<Type, Sprite>();

        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();

        int slotCount = (int)Inventory.MAX_SLOTS;
        CreateItemsContainer(slotCount);

        _slots = new Slot[slotCount];
        for (int i = 0; i < slotCount; i++)
            _slots[i] = CreateSlot(i);

        foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (!type.IsClass || type.IsAbstract)
                continue;

            if (!typeof(Item).IsAssignableFrom(type))
                continue;

            subscribeToEvents.MakeGenericMethod(type).Invoke(this, new object[0]);
        }
    }

    private void SubscribeToEvent<ItemType>() where ItemType : Item
    {
        object callback = EventBus.SubscribeAndReturn<InventoryItemModifiedEvent<ItemType>>(OnSlotModified);

        _itemModifiedEvents.Add(typeof(InventoryItemModifiedEvent<ItemType>), callback);

        void OnSlotModified(in InventoryItemModifiedEvent<ItemType> e)
        {
            UpdateSlot(e.slotIndex, typeof(ItemType), e.currentValue);
        }
    }

    private void UpdateSlot(int slotIndex, Type itemType, uint currentValue)
    {
        if (slotIndex < 0 || slotIndex >= _slots.Length)
            return;

        Slot slot = _slots[slotIndex];

        if (currentValue == 0)
        {
            ClearSlot(slot);
            return;
        }

        if (slot.itemType != itemType)
        {
            slot.itemType = itemType;
            slot.icon.sprite = GetItemSprite(itemType);
            slot.icon.enabled = true;
        }

        slot.label.text = $"x{currentValue}";
    }

    private void ClearSlot(Slot slot)
    {
        slot.itemType = null;
        slot.icon.sprite = null;
        slot.icon.enabled = false;
        slot.label.text = string.Empty;
    }

    private void CreateItemsContainer(int slotCount)
    {
        GameObject containerObj = new GameObject("ItemsContainer", typeof(RectTransform));
        itemsContainer = containerObj.GetComponent<RectTransform>();
        itemsContainer.SetParent(transform, false);

        itemsContainer.anchorMin = Vector2.zero;
        itemsContainer.anchorMax = Vector2.zero;
        itemsContainer.pivot = Vector2.zero;
        itemsContainer.anchoredPosition = new Vector2(Margin, Margin);
        itemsContainer.sizeDelta = new Vector2(
            slotCount * CellWidth + (slotCount - 1) * Spacing,
            CellHeight);

        GridLayoutGroup grid = containerObj.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(CellWidth, CellHeight);
        grid.spacing = new Vector2(Spacing, Spacing);
        grid.startCorner = GridLayoutGroup.Corner.LowerLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.LowerLeft;
        grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        grid.constraintCount = 1;
    }

    private Slot CreateSlot(int index)
    {
        GameObject entry = new GameObject($"Slot_{index}", typeof(RectTransform));
        entry.transform.SetParent(itemsContainer, false);

        Image background = entry.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.5f);
        background.raycastTarget = false;

        GameObject iconObj = new GameObject("Icon", typeof(RectTransform));
        RectTransform iconRect = (RectTransform)iconObj.transform;
        iconRect.SetParent(entry.transform, false);
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0f, 0.5f);
        iconRect.anchoredPosition = new Vector2(8f, 0f);
        iconRect.sizeDelta = new Vector2(IconSize, IconSize);

        Image icon = iconObj.AddComponent<Image>();
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        icon.enabled = false;

        GameObject textObj = new GameObject("Label", typeof(RectTransform));
        RectTransform textRect = (RectTransform)textObj.transform;
        textRect.SetParent(entry.transform, false);
        textRect.anchorMin = new Vector2(1f, 0f);
        textRect.anchorMax = new Vector2(1f, 0f);
        textRect.pivot = new Vector2(1f, 0f);
        textRect.anchoredPosition = new Vector2(-6f, 4f);
        textRect.sizeDelta = new Vector2(90f, 32f);

        TextMeshProUGUI label = textObj.AddComponent<TextMeshProUGUI>();

        TMP_FontAsset font = TMP_Settings.defaultFontAsset;

        label.font = font;
        label.fontSharedMaterial = font.material;

        label.color = Color.white;
        label.fontSize = 24;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.BottomRight;
        label.enableWordWrapping = false;
        label.overflowMode = TextOverflowModes.Overflow;
        label.raycastTarget = false;
        label.outlineWidth = 0.25f;
        label.outlineColor = Color.black;
        label.text = string.Empty;

        return new Slot { icon = icon, label = label, itemType = null };
    }

    private Sprite GetItemSprite(Type itemType)
    {
        if (_spriteCache.TryGetValue(itemType, out Sprite cached))
            return cached;

        Sprite sprite = null;
        GameObject prefab = PrefabsRegistry.FindPrefabByName(itemType.Name);

        if (prefab != null && prefab.TryGetComponent<SpriteRenderer>(out SpriteRenderer spriteRenderer))
            sprite = spriteRenderer.sprite;

        _spriteCache[itemType] = sprite;
        return sprite;
    }

    public void LateInit()
    {
    }

    public void Dispose()
    {
        foreach (KeyValuePair<Type, object> item in _itemModifiedEvents)
            unsubscribe.MakeGenericMethod(item.Key).Invoke(EventBus, new object[] { item.Value });

        _itemModifiedEvents?.Clear();
        _spriteCache?.Clear();
        _slots = null;

        if (itemsContainer != null)
            Destroy(itemsContainer.gameObject);
    }
}
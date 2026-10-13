using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DepositView : MonoBehaviour, IInitiable, IDisposable
{
    private PrefabsRegistry PrefabsRegistry => ServiceProvider.Instance.GetService<PrefabsRegistry>();
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    private const float CellWidth = 240f;
    private const float CellHeight = 64f;
    private const float IconSize = 48f;
    private const float Spacing = 10f;

    private Dictionary<Type, object> _itemModifiedEvents;
    private MethodInfo subscribeToEvents;
    private MethodInfo unsubscribe;

    private Dictionary<Type, string> textFormats;
    private Dictionary<Type, TextMeshProUGUI> labels;

    private Canvas canvas = null;
    private RectTransform itemsContainer = null;

    public void Init()
    {
        subscribeToEvents = GetType().GetMethod(nameof(SubscribeToEvent), BindingFlags.NonPublic | BindingFlags.Instance);
        unsubscribe = EventBus.GetType().GetMethod(nameof(EventBus.Unsubscribe), BindingFlags.Public | BindingFlags.Instance);

        _itemModifiedEvents = new Dictionary<Type, object>();
        textFormats = new Dictionary<Type, string>();
        labels = new Dictionary<Type, TextMeshProUGUI>();

        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();

        CreateItemsContainer();

        foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (!type.IsClass || type.IsAbstract)
                continue;

            if (!typeof(Item).IsAssignableFrom(type))
                continue;

            textFormats[type] = $"{type.Name} x{{0}}";

            CreateItemEntry(type);

            subscribeToEvents.MakeGenericMethod(type).Invoke(this, new object[0]);
        }
    }

    private void SubscribeToEvent<ItemType>() where ItemType : Item
    {
        object callback = EventBus.SubscribeAndReturn<DepositItemModifiedEvent<ItemType>>(SetAmountEvent);

        _itemModifiedEvents.Add(typeof(DepositItemModifiedEvent<ItemType>), callback);

        void SetAmountEvent(in DepositItemModifiedEvent<ItemType> inventoryItemModifiedEvent)
        {
            SetAmount(typeof(ItemType), inventoryItemModifiedEvent.currentValue);
        }
    }

    private void CreateItemsContainer()
    {
        GameObject containerObj = new GameObject("ItemsContainer");
        itemsContainer = containerObj.AddComponent<RectTransform>();
        itemsContainer.SetParent(transform, false);

        itemsContainer.anchorMin = Vector2.zero;
        itemsContainer.anchorMax = Vector2.one;
        itemsContainer.offsetMin = Vector2.zero;
        itemsContainer.offsetMax = Vector2.zero;

        GridLayoutGroup grid = containerObj.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(CellWidth, CellHeight);
        grid.spacing = new Vector2(Spacing, Spacing);
        grid.padding = new RectOffset(20, 20, 20, 20);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperLeft;
        grid.constraint = GridLayoutGroup.Constraint.Flexible;
    }

    private void CreateItemEntry(Type itemType)
    {
        GameObject entry = new GameObject(itemType.Name, typeof(RectTransform));
        entry.transform.SetParent(itemsContainer, false);

        Image background = entry.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.5f);

        HorizontalLayoutGroup row = entry.AddComponent<HorizontalLayoutGroup>();
        row.padding = new RectOffset(8, 8, 8, 8);
        row.spacing = 10f;
        row.childAlignment = TextAnchor.MiddleLeft;
        row.childControlWidth = true;
        row.childControlHeight = true;
        row.childForceExpandWidth = false;
        row.childForceExpandHeight = false;

        GameObject iconObj = new GameObject("Icon", typeof(RectTransform));
        iconObj.transform.SetParent(entry.transform, false);

        RawImage icon = iconObj.AddComponent<RawImage>();
        icon.texture = GetItemTexture(itemType);

        LayoutElement iconLayout = iconObj.AddComponent<LayoutElement>();
        iconLayout.preferredWidth = IconSize;
        iconLayout.preferredHeight = IconSize;

        GameObject textObj = new GameObject("Label", typeof(RectTransform));
        textObj.transform.SetParent(entry.transform, false);

        TextMeshProUGUI label = textObj.AddComponent<TextMeshProUGUI>();
        label.fontSize = 22;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.enableWordWrapping = false;
        label.overflowMode = TextOverflowModes.Ellipsis;

        LayoutElement labelLayout = textObj.AddComponent<LayoutElement>();
        labelLayout.flexibleWidth = 1f;

        labels[itemType] = label;

        SetAmount(itemType, 0);
    }

    private void SetAmount(Type itemType, uint amount)
    {
        if (!textFormats.TryGetValue(itemType, out string format))
            return;

        if (!labels.TryGetValue(itemType, out TextMeshProUGUI label))
            return;

        label.text = string.Format(format, amount);
    }

    private Texture GetItemTexture(Type itemType)
    {
        GameObject gameObject = PrefabsRegistry.FindPrefabByName(itemType.Name);

        if (gameObject == null || !gameObject.TryGetComponent<SpriteRenderer>(out SpriteRenderer spriteRenderer))
            return Texture2D.whiteTexture;

        return spriteRenderer.sprite.texture;
    }

    public void LateInit()
    {

    }

    public void Dispose()
    {
        foreach (KeyValuePair<Type, object> item in _itemModifiedEvents)
            unsubscribe.MakeGenericMethod(item.Key).Invoke(EventBus, new object[] { item.Value });

        textFormats?.Clear();
        labels?.Clear();

        if (itemsContainer != null)
            Destroy(itemsContainer.gameObject);
    }
}

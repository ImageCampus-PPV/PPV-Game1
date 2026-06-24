using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableSlot : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private DragVisual _dragVisual;
    [SerializeField] private Image _slotIcon;

    public SlotData SlotData { get; set; }

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void SetDragVisual(DragVisual dragVisual)
    {
        _dragVisual = dragVisual;
    }

    private DragVisual GetDragVisual()
    {
        if (_dragVisual != null) 
            return _dragVisual;

        _dragVisual = FindFirstObjectByType<DragVisual>();

        return _dragVisual;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (SlotData == null || SlotData.Stack == null) 
            return;

        DragVisual visual = GetDragVisual();

        if (visual == null) 
            return;

        Sprite icon = _slotIcon != null ? _slotIcon.sprite : null;

        visual.Show(icon, eventData.position);

        _canvasGroup.alpha = 0.4f;
        //_canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (SlotData == null || SlotData.Stack == null) 
            return;

        GetDragVisual()?.MoveTo(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GetDragVisual()?.Hide();
        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

public class DroppableSlot : MonoBehaviour, IDropHandler
{
    public InventoryDepositMediator Mediator { get; set; }
    public SlotData SlotData { get; set; }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log($"[DroppableSlot] OnDrop disparado en {gameObject.name}");

        DraggableSlot draggable = eventData.pointerDrag?.GetComponent<DraggableSlot>();
        if (draggable == null)
        {
            Debug.Log("[DroppableSlot] No se encontro DraggableSlot en el objeto arrastrado.");
            return;
        }

        SlotData source = draggable.SlotData;
        SlotData target = SlotData;

        if (source == null)
        {
            Debug.Log("[DroppableSlot] source SlotData es null.");
            return;
        }

        if (target == null)
        {
            Debug.Log("[DroppableSlot] target SlotData es null.");
            return;
        }

        if (Mediator == null)
        {
            Debug.Log("[DroppableSlot] Mediator es null.");
            return;
        }

        if (source.Owner == target.Owner && source.Index == target.Index)
        {
            Debug.Log("[DroppableSlot] Mismo slot, ignorando.");
            return;
        }

        Debug.Log($"[DroppableSlot] Drop valido: {source.Owner}[{source.Index}] → {target.Owner}[{target.Index}]");
        Mediator.HandleDrop(source, target);
    }
}

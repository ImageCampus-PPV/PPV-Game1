using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using UnityEngine;

public class PressurePlate : BaseEntity
{
    private uint entityPressingCount = 0;

    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    public override void Init()
    {
        base.Init();

        if (!TryGetComponent<Collider2D>(out Collider2D collider))
            collider = gameObject.AddComponent<BoxCollider2D>();

        collider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ++entityPressingCount;
        EventBus.Raise<OnPressurePlatePress>(ID);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        --entityPressingCount;

        if (entityPressingCount != 0)
            return;

        EventBus.Raise<OnPressurePlatePress>(ID);
    }
}

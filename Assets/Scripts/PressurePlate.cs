using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using UnityEngine;

public class PressurePlate : BaseEntity
{
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    uint entityPressingCount = 0;

    [SerializeField] private LayerMask layerMask;

    public override void Init()
    {
        base.Init();

        gameObject.layer = Mathf.RoundToInt(Mathf.Log(layerMask.value, 2));

        if (!TryGetComponent<Collider2D>(out Collider2D collider))
            collider = gameObject.AddComponent<BoxCollider2D>();

        collider.isTrigger = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ++entityPressingCount;
        EventBus.Raise<OnPressurePlatePress>(ID);
    }

    private void OnCollisionExit(Collision collision)
    {
        --entityPressingCount;

        if (entityPressingCount != 0)
            return;

        EventBus.Raise<OnPressurePlatePress>(ID);
    }
}

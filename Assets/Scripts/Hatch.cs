using UnityEngine;

public class Hatch : BaseEntity
{
    public PressurePlate[] pressurePlatesToOpen;

    private Collider2D collider;

    public override void Init()
    {
        base.Init();

        if (!TryGetComponent<Collider2D>(out collider))
            collider = gameObject.AddComponent<BoxCollider2D>();
    }

    public void OpenHatch()
    {
        collider.enabled = false;
        Debug.Log($"Hatch of id {ID} opened!!");
    }

    public void CloseHatch()
    {
        collider.enabled = true;
        Debug.Log($"Hatch of id {ID} closed!!");
    }
}

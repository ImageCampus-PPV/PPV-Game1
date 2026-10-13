using GreenAbyss.Entities;
using UnityEngine;

public class Hatch : GreenAbyss.Entities.BaseEntity
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
    }

    public void CloseHatch()
    {
        collider.enabled = true;
    }
}

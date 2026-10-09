using ImageCampus.ToolBox.Events;
using UnityEngine;

public class Nucleus : DamageableEntity
{
    private SpriteRenderer _spriteRenderer;
    public override Vector2 Center => transform.position;
    public override Vector2 Size => _spriteRenderer == null ? Vector2.zero : _spriteRenderer.bounds.size;

    public override void Init()
    {
        base.Init();

        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_spriteRenderer == null)
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (_spriteRenderer == null)
            Debug.LogError("No sprite renderer found for " + nameof(Nucleus));
    }

    public void OnHordeComplete()
    {
        if (IsDowned)
            Revive();
        else
            Heal(MaxHealth);
    }
}

public struct OnHordeEndedEvent : IEvent
{
    public void Assign(params object[] parameters)
    {
    }

    public void Reset()
    {
    }
}
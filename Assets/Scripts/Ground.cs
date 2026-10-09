using UnityEngine;

public class Ground : BaseEntity, IEntityDimensions
{
    private Collider2D _collider;
    public Vector2 Center => transform.position;
    public Vector2 Size => _collider == null ? Vector2.zero : _collider.bounds.size;

    public override void Init()
    {
        base.Init();
        _collider = GetComponent<Collider2D>();
    }
}
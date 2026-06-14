using UnityEngine;

public abstract class AttackStrategy : ScriptableObject
{
    [SerializeField] protected float hitboxRadius;
    [SerializeField] protected LayerMask enemyLayer;
    public float damage;
    public float attackSpeed;
    protected Character character;
    protected bool isExecuting;

    public bool IsExecuting => isExecuting;

    public virtual void Initialize(Character character)
    {
        this.character = character;
        isExecuting = false;
    }

    public abstract void Execute(Vector2 aimDir);
    public virtual void Cancel()
    {
        isExecuting = false;
    }
    public virtual void Tick() { }
    public virtual void FixedTick() { }

    protected void DealDamageToTargets(Collider2D[] hits, float damage)
    {
        foreach (var hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            damageable?.TakeDamage(damage);
        }
    }
}

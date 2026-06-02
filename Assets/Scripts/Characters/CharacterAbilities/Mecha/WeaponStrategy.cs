using UnityEngine;
using UnityEngine.InputSystem;


public abstract class WeaponStrategy : ScriptableObject
{
    [Header("Base Weapon Config")]
    [SerializeField] protected float damage;
    [SerializeField] protected float range;
    [SerializeField] protected float cooldown;
    [SerializeField] protected LayerMask enemyLayer;

    protected Character character;
    protected float lastFireTime = float.NegativeInfinity;

    public bool IsOnCooldown => Time.time - lastFireTime < cooldown;

    public virtual void Initialize(Character character)
    {
        this.character = character;
    }

    public abstract void OnPressed(InputAction.CallbackContext context, Vector2 aimDir);
    public virtual void OnReleased(InputAction.CallbackContext context, Vector2 aimDir) { }
    public virtual void Tick() { }
    public virtual void FixedTick() { }
    public virtual void Cancel() { }

    protected void DealDamageInArea(Vector2 center, float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, enemyLayer);

        foreach (var hit in hits)
        {
            hit.GetComponent<IDamageable>()?.TakeDamage(damage);
        }
    }
}

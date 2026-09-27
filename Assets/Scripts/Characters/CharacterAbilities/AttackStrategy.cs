using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackStrategy : ScriptableObject
{
    [SerializeField] protected float hitboxRadius;
    public float damage;
    public float attackSpeed;
    protected Character character;
    protected bool isExecuting;

    public bool IsExecuting => isExecuting;
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

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

    protected void DealDamageToTargets<EntityType>(IEnumerable<DamageableEntity> hits, float damage) where EntityType : DamageableEntity
    {
        foreach (DamageableEntity hit in hits)
            EventBus.Raise<OnCombatDamage>(hit.ID, damage);
    }
}

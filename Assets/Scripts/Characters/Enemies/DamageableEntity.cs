using GreenAbyss.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using UnityEngine;

public abstract class DamageableEntity : BaseEntity, IEntityDimensions
{
    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

    [SerializeField] protected float maxHealth = 100f;

    protected float currentHealth = 0.0f;

    public float CurrentHealth => currentHealth;

    public float HpPercent => CurrentHealth / maxHealth;

    public bool IsDowned => currentHealth <= 0.0f;

    public float MaxHealth => maxHealth;
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
    public abstract Vector2 Center { get; }
    public abstract Vector2 Size { get; }

    public override void Init()
    {
        base.Init();
        currentHealth = maxHealth;
    }

    virtual public void TakeDamage(float amount)
    {
        if (IsDowned)
            return;

        currentHealth = Mathf.Max(0.0f, currentHealth - amount);

        EventBus.Raise<OnHealthChange>(ID, currentHealth, maxHealth);

        if (currentHealth == 0.0f)
        {
            Debug.Log($"Damageable Entity {name} is dead. Removing");
            EntityRegistry.Remove(this);
        }
    }


    public void Heal(float amount)
    {
        if (IsDowned)
            return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        EventBus.Raise<OnHealthChange>(ID, currentHealth, maxHealth);
    }

    public void Revive(float maxHealthPercentage = 1.0f)
    {
        if (!IsDowned)
            return;

        currentHealth = maxHealth * maxHealthPercentage;
        EventBus.Raise<OnCharacterRevived>(ID);
        EventBus.Raise<OnHealthChange>(ID, currentHealth, maxHealth);
    }
}

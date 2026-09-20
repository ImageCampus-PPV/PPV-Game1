using GreenAbyss.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using UnityEngine;

public abstract class DamageableEntity : BaseEntity
{
    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

    [SerializeField] protected float _maxHealth = 100f;

    protected float _currentHealth = 0.0f;

    public float CurrentHealth => _currentHealth;

    public float HpPercent => CurrentHealth / _maxHealth;

    public bool IsDowned => _currentHealth <= 0.0f;

    public float MaxHealth => _maxHealth;
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    private void Awake()
    {
        _currentHealth = _maxHealth;
    }

    virtual public void TakeDamage(float amount)
    {
        if (IsDowned)
            return;

        _currentHealth = Mathf.Max(0.0f, _currentHealth - amount);

        EventBus.Raise<OnHealthChange>(ID, _currentHealth, _maxHealth);

        if (_currentHealth == 0.0f)
            EntityRegistry.Remove(this);
    }

    public void Heal(float amount)
    {
        if (IsDowned)
            return;

        _currentHealth = Mathf.Min(_currentHealth + amount, _maxHealth);
        EventBus.Raise<OnHealthChange>(ID, _currentHealth, _maxHealth);
    }

    public void Revive(float maxHealthPercentage = 1.0f)
    {
        if (!IsDowned)
            return;

        _currentHealth = _maxHealth * maxHealthPercentage;
        EventBus.Raise<OnCharacterRevived>(ID);
        EventBus.Raise<OnHealthChange>(ID, _currentHealth, _maxHealth);
    }
}

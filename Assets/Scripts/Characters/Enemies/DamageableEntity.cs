using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using UnityEngine;

public abstract class DamageableEntity : BaseEntity
{
    [SerializeField] protected float _maxHealth = 100f;

    protected float _currentHealth = 0.0f;

    public float CurrentHealth => _currentHealth;

    public float HpPercent => CurrentHealth / _maxHealth;

    public bool IsDowned => _currentHealth <= 0f;

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

        _currentHealth -= amount;

        if (_currentHealth <= 0f)
        {
            _currentHealth = 0f;
            EventBus.Raise<OnCharacterDowned>(ID);
        }

        EventBus.Raise<OnHealthChange>(ID, _currentHealth, _maxHealth);
    }

    public void Heal(float amount)
    {
        if (IsDowned)
            return;

        _currentHealth = Mathf.Min(_currentHealth + amount, _maxHealth);
        EventBus.Raise<OnHealthChange>(ID, _currentHealth, _maxHealth);
    }

    public void Revive(float maxHealthPercentage)
    {
        if (!IsDowned)
            return;

        _currentHealth = _maxHealth * maxHealthPercentage;
        EventBus.Raise<OnCharacterRevived>(ID);
        EventBus.Raise<OnHealthChange>(ID, _currentHealth, _maxHealth);
    }
}

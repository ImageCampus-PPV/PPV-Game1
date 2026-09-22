using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private DamageableEntity _damageableEntity;
    [SerializeField] private Slider _healthBar;
    [SerializeField] private float _defaultFullHealth = 100f;
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    private void Awake()
    {
        if (_healthBar == null)
            Debug.LogError("No slider provided to health bar");

        EventBus.Subscribe<OnHealthChange>(UpdateHealthbar);
        _healthBar.maxValue = _defaultFullHealth;
        _healthBar.minValue = 0f;
        _healthBar.value = _healthBar.maxValue;
    }

    private void Start()
    {
        if (_damageableEntity == null)
            _damageableEntity = GetComponent<DamageableEntity>();
    }

    private void UpdateHealthbar(in OnHealthChange onHealthChange)
    {
        if (_damageableEntity == null)
            _damageableEntity = GetComponent<DamageableEntity>();

        if (_damageableEntity == null)
        {
            Debug.LogError("No damageable entity to update health");
            return;
        }    

        if (onHealthChange.entityAffectedID != _damageableEntity.ID)
            return;

        if (_healthBar.maxValue != onHealthChange.maxHealth)
            _healthBar.maxValue = onHealthChange.maxHealth;

        if (onHealthChange.currentHealth < _healthBar.minValue || onHealthChange.currentHealth > _healthBar.maxValue)
        {
            Debug.LogWarning($"Tried setting health {onHealthChange.currentHealth}. " +
                             $"The value should be between {_healthBar.minValue} and {_healthBar.maxValue}");
            return;
        }

        _healthBar.value = onHealthChange.currentHealth;
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<OnHealthChange>(UpdateHealthbar);
    }
}
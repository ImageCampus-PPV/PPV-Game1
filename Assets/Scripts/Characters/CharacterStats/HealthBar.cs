using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Health _health;
    [SerializeField] private Slider _healthBar;
    [SerializeField] private float _defaultFullHealth = 100f;

    private void Awake()
    {
        if (_health == null)
            _health = GetComponent<Health>();

        if (_health == null)
            Debug.LogError("No health component provided to health bar.");

        if (_healthBar == null)
            Debug.LogError("No slider provided to health bar");

        _health.OnHealthChanged += UpdateHealthbar;
        _healthBar.maxValue = _defaultFullHealth;
        _healthBar.minValue = 0f;
        _healthBar.value = _healthBar.maxValue;
    }

    private void UpdateHealthbar(float current, float max)
    {
        if (_healthBar.maxValue != max)
            _healthBar.maxValue = max;

        if (current < _healthBar.minValue || current > _healthBar.maxValue)
        {
            Debug.LogWarning($"Tried setting health {current}. " +
                             $"The value should be between {_healthBar.minValue} and {_healthBar.maxValue}");
            return;
        }

        _healthBar.value = current;
    }
}
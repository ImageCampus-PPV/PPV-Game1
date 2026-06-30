using System;
using UnityEngine;


public class Health : MonoBehaviour
{
    [SerializeField] private MonoBehaviour _damageableComp;
    [SerializeField] private float _maxHealth = 100f;
    private float _currentHealth;

    public float MaxHealth => _maxHealth;
    public float CurrentHealth => _currentHealth;
    public bool IsDowned => _currentHealth <= 0f;
    public event Action<float, float> OnHealthChanged;
    public event Action<MonoBehaviour> OnDowned;
    public event Action OnRevived;
    private IDamageable _damageable;

    private void Awake()
    {
        _currentHealth = _maxHealth;

        if (_damageableComp == null)
            Debug.LogError("No damageable monobehaviour provided");

        if (!_damageableComp.TryGetComponent(out IDamageable damageable))
            Debug.LogError("User provided to health bar does not implement IDamageable interface.");

        _damageable = damageable;

        _damageable.OnTakeDamage += TakeDamage;
    }

    public void TakeDamage(float damage)
    {
        if (IsDowned)
            return;

        _currentHealth -= damage;

        if (_currentHealth <= 0f)
        {
            _currentHealth = 0f;
            Debug.Log($"{_damageableComp.name} is down");
            OnDowned?.Invoke(_damageableComp);
        }

        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    public void Heal(float amount)
    {
        if (IsDowned)
            return;

        _currentHealth = Mathf.Min(_currentHealth + amount, _maxHealth);
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    public void Revive(float maxHealthPercentage)
    {
        if (!IsDowned)
            return;

        _currentHealth = _maxHealth * maxHealthPercentage;
        OnRevived?.Invoke();
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    public void Reset()
    {
        bool wasDowned = IsDowned;
        _currentHealth = _maxHealth;
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

        if (wasDowned)
            OnRevived?.Invoke();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_damageableComp == null)
            return;

        if (_damageableComp is not IDamageable)
        {
            Debug.LogError("Damageable component provided does not implement IDamageable interface.");
        }
    }
#endif
}

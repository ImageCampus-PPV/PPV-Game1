using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float _maxHealth = 100f;
    private float _currentHealth;

    public float MaxHealth => _maxHealth;
    public float CurrentHealth => _currentHealth;
    public bool IsDowned => _currentHealth <= 0f;
    public uint OwnerID { get; private set; }
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    private void Awake()
    {
        _currentHealth = _maxHealth;

        OwnerID = GetComponent<DamageableEntity>().ID;

        EventBus.Subscribe<OnCombatDamage>(TakeDamage);
    }

    public void TakeDamage(in OnCombatDamage onCombatDamage)
    {
        if (IsDowned || onCombatDamage.entityToDamageID != OwnerID)
            return;

        _currentHealth -= onCombatDamage.damageToReceive;

        if (_currentHealth <= 0f)
        {
            _currentHealth = 0f;
            EventBus.Raise<OnCharacterDowned>(OwnerID);
        }

        EventBus.Raise<OnHealthChange>(OwnerID, _currentHealth, _maxHealth);
    }

    public void Heal(float amount)
    {
        if (IsDowned)
            return;

        _currentHealth = Mathf.Min(_currentHealth + amount, _maxHealth);
        EventBus.Raise<OnHealthChange>(OwnerID, _currentHealth, _maxHealth);
    }

    public void Revive(float maxHealthPercentage)
    {
        if (!IsDowned)
            return;

        _currentHealth = _maxHealth * maxHealthPercentage;
        EventBus.Raise<OnCharacterRevived>(OwnerID);
        EventBus.Raise<OnHealthChange>(OwnerID, _currentHealth, _maxHealth);
    }

    public void Reset()
    {
        bool wasDowned = IsDowned;
        _currentHealth = _maxHealth;
        EventBus.Raise<OnHealthChange>(OwnerID, _currentHealth, _maxHealth);

        if (wasDowned)
            EventBus.Raise<OnCharacterRevived>(OwnerID);
    }
}

public struct OnHealthChange : IEvent
{
    public uint entityAffectedID;
    public float currentHealth;
    public float maxHealth;

    public void Assign(params object[] parameters)
    {
        entityAffectedID = (uint)parameters[0];
        currentHealth = (float)parameters[1];
        maxHealth = (float)parameters[2];
    }

    public void Reset()
    {
        entityAffectedID = default(uint);
        currentHealth = default(float);
        maxHealth = default(float);
    }
}

public struct OnCharacterDowned : IEvent
{
    public uint entityDownedID;
    public void Assign(params object[] parameters)
    {
        entityDownedID = (uint)parameters[0];
    }

    public void Reset()
    {
        entityDownedID = default(uint);
    }
}

public struct OnCharacterRevived : IEvent
{
    public uint entityRevivedID;
    public void Assign(params object[] parameters)
    {
        entityRevivedID = (uint)parameters[0];
    }

    public void Reset()
    {
        entityRevivedID = default(uint);
    }
}

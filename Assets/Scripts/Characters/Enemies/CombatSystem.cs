using Assets.Scripts.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using UnityEngine;

public sealed class CombatSystem : IInitiable, IService, IDisposable
{
    [SerializeField] private float _downedSpeedMultiplier = 0.3f;

    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    public bool IsPersistance => false;

    public void Init()
    {
        EventBus.Subscribe<OnCombatDamage>(DealCombatDamage);
        EventBus.Subscribe<OnCharacterDowned>(HandleDowned);
        EventBus.Subscribe<OnCharacterRevived>(HandleRevived);
    }

    public void LateInit()
    {

    }

    private void DealCombatDamage(in OnCombatDamage callback)
    {
        EntityRegistry.GetAs<DamageableEntity>(callback.entityToDamageID).TakeDamage(callback.damageToReceive);
    }

    private void HandleRevived(in OnCharacterRevived onCharacterRevived)
    {
        Character damagedEntity = EntityRegistry.GetAs<Character>(onCharacterRevived.entityRevivedID);

        damagedEntity.IsBlockingAbilities = false;
        damagedEntity.ActiveMovement.SpeedMultiplier = 1f;
        Debug.Log("Revived.");
    }

    private void HandleDowned(in OnCharacterDowned onCharacterDowned)
    {
        Character damagedEntity = EntityRegistry.GetAs<Character>(onCharacterDowned.entityDownedID);

        damagedEntity.IsBlockingAbilities = true;
        damagedEntity.ActiveMovement.SpeedMultiplier = _downedSpeedMultiplier;
        Debug.Log("Downed. Abilities blocked. Movement multiplier: " + _downedSpeedMultiplier);
    }

    public void Dispose()
    {
        EventBus.Unsubscribe<OnCombatDamage>(DealCombatDamage);
        EventBus.Unsubscribe<OnCharacterDowned>(HandleDowned);
        EventBus.Unsubscribe<OnCharacterRevived>(HandleRevived);
    }
}

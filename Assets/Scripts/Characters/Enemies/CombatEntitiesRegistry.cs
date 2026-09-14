using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System.Collections.Generic;

public sealed class CombatEntitiesRegistry : IInitiable, IService
{
    private Dictionary<uint, DamageableEntity> _damageableEntities;
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    public bool IsPersistance => false;

    public void Init()
    {
        EventBus.Subscribe<OnCombatDamage>(DealCombatDamage);
    }
    
    public void LateInit()
    {
    }

    public void RegisterCombatEntity<DamageableEntityType>(uint entityID, DamageableEntityType entity) where DamageableEntityType : DamageableEntity
    {
        if (!_damageableEntities.TryAdd(entityID, entity))
        {
            _damageableEntities[entityID] = entity;
        }
    }
    private void DealCombatDamage(in OnCombatDamage callback)
    {
        if (_damageableEntities.ContainsKey(callback.EntityToDamageID))
            _damageableEntities[callback.EntityToDamageID].TakeDamage(callback.DamageToReceive);
    }
}

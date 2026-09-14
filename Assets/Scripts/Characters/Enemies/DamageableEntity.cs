using ImageCampus.ToolBox.Services;
using System;

public abstract class DamageableEntity : BaseEntity, IInitiable
{
    private CombatEntitiesRegistry CombatEntitiesRegistry => ServiceProvider.Instance.GetService<CombatEntitiesRegistry>();

    public void Init()
    {
        CombatEntitiesRegistry.RegisterCombatEntity<DamageableEntity>(ID, this);
    }

    public void LateInit()
    {
    }

    public abstract void TakeDamage(float amount);
}

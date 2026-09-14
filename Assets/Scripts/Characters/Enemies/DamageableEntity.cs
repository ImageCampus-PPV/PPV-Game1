using ImageCampus.ToolBox.Services;
using System;

public abstract class DamageableEntity : BaseEntity, IInitiable
{
    //TODO: remove these actions
    public abstract Action<float> OnTakeDamage { get; set; }
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

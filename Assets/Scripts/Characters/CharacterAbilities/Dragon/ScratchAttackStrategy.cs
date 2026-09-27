using GreenAbyss.Entities;
using ImageCampus.ToolBox.Services;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Attacks/Air Scratch (X)")]
public class ScratchAttackStrategy : AttackStrategy
{
    private float _currentAttackTimer;
    private Vector2 _attackDir;
    private RuntimeDebugVisual DebugVisual => ServiceProvider.Instance.GetService<RuntimeDebugVisual>();
    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

    public override void Execute(Vector2 aimDir)
    {
        if (character.IsGrounded)
            return;

        isExecuting = true;

        _attackDir = aimDir;

        _currentAttackTimer = attackSpeed;

        character.IsBlockingRotation = true;

        Vector2 attackPos = (Vector2)character.transform.position + (_attackDir * (hitboxRadius));
        DebugVisual.DrawCircle(attackPos, hitboxRadius, Color.crimson, _currentAttackTimer);
        DealDamageToTargets<Enemy>(EntityRegistry.GetAllEntitiesInRadius<Enemy>(attackPos, hitboxRadius), damage);
    }

    public override void Tick()
    {
        if (!isExecuting)
            return;

        _currentAttackTimer -= Time.deltaTime;

        if (_currentAttackTimer < 0f)
        {
            isExecuting = false;
            character.IsBlockingRotation = false;
        }
    }
}

using ImageCampus.ToolBox.Services;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Attacks/Ground Tail (Y)")]
public class TailAttackStrategy : AttackStrategy
{
    [SerializeField] float _hitboxSizeX = 3f; 
    [SerializeField] float _hitboxSizeY = 1.5f; 
    private float _currentAttackTimer;

    private RuntimeDebugVisual _debugVisual;

    public override void Execute(Vector2 aimDir)
    {
        if (!character.IsGrounded)
            return;

        isExecuting = true;

        _currentAttackTimer = attackSpeed;

        character.ApplyHVelocity(0);

        Vector2 attackPos = (Vector2)character.transform.position + (Vector2.up * (_hitboxSizeY / 2f));

        if(!_debugVisual)
            _debugVisual = ServiceProvider.Instance.GetService<RuntimeDebugVisual>();

        _debugVisual.DrawBox(attackPos, new(_hitboxSizeX, _hitboxSizeY), Color.magenta, attackSpeed);

        Collider2D[] hits = Physics2D.OverlapBoxAll(attackPos, new(_hitboxSizeX, _hitboxSizeY), enemyLayer);
        DealDamageToTargets(hits, damage);
    }

    public override void Tick()
    {
        if (!isExecuting)
            return;

        character.ApplyHVelocity(0);

        _currentAttackTimer -= Time.deltaTime;
        if (_currentAttackTimer < 0f)
        {
            isExecuting = false;
        }
    }
}


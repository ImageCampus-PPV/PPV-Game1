using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Attacks/Air Scratch (X)")]
public class ScratchAttackStrategy : AttackStrategy
{
    private float _currentAttackTimer;
    private Vector2 _attackDir;

    public override void Execute(Vector2 aimDir)
    {
        if (character.IsGrounded)
            return;

        isExecuting = true;

        _attackDir = aimDir;

        _currentAttackTimer = attackSpeed;

        Vector2 attackPos = (Vector2)character.transform.position + (_attackDir * (hitboxRadius));
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, hitboxRadius, enemyLayer);
        DealDamageToTargets(hits, damage);
    }

    public override void Tick()
    {
        if (!isExecuting)
            return;

        _currentAttackTimer -= Time.deltaTime;

        if (_currentAttackTimer < 0f)
        {
            isExecuting = false;
        }
    }
}

using UnityEngine;

public abstract class EnemyAttackStrategy : ScriptableObject
{
    [SerializeField] protected float _attackRange = 6f;
    [SerializeField] protected float _attackCooldown = 2f;
    [SerializeField] protected float _anticipationTime = 1f;

    public float AttackRange => _attackRange;
    public float AttackCooldown => _attackCooldown;
    public float AnticipationTime => _anticipationTime;

    public abstract bool CanAttack(Transform self, Transform target);
    public abstract void Execute(Transform self, Transform attackOffset, Transform target);
}

using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Attack/RangedProjectile")]
public class RangedProjectileAttackStrategy : EnemyAttackStrategy
{
    [SerializeField] private GameObject _projectilePrefab;

    public override bool CanAttack(IEnemyContext self, Transform target)
    {
        if (target == null) return false;
        float dist = Vector2.Distance(self.Transform.position, target.position);
        return dist <= _attackRange;
    }

    public override void Execute(IEnemyContext self, Transform target)
    {
        if (_projectilePrefab == null || self.AttackOffset == null || target == null)
            return;

        Vector2 dir = ((Vector2)(target.position - self.AttackOffset.position)).normalized;
        GameObject proj = Instantiate(_projectilePrefab, self.AttackOffset.position, Quaternion.identity);
        proj.GetComponent<EnemyProjectile>()?.SetDirection(dir);
    }
}

using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Attack/RangedProjectile")]
public class RangedProjectileAttackStrategy : EnemyAttackStrategy
{
    [SerializeField] private GameObject _projectilePrefab;
    public override bool CanAttack(Transform self, Transform target)
    {
        if (target == null)
            return false;

        float dist = Vector2.Distance(self.position, target.position);
        return dist <= _attackRange;
    }

    public override void Execute(Transform self, Transform attackOffset, Transform target)
    {
        if (_projectilePrefab == null || attackOffset == null || target == null)
            return;

        Vector2 dir = ((Vector2)(target.position - attackOffset.position)).normalized;
        GameObject proj = Instantiate(_projectilePrefab, attackOffset.position, Quaternion.identity);
        proj.GetComponent<EnemyProjectile>()?.SetDirection(dir);
    }
}

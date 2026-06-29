using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Condition/PlayerInRange")]
public class PlayerInRangeCondition : Condition<IEnemyContext>
{
    [SerializeField] private float _range = 10f;
    [SerializeField] private LayerMask _targetLayer;

    public override bool Evaluate(IEnemyContext context)
    {
        var target = context.ExecuteQuery(new FindTargetQuery(_range, _targetLayer));
        return target != null;
    }
}

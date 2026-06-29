using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Condition/PlayerOutOfRange")]
public class PlayerOutOfRangeCondition : Condition<IEnemyContext>
{
    [SerializeField] private float _range = 12f;
    [SerializeField] private LayerMask _targetLayer;

    public override bool Evaluate(IEnemyContext context)
    {
        var target = context.ExecuteQuery(new FindTargetQuery(_range, _targetLayer));
        return target == null;
    }
}

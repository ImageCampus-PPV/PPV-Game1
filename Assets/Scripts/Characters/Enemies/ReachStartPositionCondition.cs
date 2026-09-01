using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Condition/ReachStartPos")]
public class ReachStartPositionCondition : Condition<IEnemyContext>
{
    [SerializeField] private float _distanceMargin = 0.1f;

    public override bool Evaluate(IEnemyContext context)
    {
        return Vector2.Distance(context.Position, context.PositionOnSpawn) < _distanceMargin;
    }
}
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Behaviour/Chase")]
public class ChaseBehaviour : StateBehaviour<IEnemyContext>
{
    [SerializeField] private float _moveSpeed = 4f;
    [SerializeField] private float _targetLookRange = 100f;
    [SerializeField] private LayerMask _targetMask;

    public override BehaviourActions GetOnEnter(IEnemyContext context)
    {
        BehaviourActions actions = new BehaviourActions();
        actions.AddUpdateBehaviour(() => context.Execute(new ResumeMovementCommand()));
        return actions;
    }

    public override BehaviourActions GetOnTick(IEnemyContext context)
    {
        BehaviourActions actions = new BehaviourActions();
        actions.AddUpdateBehaviour(() =>
        {
            Transform target = context.ExecuteQuery(new FindTargetQuery(_targetLookRange, _targetMask));

            if (target != null)
                context.Execute(new MoveCommand(target.position, _moveSpeed));
            else
                Debug.Log("No target found for chasing");
        });
        return actions;
    }

    public override BehaviourActions GetOnExit(IEnemyContext context)
    {
        BehaviourActions actions = new BehaviourActions();
        actions.AddUpdateBehaviour(() => context.Execute(new StopMovementCommand()));
        return actions;
    }
}

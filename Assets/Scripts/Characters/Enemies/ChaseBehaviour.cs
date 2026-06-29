using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Behaviour/Chase")]
public class ChaseBehaviour : StateBehaviour<IEnemyContext>
{
    [SerializeField] private float _moveSpeed = 4f;

    public override BehaviourActions GetOnEnter(IEnemyContext context)
    {
        var actions = new BehaviourActions();
        actions.AddUpdateBehaviour(() => context.Execute(new ResumeMovementCommand()));
        return actions;
    }

    public override BehaviourActions GetOnTick(IEnemyContext context)
    {
        var actions = new BehaviourActions();
        actions.AddUpdateBehaviour(() =>
        {
            var target = context.ExecuteQuery(new FindTargetQuery(100f, LayerMask.GetMask("Player")));
            if (target != null)
            {
                context.Execute(new MoveCommand(target.position, _moveSpeed));
            }
        });
        return actions;
    }

    public override BehaviourActions GetOnExit(IEnemyContext context)
    {
        var actions = new BehaviourActions();
        actions.AddUpdateBehaviour(() => context.Execute(new StopMovementCommand()));
        return actions;
    }
}

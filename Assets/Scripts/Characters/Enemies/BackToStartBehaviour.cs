using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Behaviour/BackToStart")]
public class BackToStartBehaviour : StateBehaviour<IEnemyContext>
{
    [SerializeField] private float _moveSpeed = 2f;
    private bool _initialied = false;
    private Vector2 _startPos;
    private bool _reachedDestination = false;

    public override BehaviourActions GetOnEnter(IEnemyContext context)
    {
        BehaviourActions actions = new BehaviourActions();
        actions.AddUpdateBehaviour(() =>
        {
            _startPos = context.PositionOnSpawn;
        });
        return actions;
    }

    public override BehaviourActions GetOnTick(IEnemyContext context)
    {
        BehaviourActions actions = new BehaviourActions();
        actions.AddUpdateBehaviour(() =>
        {
            if (!_reachedDestination)
                context.Execute(new MoveCommand(_startPos, _moveSpeed));

            if (Vector2.Distance(_startPos, context.Position) < 0.1f)
            {
                context.Execute(new StopMovementCommand());
                _reachedDestination = true;
            }
            else
                _reachedDestination = false;
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
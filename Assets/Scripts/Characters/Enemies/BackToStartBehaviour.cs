using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Behaviour/BackToStart")]
public class BackToStartBehaviour : StateBehaviour<IEnemyContext>
{
    [SerializeField] private float _moveSpeed = 2f;

    public override BehaviourActions GetOnEnter(IEnemyContext context)
    {
        BehaviourActions actions = new BehaviourActions();
        actions.AddUpdateBehaviour(() =>
        {
            //Debug.Log("Enemy " + context.Transform.name + " heading to " + _startPos + ". Current pos: " + context.Position);
        });
        return actions;
    }

    public override BehaviourActions GetOnTick(IEnemyContext context)
    {
        BehaviourActions actions = new BehaviourActions();
        actions.AddUpdateBehaviour(() =>
        {
            //Debug.Log("Enemy " + context.Transform.name + " heading to " + context.PositionOnSpawn + ". Current pos: " + context.Position);
            //Debug.Log("Move speed: " + _moveSpeed);
            context.Execute(new MoveCommand(context.PositionOnSpawn, _moveSpeed));
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
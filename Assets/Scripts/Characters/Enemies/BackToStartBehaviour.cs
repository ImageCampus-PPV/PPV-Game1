using GreenAbyss.FSM;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Behaviour/BackToStart")]
public class BackToStartBehaviour : StateBehaviour<IEnemyContext>
{
    [SerializeField] private float _moveSpeed = 2f;

    public override GreenAbyss.FSM.BehaviourActions GetOnEnter(IEnemyContext context)
    {
        GreenAbyss.FSM.BehaviourActions actions = new GreenAbyss.FSM.BehaviourActions();
        actions.AddUpdateBehaviour(() =>
        {
            //Debug.Log("Enemy " + context.Transform.name + " heading to " + _startPos + ". Current pos: " + context.Position);
        });
        return actions;
    }

    public override GreenAbyss.FSM.BehaviourActions GetOnTick(IEnemyContext context)
    {
        GreenAbyss.FSM.BehaviourActions actions = new GreenAbyss.FSM.BehaviourActions();
        actions.AddUpdateBehaviour(() =>
        {
            //Debug.Log("Enemy " + context.Transform.name + " heading to " + context.PositionOnSpawn + ". Current pos: " + context.Position);
            //Debug.Log("Move speed: " + _moveSpeed);
            context.Execute(new MoveCommand(context.PositionOnSpawn, _moveSpeed));
        });
        return actions;
    }

    public override GreenAbyss.FSM.BehaviourActions GetOnExit(IEnemyContext context)
    {
        GreenAbyss.FSM.BehaviourActions actions = new GreenAbyss.FSM.BehaviourActions();
        actions.AddUpdateBehaviour(() => context.Execute(new StopMovementCommand()));
        return actions;
    }
}
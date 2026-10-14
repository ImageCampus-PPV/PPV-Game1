using GreenAbyss.FSM;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Behaviour/Chase")]
public class ChaseBehaviour : StateBehaviour<IEnemyContext>
{
    [SerializeField] private float _moveSpeed = 4f;
    [SerializeField] private float _targetLookRange = 100f;
    [SerializeField] private LayerMask _targetMask;

    public override GreenAbyss.FSM.BehaviourActions GetOnEnter(IEnemyContext context)
    {
        GreenAbyss.FSM.BehaviourActions actions = new GreenAbyss.FSM.BehaviourActions();
        actions.AddUpdateBehaviour(() => context.Execute(new ResumeMovementCommand()));
        return actions;
    }

    public override GreenAbyss.FSM.BehaviourActions GetOnTick(IEnemyContext context)
    {
        GreenAbyss.FSM.BehaviourActions actions = new GreenAbyss.FSM.BehaviourActions();
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

    public override GreenAbyss.FSM.BehaviourActions GetOnExit(IEnemyContext context)
    {
        GreenAbyss.FSM.BehaviourActions actions = new GreenAbyss.FSM.BehaviourActions();
        actions.AddUpdateBehaviour(() => context.Execute(new StopMovementCommand()));
        return actions;
    }
}

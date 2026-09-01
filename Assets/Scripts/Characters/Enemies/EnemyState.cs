using System;

public class EnemyState<MarkerType> : State where MarkerType : new()
{
    public override BehaviourActions GetOnEnterBehaviour(params object[] parameters)
    {
        IEnemyContext context = (IEnemyContext)parameters[0];
        TransitionEvaluator evaluator = (TransitionEvaluator)parameters[1];
        IStateBehaviour<IEnemyContext> behaviour = (IStateBehaviour<IEnemyContext>)parameters[2];
        string stateName = (string)parameters[3];

        if (context is IStateDebugInfo debug)
            debug.CurrentStateName = stateName;

        return behaviour.GetOnEnter(context);
    }

    public override BehaviourActions GetOnTickBehaviour(params object[] parameters)
    {
        IEnemyContext context = (IEnemyContext)parameters[0];
        TransitionEvaluator evaluator = (TransitionEvaluator)parameters[1];
        IStateBehaviour<IEnemyContext> behaviour = (IStateBehaviour<IEnemyContext>)parameters[2];
        string stateName = (string)parameters[3];

        BehaviourActions actions = behaviour.GetOnTick(context);
        actions.SetTransitionBehaviour(() =>
        {
            if (evaluator.TryGetTransition(context, stateName, out string targetStateName))
            {
                Type targetType = evaluator.GetStateType(targetStateName);
                if (targetType != null)
                    changeState?.Invoke(targetType);
            }
        });
        return actions;
    }

    public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
    {
        IEnemyContext context = (IEnemyContext)parameters[0];
        IStateBehaviour<IEnemyContext> behaviour = (IStateBehaviour<IEnemyContext>)parameters[2];
        return behaviour.GetOnExit(context);
    }
}

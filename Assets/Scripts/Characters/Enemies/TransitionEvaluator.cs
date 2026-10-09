using System;
using System.Collections.Generic;

public class TransitionEvaluator
{
    private readonly Dictionary<string, Type> _stateNameToType;
    private readonly Dictionary<string, List<(ICondition<IEnemyContext> cond, string toState)>> _transitionsByState;
    private readonly List<(ICondition<IEnemyContext> cond, string toState)> _anyTransitions;

    public TransitionEvaluator(StateMachineConfig config, Dictionary<string, Type> nameToTypeMapping)
    {
        _stateNameToType = nameToTypeMapping;

        _transitionsByState = new Dictionary<string, List<(ICondition<IEnemyContext>, string)>>();
        foreach (StateMachineConfig.StateTransitionGroup group in config.transitionsByState)
        {
            List<(ICondition<IEnemyContext>, string)> conditionToState = new List<(ICondition<IEnemyContext>, string)>();

            foreach (StateMachineConfig.TransitionEntry entry in group.transitions)
                conditionToState.Add((entry.condition, entry.toState));

            _transitionsByState[group.stateName] = conditionToState;
        }

        _anyTransitions = new List<(ICondition<IEnemyContext>, string)>();
        foreach (StateMachineConfig.AnyTransitionEntry entry in config.anyTransitions)
            _anyTransitions.Add((entry.condition, entry.toState));
    }

    public bool TryGetTransition(IEnemyContext context, string currentStateName, out string targetStateName)
    {
        foreach ((ICondition<IEnemyContext> condition, string to) in _anyTransitions)
        {
            if (condition.Evaluate(context))
            {
                targetStateName = to;
                return true;
            }
        }
        
        if (_transitionsByState.TryGetValue(currentStateName, out List<(ICondition<IEnemyContext> cond, string toState)> list))
        {
            foreach ((ICondition<IEnemyContext> cond, string to) in list)
            {
                if (cond.Evaluate(context))
                {
                    targetStateName = to;
                    return true;
                }
            }
        }

        targetStateName = null;
        return false;
    }

    public Type GetStateType(string stateName)
    {
        _stateNameToType.TryGetValue(stateName, out Type type);
        return type;
    }
}

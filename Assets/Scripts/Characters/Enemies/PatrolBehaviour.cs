using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Behaviour/Patrol")]
public class PatrolBehaviour : StateBehaviour<IEnemyContext>
{
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _range = 3f;
    [SerializeField] private float _pauseDuration = 0.5f;

    private Vector2 _startPos;
    private int _direction = 1;
    private float _pauseTimer;

    public override BehaviourActions GetOnEnter(IEnemyContext context)
    {
        var actions = new BehaviourActions();
        actions.AddUpdateBehaviour(() =>
        {
            _startPos = context.Position;
            _direction = 1;
            _pauseTimer = 0;
        });
        return actions;
    }

    public override BehaviourActions GetOnTick(IEnemyContext context)
    {
        var actions = new BehaviourActions();
        actions.AddUpdateBehaviour(() =>
        {
            if (_pauseTimer > 0)
            {
                _pauseTimer -= Time.deltaTime;
                context.Execute(new StopMovementCommand());
                return;
            }

            float targetX = _startPos.x + _direction * _range;
            float diff = targetX - context.Position.x;

            if (Mathf.Abs(diff) < 0.05f)
            {
                _direction *= -1;
                _pauseTimer = _pauseDuration;
                context.Execute(new StopMovementCommand());
            }
            else
            {
                context.Execute(new MoveCommand(
                    new Vector2(targetX, context.Position.y),
                    _moveSpeed
                ));
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


public class StateMarker<MarkerType, StateContextType> where MarkerType : StateBehaviour<StateContextType> where StateContextType : IStateContext
{ 
}

public class TransitionEvaluator
{
    private Dictionary<string, Type> _stateNameToType;
    private Dictionary<string, List<(ICondition<IEnemyContext> cond, string toState)>> _transitionsByState;
    private List<(ICondition<IEnemyContext> cond, string toState)> _anyTransitions;

    public TransitionEvaluator(
        StateMachineConfig config,
        Dictionary<string, Type> nameToTypeMapping
    )
    {
        _stateNameToType = nameToTypeMapping;

        _transitionsByState = new Dictionary<string, List<(ICondition<IEnemyContext>, string)>>();
        foreach (var group in config.transitionsByState)
        {
            var list = new List<(ICondition<IEnemyContext>, string)>();
            foreach (var entry in group.transitions)
                list.Add((entry.condition, entry.toState));
            _transitionsByState[group.stateName] = list;
        }

        _anyTransitions = new List<(ICondition<IEnemyContext>, string)>();
        foreach (var entry in config.anyTransitions)
            _anyTransitions.Add((entry.condition, entry.toState));
    }

    public bool TryGetTransition(IEnemyContext context, string currentStateName, out string targetStateName)
    {
        foreach (var (cond, to) in _anyTransitions)
        {
            if (cond.Evaluate(context))
            {
                targetStateName = to;
                return true;
            }
        }
        
        if (_transitionsByState.TryGetValue(currentStateName, out var list))
        {
            foreach (var (cond, to) in list)
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
        _stateNameToType.TryGetValue(stateName, out var type);
        return type;
    }
}

public class EnemyState<MarkerType> : State where MarkerType : new()
{
    public override BehaviourActions GetOnEnterBehaviour(params object[] parameters)
    {
        var context = (IEnemyContext)parameters[0];
        var evaluator = (TransitionEvaluator)parameters[1];
        var behaviour = (IStateBehaviour<IEnemyContext>)parameters[2];
        var stateName = (string)parameters[3];

        if (context is IStateDebugInfo debug)
            debug.CurrentStateName = stateName;

        return behaviour.GetOnEnter(context);
    }

    public override BehaviourActions GetOnTickBehaviour(params object[] parameters)
    {
        var context = (IEnemyContext)parameters[0];
        var evaluator = (TransitionEvaluator)parameters[1];
        var behaviour = (IStateBehaviour<IEnemyContext>)parameters[2];
        var stateName = (string)parameters[3];

        var actions = behaviour.GetOnTick(context);
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
        var context = (IEnemyContext)parameters[0];
        var evaluator = (TransitionEvaluator)parameters[1];
        var behaviour = (IStateBehaviour<IEnemyContext>)parameters[2];
        var stateName = (string)parameters[3];
        return behaviour.GetOnExit(context);
    }
}

public interface IStateDebugInfo
{
    string CurrentStateName { get; set; }
}
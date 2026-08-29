using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Health))]
public class Enemy : MonoBehaviour, IEnemyContext, IDamageable, IStunnable, IStatusEffectReceiver, IStateDebugInfo
{
    [Header("State Machine")]
    [SerializeField] private StateMachineConfig _stateMachineConfig;

    [Header("Attack")]
    [SerializeField] private Transform _attackOffset;
    [SerializeField] private LayerMask _targetLayers;

    [Header("Flocking stats")]
    [SerializeField] private FlockingSettings _flockingSettings;
    [SerializeField] private LayerMask _identityLayer;
    [SerializeField] private LayerMask _obstacleLayers;

    private Vector2 _positionOnSpawn;
    private Rigidbody2D _rb;
    private Health _health;
    private DamageResponse _damageResponse;
    //TODO: separate effects logic
    private List<StatusEffect> _effects = new List<StatusEffect>();

    private FSM _fsm;
    private TransitionEvaluator _evaluator;

    private Dictionary<Type, object> _commandHandlers = new Dictionary<Type, object>();

    //TODO: make sure ALL ACTIONS GET CLEANED UP (and Funcs). I do not clean it here (my bad).
    public event Action<ICommand> OnCommandExecuted;

    public Transform Transform => transform;
    public Vector2 Position => transform.position;
    public float Health => _health.CurrentHealth;
    public float MaxHealth => _health.MaxHealth;
    public Transform AttackOffset => _attackOffset;
    public Vector2 PositionOnSpawn => _positionOnSpawn;

    public bool IsStunned { get; set; }
    public Action<float> OnTakeDamage { get; set; }
    public List<StatusEffect> ActiveEffects => _effects;

    public string CurrentStateName { get; set; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _health = GetComponent<Health>();
        _damageResponse = GetComponent<DamageResponse>();
        _positionOnSpawn = transform.position;
        Debug.Log("Position on spawn of enemy " + name + ": " + _positionOnSpawn);

        FlockingMovement movement = new FlockingMovement(_flockingSettings,
                                    new SeekSteering(_flockingSettings),
                                    new SeparationSteering(
                                        _identityLayer,
                                        _flockingSettings),
                                    new ObstacleAvoidanceSteering(
                                        _obstacleLayers,
                                        _flockingSettings),
                                    new WanderSteering(_flockingSettings)
                                    );

        RegisterCommandHandler(new MoveCommandHandler(_rb, movement));
        RegisterCommandHandler(new StopMovementCommandHandler(_rb));

        //TODO: Separate this state machine part (so it's more of a plug-in than something accumulated in the Awake)

        Dictionary<string, Type> stateNameToType = new Dictionary<string, Type>();
        foreach (StateMachineConfig.StateEntry entry in _stateMachineConfig.states)
        {
            Type marker = entry.behaviour.GetType();
            Type stateType = typeof(EnemyState<>).MakeGenericType(marker);
            stateNameToType[entry.stateName] = stateType;
        }

        _evaluator = new TransitionEvaluator(_stateMachineConfig, stateNameToType);

        Type defaultStateType = stateNameToType[_stateMachineConfig.DefaultState];
        _fsm = new FSM(defaultStateType);

        MethodInfo addStateMethod = typeof(FSM).GetMethod("AddState");
        foreach (StateMachineConfig.StateEntry entry in _stateMachineConfig.states)
        {
            Type stateType = stateNameToType[entry.stateName];
            MethodInfo genericAdd = addStateMethod.MakeGenericMethod(stateType);

            Func<object[]> onTick = () => new object[] { this, _evaluator, entry.behaviour, entry.stateName };
            Func<object[]> onEnter = () => new object[] { this, _evaluator, entry.behaviour, entry.stateName };
            Func<object[]> onExit = () => new object[] { this, _evaluator, entry.behaviour, entry.stateName };

            genericAdd.Invoke(_fsm, new object[] { onTick, onEnter, onExit });
        }

        _fsm.Transition(defaultStateType);
    }

    private void Update()
    {
        for (int i = _effects.Count - 1; i >= 0; i--)
        {
            _effects[i].Tick(this, Time.deltaTime);
            if (_effects[i].IsFinished)
                _effects.RemoveAt(i);
        }

        IsStunned = HasEffect<StunStatusEffect>();

        if (IsStunned)
            return;

        _fsm.Tick();
    }

    //TODO: Make this more readable and maybe separate it from enemy (all the command-handling logic should be separate)
    public void Execute<CommandType>(CommandType command) where CommandType : ICommand
    {
        if (_commandHandlers.TryGetValue(typeof(CommandType), out object handler))
        {
            ((ICommandHandler<CommandType>)handler).Execute(command, this);
            OnCommandExecuted?.Invoke(command);
        }
        else
            Debug.LogWarning($"No handler for {typeof(CommandType).Name}");
    }

    //TODO: query logic might be a little hardcoded, could be improved.
    public ResultType ExecuteQuery<ResultType>(ICommandQuery<ResultType> query)
    {
        if (query is FindTargetQuery find)
            return (ResultType)(object)TargetSelector.GetBestTarget(transform.position, find.Range, find.TargetLayer);

        throw new NotSupportedException($"Query {query.GetType()} not supported.");
    }

    public void RegisterCommandHandler<CommandType>(ICommandHandler<CommandType> handler) where CommandType : ICommand
    {
        _commandHandlers[typeof(CommandType)] = handler;
    }

    private class MoveCommandHandler : ICommandHandler<MoveCommand>
    {
        private readonly Rigidbody2D _rb;
        private readonly IMovementSteering _steering;

        public MoveCommandHandler(Rigidbody2D rb, IMovementSteering steering)
        {
            _rb = rb;
            _steering = steering;
        }

        public void Execute(MoveCommand command, IStateContext context)
        {
            if (!command.Target.HasValue)
            {
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            _rb.linearVelocity = _steering.GetDesiredVelocity(_rb, command.Target.Value, command.Speed);
        }
    }

    private class StopMovementCommandHandler : ICommandHandler<StopMovementCommand>
    {
        private Rigidbody2D _rb;
        public StopMovementCommandHandler(Rigidbody2D rb) => _rb = rb;
        public void Execute(StopMovementCommand command, IStateContext context)
        {
            _rb.linearVelocity = Vector2.zero;
        }
    }

    public void TakeDamage(float damage)
    {
        _damageResponse?.ReactToDamage(damage);
        OnTakeDamage?.Invoke(damage);
    }
    public void ApplyEffect(StatusEffect effect)
    {
        _effects.Add(effect);
    }

    public bool HasEffect<EffectType>() where EffectType : StatusEffect
    {
        foreach (StatusEffect effect in _effects)
            if (effect is EffectType)
                return true;
        return false;
    }

    public void StopMovement()
    {
        Execute(new StopMovementCommand());
    }
}

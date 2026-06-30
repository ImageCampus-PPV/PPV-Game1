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

    private Rigidbody2D _rb;
    private Health _health;
    private DamageResponse _damageResponse;
    private List<StatusEffect> _effects = new List<StatusEffect>();

    private FSM _fsm;
    private TransitionEvaluator _evaluator;

    private Dictionary<object, object> _dataStore = new Dictionary<object, object>();
    private Dictionary<Type, object> _commandHandlers = new Dictionary<Type, object>();

    public event Action<ICommand> OnCommandExecuted;

    public Transform Transform => transform;
    public Vector2 Position => transform.position;
    public float Health => _health.CurrentHealth;
    public float MaxHealth => _health.MaxHealth;
    public Transform AttackOffset => _attackOffset;

    public bool IsStunned { get; set; }
    public Action<float> OnTakeDamage { get; set; }
    public List<StatusEffect> ActiveEffects => _effects;

    public string CurrentStateName { get; set; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _health = GetComponent<Health>();
        _damageResponse = GetComponent<DamageResponse>();

        var movement = new FlockingMovement(
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
        RegisterCommandHandler(new ResumeMovementCommandHandler(_rb));

        var stateNameToType = new Dictionary<string, Type>();
        foreach (var entry in _stateMachineConfig.states)
        {
            Type marker = entry.behaviour.GetType();
            Type stateType = typeof(EnemyState<>).MakeGenericType(marker);
            stateNameToType[entry.stateName] = stateType;
        }

        _evaluator = new TransitionEvaluator(_stateMachineConfig, stateNameToType);

        Type defaultStateType = stateNameToType[_stateMachineConfig.DefaultState];
        _fsm = new FSM(defaultStateType);

        MethodInfo addStateMethod = typeof(FSM).GetMethod("AddState");
        foreach (var entry in _stateMachineConfig.states)
        {
            Type stateType = stateNameToType[entry.stateName];
            var genericAdd = addStateMethod.MakeGenericMethod(stateType);

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

    public ResultType ExecuteQuery<ResultType>(ICommandQuery<ResultType> query)
    {
        if (query is FindTargetQuery find)
            return (ResultType)(object)TargetSelector.GetBestTarget(transform.position, find.Range, find.TargetLayer);

        throw new NotSupportedException($"Query {query.GetType()} not supported.");
    }

    public void SetData<T>(StateDataKey<T> key, T value)
    {
        _dataStore[key] = value;
    }

    public T GetData<T>(StateDataKey<T> key)
    {
        return _dataStore.TryGetValue(key, out object v) ? (T)v : default;
    }

    public bool TryGetData<T>(StateDataKey<T> key, out T value)
    {
        if (_dataStore.TryGetValue(key, out object v))
        {
            value = (T)v; return true;
        }
        value = default;
        return false;
    }

    public void RegisterCommandHandler<CommandType>(ICommandHandler<CommandType> handler) where CommandType : ICommand
    {
        _commandHandlers[typeof(CommandType)] = handler;
    }

    private class MoveCommandHandler : ICommandHandler<MoveCommand>
    {
        private readonly Rigidbody2D _rb;
        private readonly IMovementSteering _steering;

        public MoveCommandHandler(
            Rigidbody2D rb,
            IMovementSteering steering)
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

    private class ResumeMovementCommandHandler : ICommandHandler<ResumeMovementCommand>
    {
        private Rigidbody2D _rb;
        public ResumeMovementCommandHandler(Rigidbody2D rb)
        {
            _rb = rb;
        }

        public void Execute(ResumeMovementCommand command, IStateContext context)
        {
            //TODO: Remove if unused.
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
        foreach (var e in _effects)
            if (e is EffectType)
                return true;
        return false;
    }

    public void StopMovement()
    {
        Execute(new StopMovementCommand());
    }
}

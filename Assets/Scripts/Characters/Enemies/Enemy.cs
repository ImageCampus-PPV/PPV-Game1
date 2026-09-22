using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class Enemy : DamageableEntity, IEnemyContext, IStunnable, IStatusEffectReceiver, IStateDebugInfo
{
    [Header("State Machine")]
    [SerializeField] private StateMachineConfig _defaultStateMachineConfig;
    [SerializeField] private StateMachineConfig _hordeModeStateMachineConfig;

    [Header("Attack")]
    [SerializeField] private Transform _attackOffset;
    [SerializeField] private LayerMask _targetLayers;
    [SerializeField] private bool _damageOnContact = false;
    [SerializeField] private List<DamageBehaviourConfig> _damageBehaviourConfigs;

    [Header("Steering stats")]
    [SerializeField] private SteeringSettings _steeringSettings;
    [SerializeField] private LayerMask _identityLayer;
    [SerializeField] private LayerMask _obstacleLayers;


    private Vector2 _positionOnSpawn;
    private Rigidbody2D _rb;
    private DamageResponse _damageResponse;
    //TODO: separate effects logic
    private List<StatusEffect> _effects = new List<StatusEffect>();
    private List<DamageBehaviour> _damageBehaviours;

    private FSM _fsm;
    private TransitionEvaluator _evaluator;

    private Dictionary<Type, object> _commandHandlers = new Dictionary<Type, object>();

    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
    public Transform Transform => transform;
    public Vector2 Position => transform.position;
    public Transform AttackOffset => _attackOffset;
    public Vector2 PositionOnSpawn => _positionOnSpawn;

    public bool IsStunned { get; set; }
    public List<StatusEffect> ActiveEffects => _effects;

    public string CurrentStateName { get; set; }

    private void Awake()
    {
        _damageBehaviours = new List<DamageBehaviour>();

        _rb = GetComponent<Rigidbody2D>();
        _damageResponse = GetComponent<DamageResponse>();
        _positionOnSpawn = transform.position;

        SteeringMovement movement = new SteeringMovement(_steeringSettings,
                                    new SeekSteering(_steeringSettings),
                                    new SeparationSteering(
                                        _identityLayer,
                                        _steeringSettings),
                                    new ObstacleAvoidanceSteering(
                                        _obstacleLayers,
                                        _steeringSettings),
                                    new WanderSteering(_steeringSettings)
                                    );

        RegisterCommandHandler(new MoveCommandHandler(_rb, movement));
        RegisterCommandHandler(new StopMovementCommandHandler(_rb));

        InitStateMachine(_defaultStateMachineConfig);
    }

    private void InitStateMachine(StateMachineConfig stateMachineConfig)
    {
        Dictionary<string, Type> stateNameToType = new Dictionary<string, Type>();
        foreach (StateMachineConfig.StateEntry entry in stateMachineConfig.states)
        {
            Type marker = entry.behaviour.GetType();
            Type stateType = typeof(EnemyState<>).MakeGenericType(marker);
            stateNameToType[entry.stateName] = stateType;
        }

        _evaluator = new TransitionEvaluator(stateMachineConfig, stateNameToType);

        Type defaultStateType = stateNameToType[stateMachineConfig.DefaultState];
        _fsm = new FSM(defaultStateType);

        MethodInfo addStateMethod = typeof(FSM).GetMethod(nameof(FSM.AddState));
        foreach (StateMachineConfig.StateEntry entry in stateMachineConfig.states)
        {
            StateBehaviour<IEnemyContext> stateBehaviourInstance = Instantiate(entry.behaviour);

            Type stateType = stateNameToType[entry.stateName];
            MethodInfo genericAdd = addStateMethod.MakeGenericMethod(stateType);

            Func<object[]> onTick = () => new object[] { this, _evaluator, stateBehaviourInstance, entry.stateName };
            Func<object[]> onEnter = () => new object[] { this, _evaluator, stateBehaviourInstance, entry.stateName };
            Func<object[]> onExit = () => new object[] { this, _evaluator, stateBehaviourInstance, entry.stateName };

            genericAdd.Invoke(_fsm, new object[] { onTick, onEnter, onExit });
        }

        _fsm.Transition(defaultStateType);

        foreach (DamageBehaviourConfig config in _damageBehaviourConfigs)
        {
            _damageBehaviours.Add(config.CreateBehaviour());
        }
    }

    public void SwitchToHordeMode()
    {
        InitStateMachine(_hordeModeStateMachineConfig);
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

        foreach (DamageBehaviour damageBehaviour in _damageBehaviours)
        {
            damageBehaviour.Tick(Time.deltaTime);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        foreach (DamageBehaviour damageBehaviour in _damageBehaviours)
            damageBehaviour.OnCollisionStay(collision);
    }

    //TODO: Make this more readable and maybe separate it from enemy (all the command-handling logic should be separate)
    public void Execute<CommandType>(CommandType command) where CommandType : ICommand
    {
        if (_commandHandlers.TryGetValue(typeof(CommandType), out object handler))
        {
            ((ICommandHandler<CommandType>)handler).Execute(command, this);
        }
        else
            Debug.LogWarning($"No handler for {typeof(CommandType).Name}");
    }

    //TODO: query logic might be a little hardcoded, could be improved.
    public ResultType ExecuteQuery<ResultType>(ICommandQuery<ResultType> query)
    {
        if (query is FindTargetQuery find)
            //TODO: fix this boxing!
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

    public override void TakeDamage(float damage)
    {
        _damageResponse?.ReactToDamage(damage);
        base.TakeDamage(damage);
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

    public override void Dispose()
    {
        base.Dispose();
        _fsm.Dispose();
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour, IDamageable, IStunnable, IStatusEffectReceiver
{
    [SerializeField] private EnemyMovementStrategy _movementStrategy;
    [SerializeField] private EnemyAttackStrategy _attackStrategy;
    [SerializeField] private float _attackRange;
    [SerializeField] private Transform _attackOffset;
    [SerializeField] private LayerMask _toAttack;

    private DamageResponse _reaction;
    private readonly List<StatusEffect> _effects = new();
    private Rigidbody2D _rb;
    private Transform _target;
    private float _attackCooldownTimer;
    private bool _isAnticipating;
    private float _anticipationTimer;
    public List<StatusEffect> ActiveEffects => _effects;

    public bool IsStunned { get; set; }
    public Action<float> OnTakeDamage { get; set; }

    private void Awake()
    {
        _reaction = GetComponent<DamageResponse>();
        if (_reaction == null)
            Debug.LogWarning($"The enemy {gameObject.name} does not have a reaction to damage assigned.");

        _rb = GetComponent<Rigidbody2D>();

        if (_movementStrategy != null)
            _movementStrategy = Instantiate(_movementStrategy);
        if (_attackStrategy != null)
            _attackStrategy = Instantiate(_attackStrategy);

        _movementStrategy?.Initialize(_rb, transform.position);
    }
    private void Update()
    {
        IsStunned = false;
        EffectsTick();

        if (IsStunned)
            return;

        ExecuteLogic();
    }

    private void ExecuteLogic()
    {
        _target = FindTarget();

        if (_attackCooldownTimer > 0)
            _attackCooldownTimer -= Time.deltaTime;

        if (_isAnticipating)
        {
            _anticipationTimer -= Time.deltaTime;

            if (_anticipationTimer <= 0)
            {
                _isAnticipating = false;
                _attackStrategy?.Execute(transform, _attackOffset, _target);
                _attackCooldownTimer = _attackStrategy.AttackCooldown;
            }
            return;
        }

        if (_target != null && _attackCooldownTimer <= 0 && _attackStrategy != null)
        {
            if (_attackStrategy.CanAttack(transform, _target))
            {
                _movementStrategy?.Stop();
                _isAnticipating = true;
                _anticipationTimer = _attackStrategy.AnticipationTime;
                return;
            }
        }

        _movementStrategy?.Move();
    }

    private Transform FindTarget()
    {
        return TargetSelector.GetBestTarget(transform.position, _attackRange, _toAttack);
    }

    private void EffectsTick()
    {
        for (int i = _effects.Count - 1; i >= 0; i--)
        {
            _effects[i].Tick(this, Time.deltaTime);

            if (_effects[i].IsFinished)
                _effects.RemoveAt(i);
        }
    }

    public void TakeDamage(float damage)
    {
        if (_reaction != null)
        {
            _reaction.ReactToDamage(damage);
        }

        OnTakeDamage.Invoke(damage);
    }

    public void ApplyEffect(StatusEffect effect)
    {
        _effects.Add(effect);
    }

    public bool HasEffect<EffectType>() where EffectType : StatusEffect
    {
        foreach (var effect in _effects)
        {
            if (effect is EffectType)
                return true;
        }

        return false;
    }

    public void StopMovement()
    {
        _movementStrategy.Stop();
    }
}

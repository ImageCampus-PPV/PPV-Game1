using ImageCampus.ToolBox.Services;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Attacks/Ground Bite (X)")]
public class BiteAttackStrategy : AttackStrategy
{
    [SerializeField] private float _slowDownMultiplier = 0.5f;
    [SerializeField] private float _speedReductionDuration = 0.3f;
    [SerializeField] private int _maxComboCount = 3;
    [SerializeField] private float _lastHitDamageMultiplier = 1.5f;
    [SerializeField] private float _comboWindow = 1.0f;

    private int _currentComboCount;
    private float _currentAttackTimer;
    private float _slowDownTimer;
    private float _lastAttackTime;
    private Vector2 _attackDir;

    private RuntimeDebugVisual _debugVisual = null;

    public override void Execute(Vector2 aimDir)
    {
        if (!character.IsGrounded)
            return;

        isExecuting = true;
        character.IsBlockingRotation = true;
        _attackDir = aimDir;

        if (Time.time - _lastAttackTime > _comboWindow)
        {
            _currentComboCount = 0;
        }

        _currentComboCount++;
        _lastAttackTime = Time.time;

        _currentAttackTimer = attackSpeed;
        _slowDownTimer = _speedReductionDuration;

        if (character.ActiveMovement != null)
            character.ActiveMovement.SpeedMultiplier = _slowDownMultiplier;

        float finalDamage = damage;

        if (_currentComboCount == _maxComboCount)
        {
            //Last combo hit
            finalDamage *= _lastHitDamageMultiplier;
            _currentComboCount = 0;
        }

        Vector2 attackPos = (Vector2)character.transform.position + (_attackDir * hitboxRadius);

        if (!_debugVisual)
            _debugVisual = ServiceProvider.Instance.GetService<RuntimeDebugVisual>();

        _debugVisual.DrawCircle(attackPos, hitboxRadius, Color.aliceBlue, attackSpeed);

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, hitboxRadius, enemyLayer);
        DealDamageToTargets(hits, finalDamage);
    }

    public override void Tick()
    {
        if (!isExecuting)
            return;

        _currentAttackTimer -= Time.deltaTime;

        if (_currentAttackTimer < 0f)
        {
            ResetSpeedModifier();
            character.IsBlockingRotation = false;
            isExecuting = false;
        }
    }

    private void ResetSpeedModifier()
    {
        if (character.ActiveMovement != null)
            character.ActiveMovement.SpeedMultiplier = 1f;
    }
}

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
    [SerializeField] private float _comboWindowAfterAttack = 1.0f;
    [SerializeField] private Color _combo1Color = Color.green;
    [SerializeField] private Color _combo2Color = Color.blue;
    [SerializeField] private Color _combo3Color = Color.red;

    private int _currentComboCount;
    private float _currentAttackTimer;
    private float _slowDownTimer;
    private float _lastAttackTime;
    private Vector2 _attackDir;
    private float _fullComboWindow;
    public int CurrentComboCount => _currentComboCount;

    private RuntimeDebugVisual _debugVisual = null;

    public override void Execute(Vector2 aimDir)
    {
        if (!character.IsGrounded)
            return;

        isExecuting = true;
        character.IsBlockingRotation = true;
        _attackDir = aimDir;

        _fullComboWindow = attackSpeed + _comboWindowAfterAttack;

        if (Time.time - _lastAttackTime > _fullComboWindow)
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

        Color comboColor = _currentComboCount switch
        {
            1 => _combo1Color,
            2 => _combo2Color,
            3 => _combo3Color,
            _ => _combo3Color
        };

        _debugVisual.DrawCircle(attackPos, hitboxRadius, comboColor, attackSpeed);

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, hitboxRadius, enemyLayer);
        DealDamageToTargets(hits, finalDamage);
    }

    public override void Tick()
    {
        if (!isExecuting)
            return;

        _currentAttackTimer -= Time.deltaTime;

        if (_slowDownTimer > 0f)
        {
            _slowDownTimer -= Time.deltaTime;

            if (_slowDownTimer <= 0f)
                ResetSpeedModifier();
        }

        if (_currentAttackTimer < 0f)
        {
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

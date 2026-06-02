using ImageCampus.ToolBox.Services;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Attacks/Ground Bite (X)")]
public class BiteAttackStrategy : AttackStrategy
{
    [SerializeField] private float _slowDownMultiplier = 0.5f;
    [SerializeField] private int _maxComboCount = 3;

    private int _currentComboCount;
    private float _currentAttackTimer;
    private Vector2 _attackDir;

    private RuntimeDebugVisual _debugVisual = null;

    public override void Execute(Vector2 aimDir)
    {
        if (!character.IsGrounded)
            return;

        isExecuting = true;

        _currentComboCount++;

        if (_currentComboCount > _maxComboCount)
            _currentComboCount = 1;

        _currentAttackTimer = attackSpeed;

        if (character.ActiveMovement != null)
            character.ActiveMovement.SpeedMultiplier = _slowDownMultiplier;

        Vector2 attackPos = (Vector2)character.transform.position + (_attackDir * hitboxRadius);

        if (!_debugVisual)
            _debugVisual = ServiceProvider.Instance.GetService<RuntimeDebugVisual>();

        _debugVisual.DrawCircle(attackPos, hitboxRadius, Color.red, attackSpeed);

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, hitboxRadius, enemyLayer);
        DealDamageToTargets(hits, damage);
    }

    public override void Tick()
    {
        if (!isExecuting)
            return;

        _currentAttackTimer -= Time.deltaTime;

        if (_currentAttackTimer < 0f)
        {
            ResetSpeedModifier();
            isExecuting = false;
        }
    }

    private void ResetSpeedModifier()
    {
        if (character.ActiveMovement != null)
            character.ActiveMovement.SpeedMultiplier = 1f;
    }
}

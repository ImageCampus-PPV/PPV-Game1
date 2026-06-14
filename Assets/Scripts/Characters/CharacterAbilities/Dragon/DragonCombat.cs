using ImageCampus.ToolBox.Services;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.InputSystem;

public interface ICombat
{
    List<AttackStrategy> Strategies { get; }
}

[CreateAssetMenu(menuName = "Abilities/DragonCombat")]
public class DragonCombat : CharacterAbility, ICombat
{
    [Header("Debug")]
    [SerializeField] private bool _showAim = true;
    [SerializeField] private float _aimLineLength = 1.5f;

    [SerializeField] private AttackStrategy _groundXAttack;
    [SerializeField] private AttackStrategy _groundYAttack;
    [SerializeField] private AttackStrategy _airXAttack;
    [SerializeField] private AttackStrategy _airYAttack;
    [SerializeField] private AttackStrategy _diveAttack;

    private AttackStrategy _currentAttack;
    private Vector2 _currentMovementInput;

    private RuntimeDebugVisual _debugVisual;
    private LineRenderer _debugLine;
    private List<AttackStrategy> _strategies = new();

    public List<AttackStrategy> Strategies => _strategies;

    public override void Initialize(Character character, Rigidbody2D rb)
    {
        base.Initialize(character, rb);

        _groundXAttack = Instantiate(_groundXAttack);
        _groundYAttack = Instantiate(_groundYAttack);
        _airXAttack = Instantiate(_airXAttack);
        _airYAttack = Instantiate(_airYAttack);
        _diveAttack = Instantiate(_diveAttack);

        _groundXAttack.Initialize(character);
        _groundYAttack.Initialize(character);
        _airXAttack.Initialize(character);
        _airYAttack.Initialize(character);
        _diveAttack.Initialize(character);

        _strategies.Clear();

        _strategies.Add(_groundXAttack);
        _strategies.Add(_groundYAttack);
        _strategies.Add(_airXAttack);
        _strategies.Add(_airYAttack);
        _strategies.Add(_diveAttack);
    }

    public override void ProcessMove(Vector2 input)
    {
        _currentMovementInput = input;
    }

    public override void ProcessAction(InputAction.CallbackContext context)
    {
        if (_currentAttack != null && _currentAttack.IsExecuting)
        {
            if (context.canceled)
            {
                if (_currentAttack is ICancelOnRelease)
                    _currentAttack.Cancel();
            }
            return;
        }

        if (context.started)
            DetermineBasicAttack();
    }

    public override void ProcessSkill(InputAction.CallbackContext context)
    {
        if (context.started && _currentAttack == null && !Character.IsGrounded)
        {
            _currentAttack = _diveAttack;
            _currentAttack?.Execute(Vector2.down);
        }
    }

    private void DetermineBasicAttack()
    {
        Vector2 aim = Character.CurrentAimDir;

        bool isVertical = Mathf.Abs(aim.y) > Mathf.Abs(aim.x);

        _currentAttack = Character.IsGrounded ?
                        isVertical ? _groundYAttack : _groundXAttack :
                        isVertical ? _airYAttack : _airXAttack;

        _currentAttack?.Execute(aim);
    }

    public override void Tick()
    {
        _currentAttack?.Tick();

        if (_currentAttack != null && !_currentAttack.IsExecuting)
            _currentAttack = null;

        if (_showAim && Character != null)
        {
            if (!_debugVisual)
                _debugVisual = ServiceProvider.Instance.GetService<RuntimeDebugVisual>();

            _debugVisual.DrawRay(
                Character.transform.position,
                Character.CurrentAimDir,
                _aimLineLength,
                Color.cyan,
                Time.deltaTime
                );
        }
    }

    public override void FixedTick()
    {
        _currentAttack?.FixedTick();
    }
}

using ImageCampus.ToolBox.Services;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Abilities/DragonCombat")]
public class DragonCombat : CharacterAbility
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
    private Vector2 _currentAimDir;

    private RuntimeDebugVisual _debugVisual;
    private LineRenderer _debugLine;

    public override void Initialize(Character character, Rigidbody2D rb)
    {
        base.Initialize(character, rb);
        _groundXAttack.Initialize(character);
        _groundYAttack.Initialize(character);
        _airXAttack.Initialize(character);
        _airYAttack.Initialize(character);
        _diveAttack.Initialize(character);

        _debugVisual = ServiceProvider.Instance.GetService<RuntimeDebugVisual>();
    }

    public override void ProcessMove(Vector2 input)
    {
        _currentMovementInput = input;

        if (input != Vector2.zero)
        {
            if (Mathf.Abs(input.y) > Mathf.Abs(input.x))
                _currentAimDir = new(0f, Mathf.Sign(input.y));
            else
                _currentAimDir = new(Mathf.Sign(input.x), 0);
        }
    }

    public override void ProcessAction(InputAction.CallbackContext context)
    {
        if (_currentAttack != null && _currentAttack.IsExecuting)
        {
            if (context.canceled)
                _currentAttack.Cancel();
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
        bool isVertical = Mathf.Abs(_currentMovementInput.y) > Mathf.Abs(_currentMovementInput.x);

        _currentAttack = Character.IsGrounded ? 
                        isVertical ? _groundYAttack : _groundXAttack : 
                        isVertical ? _airYAttack : _airXAttack;

        _currentAttack?.Execute(_currentAimDir);
    }

    public override void Tick()
    {
        _currentAttack?.Tick();

        if (_currentAttack != null && !_currentAttack.IsExecuting)
            _currentAttack = null;

        if (_showAim && Character != null)
        {
            _debugVisual.DrawRay(
                Character.transform.position,
                _currentAimDir,
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

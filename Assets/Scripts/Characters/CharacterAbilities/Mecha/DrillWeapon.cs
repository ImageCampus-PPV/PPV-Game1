using ImageCampus.ToolBox.Services;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Abilities/Weapons/Drill")]
public class DrillWeapon : WeaponStrategy
{
    [Header("Drill Config")]
    [SerializeField] private float _maxDuration = 5f;
    [SerializeField] private float _damageTickRate = 0.1f;
    [SerializeField] private float _movementSlowMultiplier = 0.5f;

    private bool _isActive;
    private float _activeTimer;
    private float _tickTimer;
    private Vector2 _aimDir;

    private RuntimeDebugVisual _debugVisual;

    public bool IsActive => _isActive;

    public override void OnPressed(InputAction.CallbackContext context, Vector2 aimDir)
    {
        if (context.started)
        {
            _isActive = true;
            _activeTimer = 0f;
            _tickTimer = 0f;
            _aimDir = GetFrontDirection();

            if (character.ActiveMovement != null)
                character.ActiveMovement.SpeedMultiplier = _movementSlowMultiplier;

            character.IsBlockingJump = true;
        }
    }

    public override void OnReleased(InputAction.CallbackContext context, Vector2 aimDir)
    {
        Cancel();
    }

    public override void Cancel()
    {
        if (!_isActive) return;

        _isActive = false;

        if (character.ActiveMovement != null)
            character.ActiveMovement.SpeedMultiplier = 1f;

        character.IsBlockingJump = false;
    }

    public override void Tick()
    {
        if (!_isActive) 
            return;

        _activeTimer += Time.deltaTime;
        _tickTimer += Time.deltaTime;

        if (_activeTimer >= _maxDuration)
        {
            Cancel();
            return;
        }

        if (_tickTimer >= _damageTickRate)
        {
            _tickTimer = 0f;
            ApplyDamage();
        }

        DrawDebug();
    }

    private void ApplyDamage()
    {
        Vector2 attackPos = (Vector2)character.transform.position + (_aimDir * range);
        DealDamageInArea(attackPos, range * 0.5f);
    }

    private void DrawDebug()
    {
        if (!_debugVisual && ServiceProvider.Instance.ContainsService<RuntimeDebugVisual>())
            _debugVisual = ServiceProvider.Instance.GetService<RuntimeDebugVisual>();

        Vector2 attackPos = (Vector2)character.transform.position + (_aimDir * range);
        _debugVisual?.DrawCircle(attackPos, range * 0.5f, Color.cyan, _damageTickRate);
    }

    private Vector2 GetFrontDirection()
    {
        float sign = Mathf.Sign(character.transform.localScale.x);
        return new Vector2(sign, 0f);
    }
}

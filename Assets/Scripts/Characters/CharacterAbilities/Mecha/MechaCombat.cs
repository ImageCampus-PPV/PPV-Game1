using UnityEngine;
using UnityEngine.InputSystem;
using ImageCampus.ToolBox.Services;


[CreateAssetMenu(menuName = "Abilities/MechaCombat")]
public class MechaCombat : CharacterAbility
{
    [Header("Weapon Slots")]
    [SerializeField] private WeaponStrategy _slot1Weapon;
    [SerializeField] private WeaponStrategy _slot2Weapon;

    [Header("Aim Debug")]
    [SerializeField] private bool _showAim = true;
    [SerializeField] private float _aimLineLength = 1.5f;

    private Vector2 _aimDir = Vector2.right;
    private Vector2 _stickAimDir = Vector2.zero;
    private RuntimeDebugVisual _debugVisual;

    public WeaponStrategy Slot1Weapon => _slot1Weapon;
    public WeaponStrategy Slot2Weapon => _slot2Weapon;

    public override void Initialize(Character character, Rigidbody2D rb)
    {
        base.Initialize(character, rb);

        _slot1Weapon?.Initialize(character);
        _slot2Weapon?.Initialize(character);
    }

    public override void ProcessAction(InputAction.CallbackContext context)
    {
        if (_slot1Weapon == null) 
            return;

        Vector2 aim = GetAimDirection();

        if (context.started || context.performed)
            _slot1Weapon.OnPressed(context, aim);
        else if (context.canceled)
            _slot1Weapon.OnReleased(context, aim);
    }

    public override void ProcessSkill(InputAction.CallbackContext context)
    {
        if (_slot2Weapon == null) 
            return;

        Vector2 aim = GetAimDirection();

        if (context.started || context.performed)
            _slot2Weapon.OnPressed(context, aim);
        else if (context.canceled)
            _slot2Weapon.OnReleased(context, aim);
    }

    public override void ProcessAim(Vector2 input)
    {
        if (input.sqrMagnitude > 0.1f)
            _stickAimDir = input.normalized;
    }

    public override void Tick()
    {
        _aimDir = GetAimDirection();

        (_slot1Weapon as GatlingWeapon)?.UpdateAimDir(_aimDir);
        (_slot2Weapon as GatlingWeapon)?.UpdateAimDir(_aimDir);

        _slot1Weapon?.Tick();
        _slot2Weapon?.Tick();

        if (_showAim && Character != null)
        {
            if (!_debugVisual && ServiceProvider.Instance.ContainsService<RuntimeDebugVisual>())
                _debugVisual = ServiceProvider.Instance.GetService<RuntimeDebugVisual>();

            _debugVisual?.DrawRay(Character.transform.position, _aimDir, _aimLineLength, Color.yellow, Time.deltaTime);
        }
    }

    public override void FixedTick()
    {
        _slot1Weapon?.FixedTick();
        _slot2Weapon?.FixedTick();
    }

    private Vector2 GetAimDirection()
    {
        if (_stickAimDir.sqrMagnitude > 0.1f)
        {
            _aimDir = _stickAimDir;
            return _aimDir;
        }

        Mouse mouse = UnityEngine.InputSystem.Mouse.current;

        if (mouse != null && Camera.main != null)
        {
            Vector3 screenPos = mouse.position.ReadValue();
            screenPos.z = Mathf.Abs(Camera.main.transform.position.z);
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(screenPos);
            Vector2 dir = mouseWorld - (Vector2)Character.transform.position;

            if (dir.sqrMagnitude > 0.01f)
            {
                _aimDir = dir.normalized;
                return _aimDir;
            }
        }

        return _aimDir;
    }
}

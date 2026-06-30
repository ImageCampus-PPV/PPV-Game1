using UnityEngine;
using UnityEngine.InputSystem;
using ImageCampus.ToolBox.Services;


[CreateAssetMenu(menuName = "Abilities/MechaCombat")]
public class MechaCombat : CharacterAbility
{
    [Header("Weapon Slots (arma actualmente equipada)")]
    [SerializeField] private WeaponStrategy _slot1Weapon;
    [SerializeField] private WeaponStrategy _slot2Weapon;

    [Header("Armas disponibles para ciclar")]
    [SerializeField] private WeaponStrategy[] _meleeWeapons;
    [SerializeField] private WeaponStrategy[] _rangedWeapons;

    [Header("Aim Debug")]
    [SerializeField] private bool _showAim = true;
    [SerializeField] private float _aimLineLength = 1.5f;

    private ShieldAbility _shieldAbility;
    private Vector2 _aimDir = Vector2.right;
    private Vector2 _stickAimDir = Vector2.zero;
    private RuntimeDebugVisual _debugVisual;

    private int _meleeIndex;
    private int _rangedIndex;

    public WeaponStrategy Slot1Weapon => _slot1Weapon;
    public WeaponStrategy Slot2Weapon => _slot2Weapon;

    public override void Initialize(Character character, Rigidbody2D rb)
    {
        base.Initialize(character, rb);
        _slot1Weapon?.Initialize(character);
        _slot2Weapon?.Initialize(character);

        if (_slot1Weapon != null)
            _meleeIndex = System.Array.IndexOf(_meleeWeapons, _slot1Weapon);
        if (_slot2Weapon != null)
            _rangedIndex = System.Array.IndexOf(_rangedWeapons, _slot2Weapon);
    }

    public void OnCycleSlot1(InputAction.CallbackContext context)
    {
        if (!context.performed || _meleeWeapons == null || _meleeWeapons.Length == 0) return;

        _meleeIndex = (_meleeIndex + 1) % _meleeWeapons.Length;
        EquipSlot1(_meleeWeapons[_meleeIndex]);
    }

    public void OnCycleSlot2(InputAction.CallbackContext context)
    {
        if (!context.performed || _rangedWeapons == null || _rangedWeapons.Length == 0) return;

        _rangedIndex = (_rangedIndex + 1) % _rangedWeapons.Length;
        EquipSlot2(_rangedWeapons[_rangedIndex]);
    }

    private void EquipSlot1(WeaponStrategy weapon)
    {
        if (weapon == null) return;

        _slot1Weapon?.Cancel();
        _slot1Weapon = weapon;
        _slot1Weapon.Initialize(Character);

        Debug.Log($"[MechaCombat] Slot 1 → {weapon.name}");
    }

    private void EquipSlot2(WeaponStrategy weapon)
    {
        if (weapon == null) return;

        _slot2Weapon?.Cancel();
        _slot2Weapon = weapon;
        _slot2Weapon.Initialize(Character);

        Debug.Log($"[MechaCombat] Slot 2 → {weapon.name}");
    }

    public override void ProcessAction(InputAction.CallbackContext context)
    {
        if (_slot1Weapon == null) return;

        Vector2 aim = Character.CurrentAimDir;
        if (context.started || context.performed)
            _slot1Weapon.OnPressed(context, aim);
        else if (context.canceled)
            _slot1Weapon.OnReleased(context, aim);
    }

    public override void ProcessSkill(InputAction.CallbackContext context)
    {
        if (_slot2Weapon == null) return;

        Vector2 aim = Character.CurrentAimDir;
        if (context.started || context.performed)
            _slot2Weapon.OnPressed(context, aim);
        else if (context.canceled)
            _slot2Weapon.OnReleased(context, aim);
    }

    public void ProcessShield(InputAction.CallbackContext context)
    {
        if (_shieldAbility == null)
        {
            foreach (var ability in Character.ActiveAbilities)
            {
                if (ability is ShieldAbility shield)
                {
                    _shieldAbility = shield;
                    break;
                }
            }
        }
        _shieldAbility?.OnShieldInput(context);
    }

    public override void ProcessAim(Vector2 input)
    {
        if (input.sqrMagnitude > 0.1f)
            _stickAimDir = input.normalized;
    }

    public override void Tick()
    {
        _aimDir = Character.CurrentAimDir;

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
}

using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using UnityEngine;
using UnityEngine.InputSystem;


[CreateAssetMenu(menuName = "Abilities/ShieldAbility")]
public class ShieldAbility : CharacterAbility
{
    [Header("Shield Config")]
    [SerializeField] private float _maxHp = 100f;
    [SerializeField] private float _minHp = 0f;
    [SerializeField] private float _lifetime = 8f;
    [SerializeField] private float _cooldown = 5f;
    [SerializeField] private float _brokenCooldownMultiplier = 2f;
    [SerializeField] private float _domeRadius = 1.5f;
    [SerializeField] private float _movementSlowMultiplier = 0.4f;

    [Header("Prefab")]
    [SerializeField] private ShieldDome _shieldPrefab;

    private ShieldDome _activeDome;
    private bool _isOnCooldown;
    private float _lifetimeTimer;
    private float _cooldownTimer;
    private float _currentCooldown;

    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
    public bool IsActive => _activeDome != null && _activeDome.gameObject.activeSelf;
    public bool IsOnCooldown => _isOnCooldown;
    public float CooldownProgress => _isOnCooldown && _currentCooldown > 0f ? 1f - (_cooldownTimer / _currentCooldown) : 1f;
    public float ShieldHpProgress => IsActive && _lifetime > 0f ? _lifetimeTimer / _lifetime : 1f;

    public float ShieldArmorProgress => _activeDome != null ? _activeDome.HpPercent : 1f;

    public override void Initialize(Character character, Rigidbody2D rb)
    {
        base.Initialize(character, rb);
        _activeDome = Instantiate(_shieldPrefab, Character.transform.position, Quaternion.identity);
        _activeDome.gameObject.SetActive(false);
        _activeDome.Initialize(_maxHp, _minHp, _domeRadius);
        EventBus.Subscribe<OnShieldBroken>(BreakShield);
    }

    public void OnShieldInput(InputAction.CallbackContext context)
    {
        if (context.started)
            TryActivateShield();
        else if (context.canceled)
            DeactivateShield(broken: false);
    }

    public override void Tick()
    {
        HandleLifetime();
        HandleCooldown();
        UpdateDomePosition();
    }

    private void HandleLifetime()
    {
        if (!IsActive)
            return;

        _lifetimeTimer -= Time.deltaTime;
        if (_lifetimeTimer <= 0f)
            DeactivateShield(broken: false);
    }

    private void HandleCooldown()
    {
        if (!_isOnCooldown)
            return;

        _cooldownTimer -= Time.deltaTime;

        if (_cooldownTimer <= 0f)
            _isOnCooldown = false;
    }

    private void UpdateDomePosition()
    {
        if (IsActive)
            _activeDome.transform.position = Character.transform.position;
    }

    private void TryActivateShield()
    {
        if (IsActive || _isOnCooldown)
            return;

        if (_shieldPrefab == null)
        {
            Debug.LogWarning("[ShieldAbility] Shield prefab not assigned.");
            return;
        }

        _activeDome.transform.position = Character.transform.position;


        _activeDome.gameObject.SetActive(true);
        _lifetimeTimer = _lifetime;

        if (Character.ActiveMovement != null)
            Character.ActiveMovement.SpeedMultiplier = _movementSlowMultiplier;

        Debug.Log("[ShieldAbility] Shield activated.");
    }

    private void BreakShield(in OnShieldBroken onShieldBroken)
    {
        if (_activeDome.ID != onShieldBroken.shieldID)
            return;

        DeactivateShield(true);
    }

    private void DeactivateShield(bool broken)
    {
        if (!IsActive)
            return;

        _activeDome.gameObject.SetActive(false);

        if (Character.ActiveMovement != null)
            Character.ActiveMovement.SpeedMultiplier = 1f;

        _activeDome?.Restore();

        if (_activeDome != null)
        {
            Object.Destroy(_activeDome.gameObject);
            _activeDome = null;
        }

        float lifetimeUsed = _lifetime > 0f ? 1f - (_lifetimeTimer / _lifetime) : 1f;
        _currentCooldown = broken ? _cooldown * _brokenCooldownMultiplier : _cooldown;
        _cooldownTimer = _currentCooldown * lifetimeUsed;
        _isOnCooldown = true;

        Debug.Log($"[ShieldAbility] Deactivated. Broken: {broken}. Cooldown: {_currentCooldown}s");
    }

    public void Cancel()
    {
        if (IsActive)
            DeactivateShield(broken: false);
    }

    public override void Dispose()
    {
        base.Dispose();
        EventBus.Unsubscribe<OnShieldBroken>(BreakShield);
    }
}

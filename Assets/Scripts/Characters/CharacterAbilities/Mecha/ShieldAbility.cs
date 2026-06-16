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
    private bool _isActive;
    private bool _isOnCooldown;
    private float _lifetimeTimer;
    private float _cooldownTimer;
    private float _currentCooldown;

    public bool IsActive => _isActive;
    public bool IsOnCooldown => _isOnCooldown;
    public float CooldownProgress => _isOnCooldown && _currentCooldown > 0f ? 1f - (_cooldownTimer / _currentCooldown) : 1f;


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
        if (!_isActive) return;

        _lifetimeTimer -= Time.deltaTime;
        if (_lifetimeTimer <= 0f)
            DeactivateShield(broken: false);
    }

    private void HandleCooldown()
    {
        if (!_isOnCooldown) return;

        _cooldownTimer -= Time.deltaTime;
        if (_cooldownTimer <= 0f)
            _isOnCooldown = false;
    }

    private void UpdateDomePosition()
    {
        if (_isActive && _activeDome != null)
            _activeDome.transform.position = Character.transform.position;
    }

    private void TryActivateShield()
    {
        if (_isActive || _isOnCooldown) return;

        if (_shieldPrefab == null)
        {
            Debug.LogWarning("[ShieldAbility] Shield prefab not assigned.");
            return;
        }

        _activeDome = Object.Instantiate(_shieldPrefab, Character.transform.position, Quaternion.identity);

        Character[] allCharacters = Object.FindObjectsByType<Character>(FindObjectsSortMode.None);
        Collider2D[] friendlyColliders = new Collider2D[allCharacters.Length];

        for (int i = 0; i < allCharacters.Length; i++)
            friendlyColliders[i] = allCharacters[i].GetComponent<Collider2D>();

        _activeDome.Initialize(_maxHp, _minHp, _domeRadius, friendlyColliders);
        _activeDome.OnShieldBroken += () => DeactivateShield(broken: true);

        _isActive = true;
        _lifetimeTimer = _lifetime;

        if (Character.ActiveMovement != null)
            Character.ActiveMovement.SpeedMultiplier = _movementSlowMultiplier;

        Debug.Log("[ShieldAbility] Shield activated.");
    }

    private void DeactivateShield(bool broken)
    {
        if (!_isActive) 
            return;

        _isActive = false;

        if (Character.ActiveMovement != null)
            Character.ActiveMovement.SpeedMultiplier = 1f;

        _activeDome?.Restore();

        if (_activeDome != null)
        {
            Object.Destroy(_activeDome.gameObject);
            _activeDome = null;
        }

        _currentCooldown = broken ? _cooldown * _brokenCooldownMultiplier : _cooldown;
        _cooldownTimer = _currentCooldown;
        _isOnCooldown = true;

        Debug.Log($"[ShieldAbility] Deactivated. Broken: {broken}. Cooldown: {_currentCooldown}s");
    }

    public void Cancel()
    {
        if (_isActive)
            DeactivateShield(broken: false);
    }
}

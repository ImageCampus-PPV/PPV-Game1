using ImageCampus.ToolBox.Services;
using UnityEngine;
using UnityEngine.InputSystem;


[CreateAssetMenu(menuName = "Abilities/Weapons/CircularSaw")]
public class CircularSawWeapon : WeaponStrategy
{
    [Header("Circular Saw Config")]
    [SerializeField] private int _maxComboCount = 3;
    [SerializeField] private float _comboWindowTime = 0.6f;

    private int _currentCombo;
    private float _comboTimer;
    private bool _isExecuting;

    private RuntimeDebugVisual _debugVisual;

    public int CurrentCombo => _currentCombo;
    public bool IsExecuting => _isExecuting;

    public override void OnPressed(InputAction.CallbackContext context, Vector2 aimDir)
    {
        if (!context.started) 
            return;

        if (IsOnCooldown) 
            return;

        if (_currentCombo >= _maxComboCount || _comboTimer <= 0f)
            _currentCombo = 0;

        _currentCombo++;
        _comboTimer = _comboWindowTime;
        _isExecuting = true;
        lastFireTime = Time.time;

        ExecuteHit(aimDir);
    }

    public override void Tick()
    {
        if (_comboTimer > 0f)
        {
            _comboTimer -= Time.deltaTime;

            if (_comboTimer <= 0f)
            {
                _currentCombo = 0;
                _isExecuting = false;
            }
        }
    }

    public override void Cancel()
    {
        _isExecuting = false;
        _currentCombo = 0;
        _comboTimer = 0f;
    }

    private void ExecuteHit(Vector2 aimDir)
    {
        Vector2 front = GetFrontDirection();
        Vector2 attackPos = (Vector2)character.transform.position + (front * range);

        DealDamageInArea(attackPos, range * 0.5f);

        if (!_debugVisual && ServiceProvider.Instance.ContainsService<RuntimeDebugVisual>())
            _debugVisual = ServiceProvider.Instance.GetService<RuntimeDebugVisual>();

        Color debugColor = _currentCombo == 1 ? Color.yellow :
                           _currentCombo == 2 ? Color.green : Color.red;

        _debugVisual?.DrawCircle(attackPos, range * 0.5f, debugColor, _comboWindowTime);
    }

    private Vector2 GetFrontDirection()
    {
        float sign = Mathf.Sign(character.transform.localScale.x);

        return new Vector2(sign, 0f);
    }
}

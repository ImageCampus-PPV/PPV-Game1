using ImageCampus.ToolBox.Services;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Skills/Dive Skill (Y)")]
public class DiveAttackStrategy : AttackStrategy
{
    [SerializeField] private float _maxDamage = 100f;
    [SerializeField] private float _maxAoeRadius = 5f;
    [SerializeField] private float _maxVulnerabilityTime = 3f;
    [SerializeField] private float _distanceToMaxDamage = 10f;
    [SerializeField] private float _fallSpeedMultiplier = 1.2f;

    private RuntimeDebugVisual _debugVisual;

    private float _startYPos;
    private bool _isFalling;
    private bool _isWeakened;
    private float _vulnerabilityTimer;
    private float _calculatedAoe;

    public override void Execute(Vector2 aimDir)
    {
        if (character.IsGrounded)
            return;

        isExecuting = true;
        _isFalling = true;
        _isWeakened = false;
        _startYPos = character.transform.position.y;

        character.IsIgnoringInput = true;
        character.Rb.gravityScale = _fallSpeedMultiplier;
    }

    public override void Tick()
    {
        if (!isExecuting)
            return;

        if (_isFalling)
        {
            character.ApplyHVelocity(0);
            if (character.IsGrounded)
                Land();
        }
        else if (_isWeakened)
        {
            _vulnerabilityTimer -= Time.deltaTime;

            if (_vulnerabilityTimer < 0f)
                Recover();
        }
    }

    private void Land()
    {
        _isFalling = false;
        _isWeakened = true;
        character.ApplyHVelocity(0f);
        character.Rb.gravityScale = 1f;

        float distanceFell = _startYPos - character.transform.position.y;
        float scaleFactor = Mathf.Clamp01(distanceFell / _distanceToMaxDamage);

        float calculatedDamage = Mathf.Lerp(damage, _maxDamage, scaleFactor);
        _calculatedAoe = Mathf.Lerp(1f, _maxAoeRadius, scaleFactor);
        _vulnerabilityTimer = Mathf.Lerp(1f, _maxVulnerabilityTime, scaleFactor);

        if (!_debugVisual)
            _debugVisual = ServiceProvider.Instance.GetService<RuntimeDebugVisual>();

        _debugVisual.DrawCircle(character.transform.position, _calculatedAoe, Color.purple, 1.5f, 0.15f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(character.transform.position, _calculatedAoe, enemyLayer);
        DealDamageToTargets(hits, calculatedDamage);
    }

    private void Recover()
    {
        _isWeakened = false;
        character.IsIgnoringInput = false;
        isExecuting = false;
        _calculatedAoe = 0f;
    }
}
using ImageCampus.ToolBox.Services;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Attacks/Air Embers (Y)")]
public class EmbersAttackStrategy : AttackStrategy
{
    [SerializeField] private float _beamRange = 5f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _burnDamagePerSecond = 3f;
    [SerializeField] private float _burnDuration = 3f;
    [SerializeField] private float _fireSpreadLength = 5f;
    [SerializeField] private float _fireSpreadHeight = 1f;
    [SerializeField] private float _beamLineThickness = 0.05f;
    [SerializeField] private float _beamWidth = 0.5f;
    private Vector2 _currentAim;

    private RuntimeDebugVisual _debugVisual;

    public override void Execute(Vector2 aimDir)
    {
        if (character.IsGrounded)
            return;

        isExecuting = true;

        _currentAim = aimDir;

        if (!_debugVisual)
            _debugVisual = ServiceProvider.Instance.GetService<RuntimeDebugVisual>();
    }

    public override void Tick()
    {
        if (!isExecuting)
            return;

        if (character.IsGrounded)
        {
            Cancel();
            return;
        }

        _currentAim = character.CurrentAimDir;

        Vector2 start = character.transform.position;

        RaycastHit2D groundHit = Physics2D.Raycast(start, _currentAim, _beamRange, _groundLayer);

        Vector2 beamCenter = start + _currentAim * (_beamRange * 0.5f);
        Vector2 beamSize = new(_beamRange, _beamWidth);
        float beamAngle = Mathf.Atan2(_currentAim.y, _currentAim.x) * Mathf.Rad2Deg;

        if (_debugVisual)
            _debugVisual.DrawOrientedBox(beamCenter, beamSize, beamAngle, Color.red, Time.deltaTime, _beamLineThickness <= 0f ? 0.05f : _beamLineThickness);

        Collider2D[] hits = Physics2D.OverlapBoxAll(beamCenter, beamSize, beamAngle, enemyLayer | _groundLayer);

        foreach (var hit in hits)
        {
            if (((1 << hit.gameObject.layer) & enemyLayer) != 0)
            {
                IDamageable damageable = hit.GetComponent<IDamageable>();
                //mientras el ascuas toque al enemigo, le hace daño
                damageable?.TakeDamage(damage * Time.deltaTime);

                if (hit.TryGetComponent<IStatusEffectReceiver>(out var receiver))
                //y, además, le aplica el efecto de quemadura por x tiempo.
                {
                    if (!receiver.HasEffect<BurnEffect>())
                        receiver.ApplyEffect(new BurnEffect(_burnDuration, _burnDamagePerSecond));
                }
            }

            if (groundHit.collider != null)
            {
                Vector2 rangeSize = new(_fireSpreadLength, _fireSpreadHeight);

                Collider2D[] groundAoeHits = Physics2D.OverlapBoxAll(groundHit.point, rangeSize, 0f, enemyLayer);
                DealDamageToTargets(groundAoeHits, damage * Time.deltaTime);

                _debugVisual.DrawBox(groundHit.point, rangeSize, Color.orange, Time.deltaTime);

                break;
            }
        }
    }
}

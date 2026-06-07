using ImageCampus.ToolBox.Services;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Attacks/Air Embers (Y)")]
public class EmbersAttackStrategy : AttackStrategy
{
    [SerializeField] private float _range = 5f;
    [SerializeField] private float _groundRadius = 2f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _burnDamagePerSecond = 3f;
    [SerializeField] private float _burnDuration = 3f;
    [SerializeField] private float _fireSpreadLength = 5f;
    [SerializeField] private float _fireSpreadHeight = 1f;
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

        _debugVisual.DrawRay(character.transform.position, _currentAim, _range, Color.red, Time.deltaTime);

        RaycastHit2D[] hits = Physics2D.RaycastAll(character.transform.position, _currentAim, _range, enemyLayer | _groundLayer);

        foreach (var hit in hits)
        {
            if (((1 << hit.collider.gameObject.layer) & enemyLayer) != 0)
            {
                IDamageable damageable = hit.collider.GetComponent<IDamageable>();
                //mientras el ascuas toque al enemigo, le hace daño
                damageable?.TakeDamage(damage * Time.deltaTime);

                if (hit.collider.TryGetComponent<IStatusEffectReceiver>(out var receiver))
                //y, además, le aplica el efecto de quemadura por x tiempo.
                {
                    if (!receiver.HasEffect<BurnEffect>())
                        receiver.ApplyEffect(new BurnEffect(_burnDuration, _burnDamagePerSecond));
                }
            }

            if (((1 << hit.collider.gameObject.layer) & _groundLayer) != 0)
            {
                _debugVisual.DrawCircle(hit.point, _groundRadius, Color.coral, Time.deltaTime);

                Vector2 rangeSize = new(_fireSpreadLength, _fireSpreadHeight);

                Collider2D[] groundAoeHits = Physics2D.OverlapBoxAll(hit.point, rangeSize, enemyLayer);
                DealDamageToTargets(groundAoeHits, damage * Time.deltaTime);

                _debugVisual.DrawBox(hit.point, rangeSize, Color.orange, Time.deltaTime);

                break;
            }
        }
    }
}

using ImageCampus.ToolBox.Services;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Attacks/Air Embers (Y)")]
public class EmbersAttackStrategy : AttackStrategy
{
    [SerializeField] private float _range = 5f;
    [SerializeField] private float _groundRadius = 2f;
    [SerializeField] private float _continuousBurnDamage = 5f;
    [SerializeField] private LayerMask _groundLayer;
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

        _debugVisual.DrawRay(character.transform.position, _currentAim, _range, Color.blueViolet, Time.deltaTime);

        RaycastHit2D[] hits = Physics2D.RaycastAll(character.transform.position, _currentAim, _range, enemyLayer | _groundLayer);

        foreach (var hit in hits)
        {
            if (((1 << hit.collider.gameObject.layer) & enemyLayer) != 0)
            {
                IDamageable damageable = hit.collider.GetComponent<IDamageable>();
                damageable?.TakeDamage(_continuousBurnDamage * Time.deltaTime);
            }

            if (((1 << hit.collider.gameObject.layer) & _groundLayer) != 0)
            {
                _debugVisual.DrawCircle(hit.point, _groundRadius, Color.coral, Time.deltaTime);

                Collider2D[] groundAoeHits = Physics2D.OverlapCircleAll(hit.point, _groundRadius, enemyLayer);
                DealDamageToTargets(groundAoeHits, _continuousBurnDamage * Time.deltaTime);
                break;
            }
        }
    }
}

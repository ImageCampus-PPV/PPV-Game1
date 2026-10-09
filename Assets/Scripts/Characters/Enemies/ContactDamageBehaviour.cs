using UnityEngine;

public sealed class ContactDamageBehaviour : DamageBehaviour
{
    private readonly float _damage;
    private readonly float _cooldown;
    private readonly LayerMask _targetLayers;

    private float _cooldownTimer;

    public ContactDamageBehaviour(float damage, float cooldown, LayerMask targetLayers)
    {
        _damage = damage;
        _cooldown = cooldown;
        _targetLayers = targetLayers;
    }

    public override void OnCollisionStay(Collision2D collision)
    {
        if (_cooldownTimer > 0f)
            return;

        GameObject target = collision.gameObject;

        if ((_targetLayers.value & (1 << target.layer)) == 0)
            return;

        if (!target.TryGetComponent<DamageableEntity>(out DamageableEntity damageable))
            return;

        damageable.TakeDamage(_damage);

        _cooldownTimer = _cooldown;
    }

    public override void Tick(float deltaTime)
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= deltaTime;
    }
}

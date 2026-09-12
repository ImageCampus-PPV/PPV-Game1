public class BurnEffect : StatusEffect
{
    private readonly float _damagePerSecond;

    public override string DisplayName => "Burning";

    public BurnEffect(float duration, float damagePerSecond) : base(duration)
    {
        _damagePerSecond = damagePerSecond;
    }

    public override void Tick(IStatusEffectReceiver target, float dt)
    {
        remainingTime -= dt;

        if (target is IDamageable damageable)
            damageable.TakeDamage(_damagePerSecond * dt);
    }
}
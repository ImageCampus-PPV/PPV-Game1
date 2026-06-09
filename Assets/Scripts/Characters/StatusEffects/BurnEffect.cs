public class BurnEffect : StatusEffect
{
    private float _damagePerSecond;

    public override string DisplayName => "Burning!";

    public BurnEffect(float duration, float damagePerSecond)
    {
        remainingTime = duration;
        _damagePerSecond = damagePerSecond;
    }

    public override void Tick(IDamageable target, float dt)
    {
        remainingTime -= dt;

        target.TakeDamage(_damagePerSecond * dt);
    }
}


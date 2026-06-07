public class BurnEffect : StatusEffect
{
    private float _duration;
    private float _damagePerSecond;

    public override bool IsFinished => _duration <= 0f;

    public BurnEffect(float duration, float damagePerSecond)
    {
        _duration = duration;
        _damagePerSecond = damagePerSecond;
    }

    public override void Tick(IDamageable target, float dt)
    {
        _duration -= dt;

        target.TakeDamage(_damagePerSecond * dt);
    }
}


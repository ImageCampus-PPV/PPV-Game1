public abstract class StatusEffect
{
    protected float remainingTime;

    public float RemainingTime => remainingTime;
    public bool IsFinished => remainingTime <= 0f;
    public virtual string DisplayName => GetType().Name;

    public abstract void Tick(IDamageable target, float dt);
}
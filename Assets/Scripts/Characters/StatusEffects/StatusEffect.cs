public abstract class StatusEffect
{
    public abstract void Tick(IDamageable target, float dt);
    public abstract bool IsFinished { get; }
}
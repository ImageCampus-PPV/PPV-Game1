public class StunStatusEffect : StatusEffect
{
    public override string DisplayName => "Stunned!";

    public StunStatusEffect(float duration)
    {
        remainingTime = duration;
    }

    public override void Tick(IStatusEffectReceiver target, float dt)
    {
        remainingTime -= dt;

        if (target is IStunnable stunnable)
        {
            stunnable.StopMovement();
        }
    }
}

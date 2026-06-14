public interface IStatusEffectReceiver
{
    bool HasEffect<EffectType>() where EffectType : StatusEffect;
    void ApplyEffect(StatusEffect effect);
}


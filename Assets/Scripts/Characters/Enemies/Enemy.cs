using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable, IStatusEffectReceiver
{
    private DamageResponse _reaction;
    private readonly List<StatusEffect> _effects = new();

    private void Awake()
    {
        _reaction = GetComponent<DamageResponse>();

        if (_reaction == null)
            Debug.LogWarning($"The enemy {gameObject.name} does not have a reaction to damage assigned.");
    }

    public void TakeDamage(float damage)
    {
        if (_reaction != null)
        {
            _reaction.ReactToDamage(damage);
        }
    }

    private void Update()
    {
        for (int i = _effects.Count; i > 0; i--)
        {
            _effects[i].Tick(this, Time.deltaTime);

            if (_effects[i].IsFinished)
                _effects.RemoveAt(i);
        }
    }

    public void ApplyEffect(StatusEffect effect)
    {
        _effects.Add(effect);
    }

    public bool HasEffect<EffectType>() where EffectType : StatusEffect
    {
        return _effects.Any(effect => effect is EffectType);
    }
}

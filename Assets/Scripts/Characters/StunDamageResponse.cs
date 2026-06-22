using UnityEngine;

public class StunDamageResponse : DamageResponse
{
    [SerializeField] private float _stunDuration = 1.5f;
    private IStatusEffectReceiver _receiver;
    private void Awake()
    {
        _receiver = GetComponent<IStatusEffectReceiver>();
    }

    public override void ReactToDamage(float damage)
    {
        if (_receiver != null)
            _receiver.ApplyEffect(new StunStatusEffect(_stunDuration));
    }
}
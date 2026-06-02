using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    private DamageResponse _reaction;

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
}

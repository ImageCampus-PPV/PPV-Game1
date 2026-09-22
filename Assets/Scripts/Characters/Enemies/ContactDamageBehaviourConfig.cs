using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Behaviour/" + nameof(ContactDamageBehaviourConfig))]
public sealed class ContactDamageBehaviourConfig : DamageBehaviourConfig
{
    [SerializeField] private float _damage = 1f;
    [SerializeField] private float _cooldown = 1f;
    [SerializeField] private LayerMask _targetLayers;

    public override DamageBehaviour CreateBehaviour()
    {
        return new ContactDamageBehaviour(_damage, _cooldown, _targetLayers);
    }
}

using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using UnityEngine;

[RequireComponent(typeof(Health), typeof(Character))]
public class DownStateTracker : BaseEntity
{
    [SerializeField] private float _downedSpeedMultiplier = 0.3f;

    private Health _health;
    private Character _character;
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    private void Awake()
    {
        _character = GetComponent<Character>();
        _health = GetComponent<Health>();
    }

    private void Start()
    {
        EventBus.Subscribe<OnCharacterDowned>(HandleDowned);
        EventBus.Subscribe<OnCharacterRevived>(HandleRevived);
    }

    private void HandleRevived(in OnCharacterRevived onCharacterRevived)
    {
        if (_health.OwnerID != onCharacterRevived.entityRevivedID)
            return;

        _character.IsBlockingAbilities = false;
        _character.ActiveMovement.SpeedMultiplier = 1f;
        Debug.Log("Revived.");
    }

    private void HandleDowned(in OnCharacterDowned onCharacterDowned)
    {
        if (_health.OwnerID != onCharacterDowned.entityDownedID)
            return;

        _character.IsBlockingAbilities = true;
        _character.ActiveMovement.SpeedMultiplier = _downedSpeedMultiplier;
        Debug.Log("Downed. Abilities blocked. Movement multiplier: " + _downedSpeedMultiplier);
    }
}
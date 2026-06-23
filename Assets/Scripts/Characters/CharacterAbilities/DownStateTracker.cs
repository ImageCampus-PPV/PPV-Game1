using UnityEngine;

[RequireComponent(typeof(Health), typeof(Character))]
public class DownStateTracker : MonoBehaviour
{
    [SerializeField] private float _downedSpeedMultiplier = 0.3f;

    private Health _health;
    private Character _character;

    private void Awake()
    {
        _character = GetComponent<Character>();
        _health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        _health.OnDowned += HandleDowned;
        _health.OnRevived += HandleRevived;
    }

    private void OnDisable()
    {
        _health.OnDowned -= HandleDowned;
        _health.OnRevived -= HandleRevived;
    }

    private void HandleRevived()
    {
        _character.IsBlockingAbilities = false;
        _character.ActiveMovement.SpeedMultiplier = 1f;
        Debug.Log("Revived.");
    }

    private void HandleDowned()
    {
        _character.IsBlockingAbilities = true;
        _character.ActiveMovement.SpeedMultiplier = _downedSpeedMultiplier;
        Debug.Log("Downed.");
    }
}
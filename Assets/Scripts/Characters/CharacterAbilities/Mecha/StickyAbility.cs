using GreenAbyss.Events;
using GreenAbyss.Services;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Sticky")]
public class StickyAbility : CharacterAbility
{
    [SerializeField] private float _stickDuration = 1.5f;
    [SerializeField] private float _jumpOffForceX = 10f;
    [SerializeField] private float _unstickForceXMultiplier = 0.5f;
    [SerializeField] private float _jumpOffForceY = 15f;
    [SerializeField] private float _slowFallGravityScale = 0.3f;

    private float _stickTimer;
    private Vector2 _wallNormal;
    private Vector2 _attachPoint;

    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
    public bool IsSticking { get; private set; }

    public override void Initialize(Character character, Rigidbody2D rb)
    {
        base.Initialize(character, rb);

        EventBus.Subscribe<OnCharacterJumpPressed>(JumpOff);
    }

    public override void CharacterCollisionStay(Collision2D collision)
    {
        if (IsSticking || Character.IsGrounded)
            return;

        if (collision.gameObject.TryGetComponent<StickyWall>(out StickyWall stickyWall))
        {
            Vector3 normal = collision.contacts[0].normal;
            normal.y = 0;
            normal.z = 0;
            Stick(collision.contacts[0].point, collision.contacts[0].normal);
        }
    }

    public void Stick(Vector2 contactPoint, Vector2 wallNormal)
    {
        if (IsSticking || wallNormal.Equals(Vector3.zero))
            return;

        _wallNormal = wallNormal.normalized;

        _attachPoint = contactPoint + wallNormal * (Character.transform.lossyScale.x * 0.5f);

        IsSticking = true;
        _stickTimer = _stickDuration;

        Rb.linearVelocity = Vector2.zero;

        Rb.gravityScale = _slowFallGravityScale;

        Character.transform.position = _attachPoint;
        Character.ForceResetJumpCount();
    }

    public override void FixedTick()
    {
        if (!IsSticking)
            return;

        _stickTimer -= Time.fixedDeltaTime;

        if (_stickTimer <= 0f)
        {
            Unstick();
            return;
        }

        Character.transform.position = _attachPoint;
    }

    public void JumpOff(in OnCharacterJumpPressed onCharacterJumpPressed)
    {
        if (Character.ID != onCharacterJumpPressed.characterID)
            return;

        if (!IsSticking)
            return;

        Unstick();

        Vector2 force = _wallNormal * _jumpOffForceX + Vector2.up * _jumpOffForceY;

        Rb.linearVelocity = Vector2.zero;

        Rb.AddForce(force, ForceMode2D.Impulse);
    }

    private void Unstick()
    {
        if (!IsSticking)
            return;

        Character.transform.position += (Vector3)(_wallNormal * 0.1f);

        IsSticking = false;
        Rb.gravityScale = 1f;
    }
}

using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Sticky")]
public class StickyAbility : CharacterAbility
{
    [SerializeField] private float _stickDuration = 1.5f;
    [SerializeField] private Vector2 _jumpOffForce = new Vector2(5f, 15f);
    
    private float _stickTimer;
    private Vector2 _wallNormal;
    private Vector2 _attachPoint;

    public bool IsSticking { get; private set; }

    public override void Initialize(Character character, Rigidbody2D rb)
    {
        base.Initialize(character, rb);

        Character.JumpPressedEvent += JumpOff;
    }

    public override void CharCollisionStay(Collision2D collision)
    {
        if (IsSticking || Character.IsGrounded)
            return;

        if (collision.gameObject.TryGetComponent<StickyWall>(out var stickyWall))
        {
            Stick(collision.contacts[0].point, stickyWall.Normal);
        }
    }

    public void Stick(Vector2 contactPoint, Vector2 wallNormal)
    {
        if (IsSticking)
            return;

        _wallNormal = wallNormal.normalized;

        _attachPoint = contactPoint + wallNormal * (Character.transform.lossyScale.x * 0.5f);

        IsSticking = true;
        _stickTimer = _stickDuration;

        Rb.linearVelocity = Vector2.zero;

        Rb.gravityScale = 0f;

        Character.transform.position = _attachPoint;
    }

    public override void FixedTick()
    {
        if (!IsSticking)
            return;

        _stickTimer -= Time.fixedDeltaTime;

        if (_stickTimer<= 0f)
        {
            Unstick();
            return;
        }

        Character.transform.position = _attachPoint;
    }

    public void JumpOff()
    {
        if (!IsSticking)
            return;

        Unstick();

        Vector2 force = _wallNormal * _jumpOffForce.x + Vector2.up * _jumpOffForce.y;

        Rb.linearVelocity = Vector2.zero;

        Rb.AddForce(force, ForceMode2D.Impulse);
    }

    private void Unstick()
    {
        if (!IsSticking)
            return;

        Character.transform.position += (Vector3)(_wallNormal * 0.1f);

        Rb.AddForce(_wallNormal * _jumpOffForce.x * 0.5f, ForceMode2D.Impulse);

        IsSticking = false;
        Rb.gravityScale = 1f;
    }
}

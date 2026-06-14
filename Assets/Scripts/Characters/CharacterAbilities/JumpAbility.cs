using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Abilities/Jump")]
public class JumpAbility : CharacterAbility
{
    [Header("Jump fields")]
    [SerializeField] protected float jumpForce;
    [SerializeField] protected int _extraJumpsCount;
    //if it's > 1, jump force increases in each jump, if < 1, jump force decreases in each jump.
    [SerializeField] private float _jumpForceModifier;

    protected int _currentJumpCount = 0;

    public override void Initialize(Character character, Rigidbody2D rb)
    {
        base.Initialize(character, rb);
        Character.TouchGroundEvent -= ResetJumps;
        Character.TouchGroundEvent += ResetJumps;
    }

    public override void ProcessJump(InputAction.CallbackContext context)
    {
        if (context.started)
        { 
            Character.JumpPressedEvent?.Invoke();

            if (CanJump())
                Jump();
        }

        if (context.canceled)
            Character.JumpReleasedEvent?.Invoke();
    }

    private void OnDestroy()
    {
        if (Character != null)
            Character.TouchGroundEvent -= ResetJumps;
    }

    private void ResetJumps()
    {
        _currentJumpCount = 0;
    }

    public virtual void Jump()
    {
        float force = (jumpForce* (float)Math.Pow(_jumpForceModifier, _currentJumpCount));
        ApplyJumpForce(force);
        _currentJumpCount++;

        Character.ForceSetGrounded(false);
    }

    public virtual void RequestJump()
    {
        if (CanJump())
            Jump();
    }

    public virtual bool CanJump()
    {
        bool canUseCoyote = Time.time - Character.LastGroundedTime <= Character.CoyoteTime &&
                            _currentJumpCount == 0;
        return Character.IsGrounded || canUseCoyote || _currentJumpCount < _extraJumpsCount;
    }

    public void ApplyJumpForce(float jumpForce)
    {
        Vector2 vel = Rb.linearVelocity;
        vel.y = 0f;
        Rb.linearVelocity = vel;

        Rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }
}
using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Abilities/Jump")]
public class JumpAbility : CharacterAbility
{
    [Header("Jump fields")]
    [SerializeField] protected float jumpForce;
    [SerializeField] protected int jumpsCount;
    [SerializeField] protected float holdForce = 15f;
    [SerializeField] protected float maxHoldTime = 0.2f;
    //if it's > 1, jump force increases in each jump, if < 1, jump force decreases in each jump.
    [SerializeField] private float jumpForceModifier;

    protected int currentJumpCount = 0;
    protected float jumpHoldTimer;
    protected bool isHoldingJump;

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

            RequestJump();
        }

        if (context.canceled)
        {
            Character.JumpReleasedEvent?.Invoke();
            isHoldingJump = false;
        }
    }

    private void OnDestroy()
    {
        if (Character != null)
            Character.TouchGroundEvent -= ResetJumps;
    }

    private void ResetJumps()
    {
        if (Rb.linearVelocity.y > 0.1f)
            return;

        currentJumpCount = 0;
        isHoldingJump = false;
    }

    public virtual void Jump()
    {
        float force = (jumpForce * (float)Math.Pow(jumpForceModifier, currentJumpCount));
        ApplyJumpForce(force);
        currentJumpCount++;

        Character.ForceSetGrounded(false);
    }

    public virtual void RequestJump()
    {
        if (CanJump())
        {
            isHoldingJump = true;
            jumpHoldTimer = 0f;
            Jump();
        }
    }

    public override void FixedTick()
    {
        base.FixedTick();

        if (isHoldingJump)
        {
            jumpHoldTimer += Time.fixedDeltaTime;

            if (jumpHoldTimer <= maxHoldTime)
            {
                int activeJumpIndex = Mathf.Max(0, currentJumpCount - 1);
                float multiplier = (float)Math.Pow(jumpForceModifier, activeJumpIndex);

                float addedForce = holdForce * multiplier;
                Rb.AddForce(Vector2.up * addedForce, ForceMode2D.Force);
            }
        }
    }

    public virtual bool CanJump()
    {
        bool canUseCoyote = Time.time - Character.LastGroundedTime <= Character.CoyoteTime &&
                            currentJumpCount == 0;
        return Character.IsGrounded || canUseCoyote || currentJumpCount < jumpsCount;
    }

    public void ApplyJumpForce(float jumpForce)
    {
        Vector2 vel = Rb.linearVelocity;
        vel.y = 0f;
        Rb.linearVelocity = vel;

        Rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }
}
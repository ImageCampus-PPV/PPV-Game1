using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Glide")]
public class GlideAbility : CharacterAbility
{
    [SerializeField] private float _glideGravityScale;
    private float _normalGravityScale;
    private bool _isHoldingJump;

    public bool IsGliding { get; private set; }

    public override void Initialize(Character character, Rigidbody2D rb)
    {
        base.Initialize(character, rb);

        _normalGravityScale = Rb.gravityScale;

        Character.JumpPressedEvent += OnJumpPressed;
        Character.JumpReleasedEvent += OnJumpReleased;
        Character.TouchGroundEvent += OnTouchGround;
    }

    public void OnJumpPressed()
    {
        _isHoldingJump = true;
    }

    public void OnJumpReleased()
    {
        _isHoldingJump = false;
        StopGliding();
    }

    private void StopGliding()
    {
        IsGliding = false;

        //Debug.Log("Gliding Stopped");

        Rb.gravityScale = _normalGravityScale;
    }

    public void OnTouchGround()
    {
        StopGliding();
    }

    public override void Tick()
    {
        bool shouldGlide = _isHoldingJump && 
                           Rb.linearVelocity.y < 0f;

        if (shouldGlide && !IsGliding)
            StartGliding();
        else if (!shouldGlide && IsGliding)
            StopGliding();
    }

    private void StartGliding()
    {
        IsGliding = true;
        Rb.gravityScale = _glideGravityScale;
        //Debug.Log("Gliding started");
    }
}
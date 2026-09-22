using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Glide")]
public class GlideAbility : CharacterAbility
{
    [SerializeField] private float _glideGravityScale;
    private float _normalGravityScale;
    private bool _isHoldingJump;

    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
    public bool IsGliding { get; private set; }

    public override void Initialize(Character character, Rigidbody2D rb)
    {
        base.Initialize(character, rb);

        _normalGravityScale = Rb.gravityScale;

        EventBus.Subscribe<OnCharacterJumpPressed>(OnJumpPressed);
        EventBus.Subscribe<OnCharacterJumpReleased>(OnJumpReleased);
        EventBus.Subscribe<OnCharacterTouchedGround>(OnTouchGround);
    }

    public void OnJumpPressed(in OnCharacterJumpPressed onCharacterJumpPressed)
    {
        if (Character.ID != onCharacterJumpPressed.characterID)
            return;

        _isHoldingJump = true;
    }

    public void OnJumpReleased(in OnCharacterJumpReleased onCharacterJumpReleased)
    {
        if (Character.ID != onCharacterJumpReleased.characterID)
            return;

        _isHoldingJump = false;
        StopGliding();
    }

    private void StopGliding()
    {
        IsGliding = false;

        //Debug.Log("Gliding Stopped");

        Rb.gravityScale = _normalGravityScale;
    }

    public void OnTouchGround(in OnCharacterTouchedGround onCharacterTouchedGround)
    {
        if (Character.ID != onCharacterTouchedGround.characterID)
            return;

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
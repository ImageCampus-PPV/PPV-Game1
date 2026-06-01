using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Movement")]
public class MovementAbility : CharacterAbility
{
    [Header("Movement fields")]
    [SerializeField] private float _maxSpeed = 6f;
    [SerializeField] private float _initialSpeed = 2f;
    [SerializeField] private float _accelerationReductOnAir = 0.5f;
    [SerializeField] private float _accelerationTime = 1f;
    [SerializeField] private float _decelerationTime = 0.5f;
    [SerializeField] private float _revDecelerationMultiplier = 2f;

    private float _currentXInput = 0;
    private float _currentAccelerationTime = 0f;
    private float _currentXVelocity;

    public override void Initialize(Character character, Rigidbody2D rb)
    {
        base.Initialize(character, rb);
        _currentXVelocity = _initialSpeed;
    }

    public override void ProcessMove(Vector2 input)
    {
        _currentXInput = Math.Clamp(input.x, -1f, 1f);
    }

    public override void FixedTick()
    {
        UpdateVelocity();
        Character.ApplyHVelocity(_currentXVelocity);
    }

    private void UpdateVelocity()
    {
        float accelerationRate = (_maxSpeed - _initialSpeed) / _accelerationTime;
        float decelerationRate = _maxSpeed / _decelerationTime;

        if (!Character.IsGrounded)
            accelerationRate *= _accelerationReductOnAir;

        float currentSpeed = Mathf.Abs(_currentXVelocity);

        if (Math.Abs(_currentXInput) < 0.01f)
        {
            _currentXVelocity = Mathf.MoveTowards(
                _currentXVelocity,
                0f,
                decelerationRate * Time.fixedDeltaTime);
            return;
        }

        float inputSign = Mathf.Sign(_currentXInput);
        float velocitySign = Mathf.Sign(_currentXVelocity);

        if (currentSpeed > 0.01f && velocitySign != inputSign)
        {
            _currentXVelocity = Mathf.MoveTowards(
                _currentXVelocity,
                0f,
                decelerationRate * _revDecelerationMultiplier * Time.fixedDeltaTime);


            if (currentSpeed >= _maxSpeed - 0.1f)
            {
                //TODO: Skidding animation
            }

            return;
        }

        if (currentSpeed < 0.01f)
        {
            _currentXVelocity = _initialSpeed * inputSign;
        }

        _currentXVelocity = Mathf.MoveTowards(_currentXVelocity, _maxSpeed * inputSign, accelerationRate * Time.fixedDeltaTime);
    }
}
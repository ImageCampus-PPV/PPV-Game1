using UnityEngine;

public class FlockingMovement : IMovementSteering
{
    private readonly SteeringContext _context = new();

    private readonly ISteeringBehaviour[] _behaviours;

    public FlockingMovement(params ISteeringBehaviour[] behaviours)
    {
        _behaviours = behaviours;
    }

    public Vector2 GetDesiredVelocity(Rigidbody2D rb,
                                      Vector2 targetPosition,
                                      float speed)
    {
        _context.TargetPosition = targetPosition;
        _context.DesiredDirection = (targetPosition - rb.position).normalized;
        _context.DistanceToTarget = Vector2.Distance(rb.position, targetPosition);

        Vector2 desiredDirection = _context.DesiredDirection;

        Vector2 steering = Vector2.zero;

        foreach (var behaviour in _behaviours)
        {
            steering += behaviour.GetSteering(
                rb,
                desiredDirection,
                _context);
        }

        if (steering.sqrMagnitude < 0.05f)
        {
            if (_context.HasDirection)
                steering = _context.LastSuccessfulDirection;
        }
        else
        {
            steering.Normalize();
            _context.LastSuccessfulDirection = steering;
            _context.HasDirection = true;
        }

        Vector2 desiredVelocity = steering * speed;

        desiredVelocity = Vector2.ClampMagnitude(desiredVelocity, speed);

        return desiredVelocity;
    }
}
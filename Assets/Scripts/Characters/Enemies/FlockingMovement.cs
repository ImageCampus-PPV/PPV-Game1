using UnityEngine;

public class FlockingMovement : IMovementSteering
{
    private readonly SteeringContext _context = new();

    private readonly ISteeringBehaviour[] _behaviours;
    private readonly FlockingSettings _settings;

    public FlockingMovement(FlockingSettings settings, params ISteeringBehaviour[] behaviours)
    {
        //Debug.Log("Steering behaviours: " + behaviours.Length);
        _behaviours = behaviours;
        _settings = settings;
    }

    public Vector2 GetDesiredVelocity(Rigidbody2D rb, Vector2 targetPosition, float speed)
    {
        //Debug.Log("Getting desired velocity");
        _context.TargetPosition = targetPosition;
        _context.DesiredDirection = (targetPosition - rb.position).normalized;
        _context.DistanceToTarget = Vector2.Distance(rb.position, targetPosition);

        if (!_settings.enabled)
        {
            return _context.DesiredDirection * speed;
        }

        Vector2 desiredDirection = _context.DesiredDirection;

        Vector2 steering = Vector2.zero;

        foreach (ISteeringBehaviour behaviour in _behaviours)
        {
            //Debug.Log("Behaviour added: " + behaviour);
            Vector2 behaviourSteering = behaviour.GetSteering(rb, desiredDirection, _context);
            //Debug.Log("Behaviour steering: " + behaviourSteering);
            steering += behaviourSteering;
        }

        if (steering.sqrMagnitude < 0.05f)
        {
            //Debug.Log("Steering too small, using last direction");
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

        //Debug.Log("Final velocity: " + desiredVelocity);
        return desiredVelocity;
    }
}
using UnityEngine;

public class ObstacleAvoidanceSteering : ISteeringBehaviour
{
    private readonly float _bodyRadius;
    private readonly float _obstacleLookDistance;
    private readonly LayerMask _obstacleLayers;
    private readonly float _weight;
    private readonly float _normalWeight;
    private readonly float _tangentWeight;

    public ObstacleAvoidanceSteering(LayerMask obstaclesLayer, FlockingSettings settings)
    {
        _obstacleLayers = obstaclesLayer;
        _bodyRadius = settings.bodyRadius;
        _obstacleLookDistance = settings.obstacleLookDistance;
        _weight = settings.obstacleWeight;
        _normalWeight = settings.obstacleNormalWeight;
        _tangentWeight = settings.obstacleTangentWeight;
    }

    public Vector2 GetSteering(Rigidbody2D rb, Vector2 desiredDirection, SteeringContext context)
    {
        RaycastHit2D hit = Physics2D.CircleCast(rb.position,
                                                _bodyRadius,
                                                desiredDirection,
                                                _obstacleLookDistance,
                                                _obstacleLayers);

        if (!hit)
            return Vector2.zero;

        //how much should I avoid the obstacle? If it's too far, I won't even try.
        //if it's too far, strength = 0. If it's close, strength = 1.
        float strength = 1f - (hit.distance / _obstacleLookDistance);

        //the tangent of the surface to slide off of it instead of just moving backwards
        Vector2 tangent = Vector2.Perpendicular(hit.normal);

        //this prevents me from just going to the oppossite direction and instead preserve the original intention.
        if (Vector2.Dot(tangent, desiredDirection) < 0)
            tangent = -tangent;

        //normal weight: get out of the obstacle (hit normal)
        //tangent weight: slide along the obstacle (tangent)
        //if the normal weight is too low the enemy is more likely to clip into the obstacle. This is not handled like a collision, but like an intention of movement.
        Vector2 avoid = hit.normal * _normalWeight + tangent * _tangentWeight;

        return _weight * strength * avoid.normalized;
    }
}

using UnityEngine;

public class ObstacleAvoidanceSteering : ISteeringBehaviour
{
    private readonly float _bodyRadius;
    private readonly float _obstacleLookRadius;
    private readonly LayerMask _obstacleLayers;
    private readonly float _weight;

    public ObstacleAvoidanceSteering(LayerMask obstaclesLayer, FlockingSettings settings)
    {
        _obstacleLayers = obstaclesLayer;
        _bodyRadius = settings.bodyRadius;
        _obstacleLookRadius = settings.obstacleLookRadius;
        _weight = settings.obstacleWeight;
    }

    public Vector2 GetSteering(Rigidbody2D rb, Vector2 desiredDirection, SteeringContext context)
    {
        RaycastHit2D hit = Physics2D.CircleCast(rb.position,
                                                _bodyRadius,
                                                desiredDirection,
                                                _obstacleLookRadius,
                                                _obstacleLayers);

        if (!hit)
            return Vector2.zero;

        float strength = 1f - (hit.distance / _obstacleLookRadius);

        Vector2 tangent = Vector2.Perpendicular(hit.normal);

        if (Vector2.Dot(tangent, desiredDirection) < 0)
            tangent = -tangent;

        Vector2 avoid =
            hit.normal * 0.7f +
            tangent * 0.3f;

        return avoid.normalized * strength * _weight;
    }
}

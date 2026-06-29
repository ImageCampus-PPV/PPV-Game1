using UnityEngine;

public class SeparationSteering : ISteeringBehaviour
{
    private readonly float _radius;
    private readonly float _weight;
    private readonly LayerMask _identityLayer;
    private readonly Collider2D[] _overlapBuffer = new Collider2D[16];

    public SeparationSteering(LayerMask identityLayer, FlockingSettings settings)
    {
        _identityLayer = identityLayer;
        _radius = settings.separationRadius;
        _weight = settings.separationWeight;
    }

    public Vector2 GetSteering(Rigidbody2D rb, Vector2 desiredDirection, SteeringContext context)
    {
        int count = Physics2D.OverlapCircle(rb.position,
                                            _radius,
                                            new ContactFilter2D
                                            {
                                                useLayerMask = true,
                                                layerMask = _identityLayer
                                            },
                                            _overlapBuffer);

        Vector2 force = Vector2.zero;

        for (int i = 0; i < count; i++)
        {
            Rigidbody2D otherRb = _overlapBuffer[i].attachedRigidbody;

            if (otherRb == rb)
                continue;

            Vector2 myFuture = rb.position + rb.linearVelocity * 0.3f;
            Vector2 otherFuture = otherRb.position + otherRb.linearVelocity * 0.3f;

            Vector2 delta = myFuture - otherFuture;

            float distance = delta.magnitude;

            if (distance < 0.001f || distance > _radius)
                continue;

            Vector2 relativeVelocity = otherRb.linearVelocity - rb.linearVelocity;

            float approaching =
                Vector2.Dot(relativeVelocity, -delta.normalized);

            if (approaching <= 0f)
                continue;

            float strength = (_radius - distance) / _radius;

            force += delta.normalized * strength;
        }

        float alignment = Vector2.Dot(force.normalized, desiredDirection);

        force = Vector2.ClampMagnitude(force, 1f);

        force *= Mathf.Lerp(0.3f, 1f, (alignment + 1f) * 0.5f);

        return force * _weight;
    }

}

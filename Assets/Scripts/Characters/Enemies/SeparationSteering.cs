using UnityEngine;

public class SeparationSteering : ISteeringBehaviour
{
    private readonly float _radius;
    private readonly float _weight;
    private readonly float _predictionTime;
    private readonly LayerMask _identityLayer;
    private const int MAX_STORED_COLLIDERS = 16; 
    private readonly Collider2D[] _overlapBuffer = new Collider2D[MAX_STORED_COLLIDERS];

    public SeparationSteering(LayerMask identityLayer, FlockingSettings settings)
    {
        _identityLayer = identityLayer;
        _radius = settings.separationRadius;
        _weight = settings.separationWeight;
        _predictionTime = settings.separationPredictionTime;
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

        //separation force
        Vector2 force = Vector2.zero;

        for (int i = 0; i < count; i++)
        {
            Rigidbody2D otherRb = _overlapBuffer[i].attachedRigidbody;

            if (otherRb == rb)
                continue;

            //where I'll be
            Vector2 myFuture = rb.position + rb.linearVelocity * _predictionTime;
            //where the other will be
            Vector2 otherFuture = otherRb.position + otherRb.linearVelocity * _predictionTime;

            //how far will we be?
            Vector2 meToOtherVec = myFuture - otherFuture;

            float distance = meToOtherVec.magnitude;

            if (distance < Mathf.Epsilon || distance > _radius)
                continue;

            Vector2 relativeVelocity = otherRb.linearVelocity - rb.linearVelocity;

            //are we approaching each other? 
            float approaching = Vector2.Dot(relativeVelocity, -meToOtherVec.normalized);

            if (approaching <= 0f)
                //not approaching
                continue;

            //separation strength
            float strength = (_radius - distance) / _radius;

            force += meToOtherVec.normalized * strength;
        }

        //how much does my separation force agree from the direction I want to move to? (1: same direction, -1: opposite direction, 0: perpendicular)
        float alignment = Vector2.Dot(force.normalized, desiredDirection);

        force = Vector2.ClampMagnitude(force, 1f);

        //instead of -1, 0, 1 it does from 0 to 1 since lerp is not meant to use negative values. 0 is -1, 0.5 is 0, 1 is 1.
        float interval = (alignment + 1f) * 0.5f;

        //if it's not aligned, the force will be reduced by 30%, otherwise it preserves all force.
        force *= Mathf.Lerp(0.3f, 1f, interval);

        return force * _weight;
    }

}

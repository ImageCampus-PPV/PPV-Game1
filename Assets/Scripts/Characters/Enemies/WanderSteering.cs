using UnityEngine;

public class WanderSteering : ISteeringBehaviour
{
    private readonly float _weight;
    private readonly float _jitter;
    private readonly float _radius;

    public WanderSteering(FlockingSettings settings)
    {
        _weight = settings.wanderWeight;
        _jitter = settings.wanderJitter;
        _radius = settings.wanderRadius;
    }

    public Vector2 GetSteering(Rigidbody2D rb,
                               Vector2 desiredDirection,
                               SteeringContext context)
    {
        if (!context.WanderInitialized)
        {
            context.WanderAngle = UnityEngine.Random.Range(0f, 360f);
            context.WanderOffset = UnityEngine.Random.Range(0f, 1000f);
            context.WanderInitialized = true;
        }

        float noise = Mathf.PerlinNoise(context.WanderOffset, Time.time * 0.5f);

        noise = (noise - .5f) * 2f;

        context.WanderAngle += noise * _jitter * Time.deltaTime;

        float radians = context.WanderAngle * Mathf.Deg2Rad;

        Vector2 wander = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));

        return _radius * _weight * wander;
    }
}

using UnityEngine;

public class WanderSteering : ISteeringBehaviour
{
    private readonly float _weight;
    private readonly float _jitter;
    private readonly float _strength;

    public WanderSteering(FlockingSettings settings)
    {
        _weight = settings.wanderWeight;
        _jitter = settings.wanderJitter;
        _strength = settings.wanderStrength;
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

        //Perlin noise generates smooth random values with a function.
        //The values tend to be similar, so the direction changes gradually so it doesn't
        //jump between random values every frame.
        //The time * 0.5 makes the travel through the noise slower, causing the direction to change slower.
        float noise = Mathf.PerlinNoise(context.WanderOffset, Time.time * 0.5f);

        //the noise will most often return a value between 0 and 1 but we want from -1 to 1. 
        //first I do noise - 0.5f, so I have a range of -0.5 to 0.5. Then I multiply it by 2, and I get a -1 to 1 range.
        //if noise is -1: turn to one side as far as possible.
        //if noise is 0: don't turn.
        //if noise is 1: turn to the other side as far as possible.
        noise = (noise - 0.5f) * 2f;

        //this handles how the angle changes.
        //noise: to which direction should I turn?
        //jitter: how much can I turn every time?
        context.WanderAngle += noise * _jitter * Time.deltaTime;

        float radians = context.WanderAngle * Mathf.Deg2Rad;

        //wander vector (gets the direction from the angle)
        Vector2 wander = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));

        return _strength * _weight * wander;
    }
}

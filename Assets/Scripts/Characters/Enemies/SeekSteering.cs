using UnityEngine;

public class SeekSteering : ISteeringBehaviour
{
    private readonly float _weight;

    public SeekSteering(SteeringSettings settings)
    {
        _weight = settings.seekWeight;
    }

    public Vector2 GetSteering(Rigidbody2D rb,
                               Vector2 desiredDirection,
                               SteeringContext context)
    {
        return desiredDirection * _weight;
    }
}

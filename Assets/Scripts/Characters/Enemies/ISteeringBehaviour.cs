using UnityEngine;

public interface ISteeringBehaviour
{
    Vector2 GetSteering(Rigidbody2D rb,
                        Vector2 targetPosition,
                        SteeringContext context);
}

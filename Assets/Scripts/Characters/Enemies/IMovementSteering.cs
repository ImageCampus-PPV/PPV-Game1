using UnityEngine;

public interface IMovementSteering
{
    Vector2 GetDesiredVelocity(Rigidbody2D rb, Vector2 desiredDirection, float speed);
}

    
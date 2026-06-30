using UnityEngine;

public class SteeringContext
{
    public Vector2 LastSuccessfulDirection = Vector2.right;
    public bool HasDirection;

    public float WanderAngle;
    public float WanderOffset;
    public bool WanderInitialized;

    public Vector2 TargetPosition;
    public Vector2 DesiredDirection;
    public float DistanceToTarget;
    public float OrbitDirection;
    public bool OrbitInitialized;
}

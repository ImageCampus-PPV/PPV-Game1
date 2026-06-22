using System;
using UnityEngine;

public abstract class EnemyMovementStrategy : ScriptableObject
{
    public abstract void Initialize(Rigidbody2D rb, Vector2 startPos);
    public abstract void Move();
    public abstract void Stop();
    public abstract void Resume();
    public abstract bool IsMoving { get; }
}

using UnityEngine;

public interface IEnemyContext : IStateContext
{
    Transform Transform { get; }
    Vector2 Position { get; }
    Vector2 PositionOnSpawn { get; }
    float MaxHealth { get; }
    Transform AttackOffset { get; }
}

using UnityEngine;

public interface IEnemyContext : IStateContext
{
    Transform Transform { get; }
    Vector2 Position { get; }
    float Health { get; }
    float MaxHealth { get; }
    Transform AttackOffset { get; }
}

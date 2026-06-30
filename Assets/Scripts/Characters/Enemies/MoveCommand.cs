using UnityEngine;

public class MoveCommand : ICommand
{
    public Vector2? Target { get; }
    public float Speed { get; }
    public MoveCommand(Vector2? target, float speed) 
    { 
        Target = target;
        Speed = speed; 
    }
}

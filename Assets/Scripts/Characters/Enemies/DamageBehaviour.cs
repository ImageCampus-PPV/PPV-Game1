using UnityEngine;

public abstract class DamageBehaviour
{
    public abstract void Tick(float deltaTime);

    public abstract void OnCollisionStay(Collision2D collision);
}

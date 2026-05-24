using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerView : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rb;

    public Rigidbody2D Rb { get => _rb; set => _rb = value; }

    public void ApplyHVelocity(float xVel)
    {
        Vector2 vel = _rb.linearVelocity;
        vel.x = xVel;
        _rb.linearVelocity = vel;
    }

    public void ApplyJumpForce(float jumpForce)
    {
        _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }
}

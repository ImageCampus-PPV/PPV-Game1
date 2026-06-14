using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    private float _damage;
    private float _speed;
    private float _range;
    private LayerMask _enemyLayer;
    private Vector2 _direction;
    private Vector2 _startPosition;

    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(float damage, float speed, float range, LayerMask enemyLayer, Vector2 direction)
    {
        _damage = damage;
        _speed = speed;
        _range = range;
        _enemyLayer = enemyLayer;
        _direction = direction.normalized;
        _startPosition = transform.position;

        _rb.linearVelocity = _direction * _speed;
    }

    private void Update()
    {
        if (Vector2.Distance(_startPosition, transform.position) >= _range)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((_enemyLayer.value & (1 << other.gameObject.layer)) == 0) 
            return;

        other.GetComponent<IDamageable>()?.TakeDamage(_damage);
        Destroy(gameObject);
    }
}

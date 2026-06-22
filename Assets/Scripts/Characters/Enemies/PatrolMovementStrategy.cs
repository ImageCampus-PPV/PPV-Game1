using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Movement/PatrolMovementStrategy")]
public class PatrolMovementStrategy : EnemyMovementStrategy
{
    [SerializeField] private float _moveSpeed = 4f;
    [SerializeField] private float _moveRange = 3f;
    [SerializeField] private float _changeDirDelay = 0.5f;

    private Rigidbody2D _rb;
    private Vector2 _startPos;
    private float _leftBound;
    private float _rightBound;
    //1 = derecha, -1 = izquierda
    private int _direction = 1;
    private float _pauseTimer;
    private bool _isMoving = true;

    public override bool IsMoving => _isMoving && _pauseTimer <= 0f;

    public override void Initialize(Rigidbody2D rb, Vector2 startPos)
    {
        _rb = rb;
        _startPos = startPos;
        _leftBound = _startPos.x - _moveRange;
        _rightBound = _startPos.x + _moveRange;
    }

    public override void Move()
    {
        if (!_isMoving)
            return;

        if (_pauseTimer > 0f)
        {
            _pauseTimer -= Time.deltaTime;
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        float targetX = _direction == 1 ? _rightBound : _leftBound;
        float diff = targetX - _rb.position.x;

        if (Mathf.Abs(diff) < 0.05f)
        {
            _rb.linearVelocity = Vector2.zero;
            _direction *= -1;
            _pauseTimer = _changeDirDelay;
        }
        else
        {
            float speed = _direction * _moveSpeed;
            _rb.linearVelocity = new Vector2(speed, 0);
        }
    }

    public override void Stop()
    {
        _isMoving = false;
        _rb.linearVelocity = Vector2.zero;
    }

    public override void Resume()
    {
        _isMoving = true;
    }
}


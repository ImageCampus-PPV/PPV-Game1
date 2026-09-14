using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Behaviour/GroundPatrol")]
public class GroundPatrolBehaviour : StateBehaviour<IEnemyContext>
{
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _lookAheadDistance = 0.5f;
    [SerializeField] private float _groundCheckDistance = 1f;
    [SerializeField] private float _wallCheckDistance = 0.25f;
    [SerializeField] private float _floorCheckYOffset = 0.5f;
    [SerializeField] private float _pauseDuration = 0.25f;

    private int _direction = 1;
    private float _pauseTimer;

    public override BehaviourActions GetOnEnter(IEnemyContext context)
    {
        BehaviourActions actions = new BehaviourActions();

        actions.AddUpdateBehaviour(() =>
        {
            _direction = 1;
            _pauseTimer = 0f;
        });

        return actions;
    }

    public override BehaviourActions GetOnExit(IEnemyContext context)
    {
        BehaviourActions actions = new BehaviourActions();

        actions.AddUpdateBehaviour(() =>
        {
            context.Execute(new StopMovementCommand());
        });

        return actions;
    }

    public override BehaviourActions GetOnTick(IEnemyContext context)
    {
        BehaviourActions actions = new BehaviourActions();

        actions.AddUpdateBehaviour(() =>
        {
            if (_pauseTimer > 0f)
            {
                _pauseTimer -= Time.deltaTime;
                context.Execute(new StopMovementCommand());
                return;
            }

            Vector2 position = context.Position;

            Vector2 floorCheckOrigin = position + Vector2.right * (_direction * _lookAheadDistance);

            bool isThereGround = Physics2D.Raycast(floorCheckOrigin, Vector2.down * _floorCheckYOffset, _groundCheckDistance, _groundLayer);
            bool isThereWall = Physics2D.Raycast(position, Vector2.right * _direction, _wallCheckDistance, _groundLayer);

            if (!isThereGround || isThereWall)
            {
                _direction *= -1;
                _pauseTimer = _pauseDuration;
                context.Execute(new StopMovementCommand());
                return;
            }

            Vector2 targetPosition = position + Vector2.right * (_direction * _lookAheadDistance);

            context.Execute(new MoveCommand(targetPosition, _moveSpeed));
        });

        return actions;
    }
}
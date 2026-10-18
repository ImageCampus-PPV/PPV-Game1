using GreenAbyss.FSM;
using GreenAbyss.Services;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Behaviour/GroundPatrol")]
public class GroundPatrolBehaviour : StateBehaviour<IEnemyContext>
{
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private LayerMask _wallLayer;
    [SerializeField] private float _lookAheadDistance = 0.5f;
    [SerializeField] private float _groundCheckDistance = 1f;
    [SerializeField] private float _wallCheckDistance = 0.25f;
    [SerializeField] private float _floorCheckYOffset = 0.5f;
    [SerializeField] private float _pauseDuration = 0.25f;

    private int _direction = 1;
    private float _pauseTimer;
    private RuntimeDebugVisual RuntimeDebugVisual => ServiceProvider.Instance.GetService<RuntimeDebugVisual>();

    public override GreenAbyss.FSM.BehaviourActions GetOnEnter(IEnemyContext context)
    {
        GreenAbyss.FSM.BehaviourActions actions = new GreenAbyss.FSM.BehaviourActions();

        actions.AddUpdateBehaviour(() =>
        {
            _direction = 1;
            _pauseTimer = 0f;
        });

        return actions;
    }

    public override GreenAbyss.FSM.BehaviourActions GetOnExit(IEnemyContext context)
    {
        GreenAbyss.FSM.BehaviourActions actions = new GreenAbyss.FSM.BehaviourActions();

        actions.AddUpdateBehaviour(() =>
        {
            context.Execute(new StopMovementCommand());
        });

        return actions;
    }

    public override GreenAbyss.FSM.BehaviourActions GetOnTick(IEnemyContext context)
    {
        GreenAbyss.FSM.BehaviourActions actions = new GreenAbyss.FSM.BehaviourActions();

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
            RaycastHit2D isThereWall = Physics2D.Raycast(position, Vector2.right * _direction, _wallCheckDistance, _wallLayer);

            //RuntimeDebugVisual.DrawRay(context.Transform.position, Vector2.right * _direction, _wallCheckDistance, Color.black, 3f);

            if (isThereWall)
                Debug.Log($"{context.Transform.name}: wall detected {isThereWall.transform.name}");

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

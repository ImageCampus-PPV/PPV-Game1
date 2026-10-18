
using GreenAbyss.Services;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Behaviour/GroundChase")]
public class GroundChaseBehaviour : StateBehaviour<IEnemyContext>
{
    [SerializeField] private float _moveSpeed = 4f;
    [SerializeField] private float _targetLookRange = 100f;
    [SerializeField] private LayerMask _targetMask;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private LayerMask _wallLayer;
    [SerializeField] private float _lookAheadDistance = 0.5f;
    [SerializeField] private float _groundCheckDistance = 1f;
    [SerializeField] private float _wallCheckDistance = 0.25f;
    [SerializeField] private float _floorCheckYOffset = 0.5f;

    private int _direction = 1;
    private RuntimeDebugVisual RuntimeDebugVisual => ServiceProvider.Instance.GetService<RuntimeDebugVisual>();

    public override GreenAbyss.FSM.BehaviourActions GetOnEnter(IEnemyContext context)
    {
        GreenAbyss.FSM.BehaviourActions actions = new GreenAbyss.FSM.BehaviourActions();

        actions.AddUpdateBehaviour(() =>
        {
            context.Execute(new ResumeMovementCommand());
        });

        return actions;
    }

    public override GreenAbyss.FSM.BehaviourActions GetOnTick(IEnemyContext context)
    {
        GreenAbyss.FSM.BehaviourActions actions = new GreenAbyss.FSM.BehaviourActions();

        actions.AddUpdateBehaviour(() =>
        {

            Transform target = context.ExecuteQuery(new FindTargetQuery(_targetLookRange, _targetMask));

            if (target == null)
            {
                context.Execute(new StopMovementCommand());
                return;
            }

            Vector2 position = context.Position;
            float horizontalDifference = target.position.x - position.x;

            if (Mathf.Abs(horizontalDifference) < 0.05f)
            {
                context.Execute(new StopMovementCommand());
                return;
            }

            int targetDirection = horizontalDifference > 0f ? 1 : -1;

            Vector2 floorCheckOrigin = position + Vector2.right * (targetDirection * _lookAheadDistance);

            bool hasGround = Physics2D.Raycast(floorCheckOrigin, Vector2.down, _groundCheckDistance, _groundLayer);

            bool hasWall = Physics2D.Raycast(position, Vector2.right * targetDirection, _wallCheckDistance, _wallLayer);
            //RuntimeDebugVisual.DrawRay(context.Transform.position, Vector2.right * targetDirection, _wallCheckDistance, Color.black, 3f);

            if (!hasGround || hasWall)
            {
                context.Execute(new StopMovementCommand());
                return;
            }

            _direction = targetDirection;

            Vector2 targetPosition = position + Vector2.right * _direction * _lookAheadDistance;

            context.Execute(new MoveCommand(targetPosition, _moveSpeed));
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
}
using UnityEngine;
using UnityEngine.Assertions.Must;

[CreateAssetMenu(menuName = "Enemy/Behaviour/Patrol")]
public class PatrolBehaviour : StateBehaviour<IEnemyContext>
{
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _range = 3f;
    [SerializeField] private float _pauseDuration = 0.5f;

    private Vector2 _startPos;
    private int _direction = 1;
    private float _pauseTimer;

    public override BehaviourActions GetOnEnter(IEnemyContext context)
    {
        BehaviourActions actions = new BehaviourActions();
        actions.AddUpdateBehaviour(() =>
        {
            _startPos = context.Position;
            _direction = 1;
            _pauseTimer = 0;
        });
        return actions;
    }

    public override BehaviourActions GetOnTick(IEnemyContext context)
    {
        BehaviourActions actions = new BehaviourActions();
        actions.AddUpdateBehaviour(() =>
        {
            if (_pauseTimer > 0)
            {
                _pauseTimer -= Time.deltaTime;
                context.Execute(new StopMovementCommand());
                return;
            }

            float targetX = _startPos.x + _direction * _range;
            float diff = targetX - context.Position.x;

            if (Mathf.Abs(diff) < 0.05f)
            {
                _direction *= -1;
                _pauseTimer = _pauseDuration;
                context.Execute(new StopMovementCommand());
            }
            else
            {
                context.Execute(new MoveCommand(new Vector2(targetX, context.Position.y), _moveSpeed));
            }
        });
        return actions;
    }

    public override BehaviourActions GetOnExit(IEnemyContext context)
    {
        BehaviourActions actions = new BehaviourActions();
        actions.AddUpdateBehaviour(() => context.Execute(new StopMovementCommand()));
        return actions;
    }
}

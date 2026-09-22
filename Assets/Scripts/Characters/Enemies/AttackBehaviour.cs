using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Behaviour/Attack")]
public class AttackBehaviour : StateBehaviour<IEnemyContext>
{
    [SerializeField] private EnemyAttackStrategy _attackStrategy;
    [SerializeField] private float _attackRange = 100f;
    [SerializeField] private LayerMask _targetLayer;

    private float _cooldownTimer;
    private float _anticipationTimer;
    private bool _isAnticipating;

    public override BehaviourActions GetOnEnter(IEnemyContext context)
    {
        BehaviourActions actions = new BehaviourActions();
        actions.AddUpdateBehaviour(() =>
        {
            context.Execute(new StopMovementCommand());
            _isAnticipating = true;
            _anticipationTimer = _attackStrategy.AnticipationTime;
            _cooldownTimer = 0;
        });
        return actions;
    }

    public override BehaviourActions GetOnTick(IEnemyContext context)
    {
        BehaviourActions actions = new BehaviourActions();
        actions.AddUpdateBehaviour(() =>
        {
            context.Execute(new StopMovementCommand());
            Transform target = context.ExecuteQuery(new FindTargetQuery(_attackRange, _targetLayer));
            if (target == null) 
                return;

            if (_cooldownTimer > 0)
            {
                _cooldownTimer -= Time.deltaTime;
                return;
            }

            if (_isAnticipating)
            {
                _anticipationTimer -= Time.deltaTime;
                if (_anticipationTimer <= 0)
                {
                    _isAnticipating = false;
                    _attackStrategy.Execute(context, target);
                    _cooldownTimer = _attackStrategy.AttackCooldown;
                }
                return;
            }

            _isAnticipating = true;
            _anticipationTimer = _attackStrategy.AnticipationTime;
        });
        return actions;
    }

    public override BehaviourActions GetOnExit(IEnemyContext context)
    {
        BehaviourActions actions = new BehaviourActions();
        actions.AddUpdateBehaviour(() => context.Execute(new ResumeMovementCommand()));
        return actions;
    }
}

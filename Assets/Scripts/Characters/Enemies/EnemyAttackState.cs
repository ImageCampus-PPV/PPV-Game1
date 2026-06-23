using UnityEngine;

public class EnemyAttackState : State
{
    private float _cooldown;
    private float _anticipation;
    private bool _isAnticipating;

    public override BehaviourActions GetOnEnterBehaviour(params object[] parameters)
    {
        Enemy enemy = (Enemy)parameters[0];

        BehaviourActions actions = new();

        actions.AddUpdateBehaviour(() =>
        {
            enemy.StopMovement();

            _isAnticipating = true;
            _anticipation = enemy.AttackStrategy.AnticipationTime;
        });

        return actions;
    }

    public override BehaviourActions GetOnTickBehaviour(params object[] parameters)
    {
        Enemy enemy = (Enemy)parameters[0];

        BehaviourActions actions = new();

        actions.AddUpdateBehaviour(() =>
        {
            enemy.StopMovement();

            Transform target = enemy.FindTarget();

            if (target == null)
                return;

            if (_cooldown > 0)
            {
                _cooldown -= Time.deltaTime;
                return;
            }

            if (_isAnticipating)
            {
                _anticipation -= Time.deltaTime;

                if (_anticipation <= 0)
                {
                    _isAnticipating = false;

                    enemy.AttackStrategy.Execute(
                        enemy.transform,
                        enemy.AttackOffset,
                        target);

                    _cooldown = enemy.AttackStrategy.AttackCooldown;
                }

                return;
            }

            _isAnticipating = true;
            _anticipation = enemy.AttackStrategy.AnticipationTime;
        });

        actions.SetTransitionBehaviour(() =>
        {
            if (enemy.FindTarget() == null)
                changeState?.Invoke(typeof(EnemyPatrolState));
        });

        return actions;
    }

    public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
    {
        Enemy enemy = (Enemy)parameters[0];

        BehaviourActions actions = new();

        actions.AddUpdateBehaviour(() =>
        {
            enemy.ResumeMovement();
        });

        return actions;
    }
}
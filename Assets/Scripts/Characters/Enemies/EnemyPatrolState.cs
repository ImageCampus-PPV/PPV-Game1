public class EnemyPatrolState : State
{
    public override BehaviourActions GetOnEnterBehaviour(params object[] parameters)
    {
        Enemy enemy = (Enemy)parameters[0];

        BehaviourActions actions = new();

        actions.AddUpdateBehaviour(() =>
        {
            enemy.ResumeMovement();
        });

        return actions;
    }

    public override BehaviourActions GetOnTickBehaviour(params object[] parameters)
    {
        Enemy enemy = (Enemy)parameters[0];

        BehaviourActions actions = new();

        actions.AddUpdateBehaviour(() =>
        {
            enemy.PatrolMove();
        });

        actions.SetTransitionBehaviour(() =>
        {
            if (enemy.FindTarget() != null)
                changeState?.Invoke(typeof(EnemyAttackState));
        });

        return actions;
    }

    public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
    {
        return default(BehaviourActions);
    }
}

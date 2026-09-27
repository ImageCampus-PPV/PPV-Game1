using GreenAbyss.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Attacks/Ground Tail (Y)")]
public class TailAttackStrategy : AttackStrategy
{
    [SerializeField] private float _hitboxSizeX = 3f;
    [SerializeField] private float _hitboxSizeY = 1.5f;
    [SerializeField] private float _knockBackXForce = 10f;
    private float _currentAttackTimer;
    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    private RuntimeDebugVisual _debugVisual;

    public override void Execute(Vector2 aimDir)
    {
        if (!character.IsGrounded)
            return;

        isExecuting = true;

        _currentAttackTimer = attackSpeed;

        character.IsIgnoringInput = true;
        character.IsBlockingRotation = true;
        character.ApplyHVelocity(0);

        Vector2 attackPos = (Vector2)character.transform.position + (Vector2.up * (_hitboxSizeY / 2f));

        if (!_debugVisual)
            _debugVisual = ServiceProvider.Instance.GetService<RuntimeDebugVisual>();

        _debugVisual.DrawBox(attackPos, new(_hitboxSizeX, _hitboxSizeY), Color.magenta, attackSpeed);

        foreach (Enemy enemy in EntityRegistry.GetAllEntitiesInBox<Enemy>(attackPos, new(_hitboxSizeX, _hitboxSizeY)))
        {
            Debug.Log($"Dealing damage to entity {enemy.name}");
            EventBus.Raise<OnCombatDamage>(enemy.ID, damage);

            if (enemy.RigidBody != null)
            {
                //TODO: make this a knockback event/effect
                float dir = Mathf.Sign(enemy.transform.position.x - character.transform.position.x);
                enemy.RigidBody.linearVelocity = Vector2.zero;
                enemy.RigidBody.AddForce(new(dir * _knockBackXForce, 0), ForceMode2D.Impulse);
            }
        }
    }

    public override void Tick()
    {
        if (!isExecuting)
            return;

        _currentAttackTimer -= Time.deltaTime;

        if (_currentAttackTimer < 0f)
        {
            character.IsIgnoringInput = false;
            character.IsBlockingRotation = false;
            isExecuting = false;
        }
    }
}


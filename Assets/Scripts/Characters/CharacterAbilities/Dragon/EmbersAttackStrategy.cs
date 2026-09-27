using GreenAbyss.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Attacks/Air Embers (Y)")]
public class EmbersAttackStrategy : AttackStrategy
{
    [SerializeField] private float _beamRange = 5f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _burnDamagePerSecond = 3f;
    [SerializeField] private float _burnDuration = 3f;
    [SerializeField] private float _fireSpreadLength = 5f;
    [SerializeField] private float _fireSpreadHeight = 1f;
    [SerializeField] private float _beamLineThickness = 0.05f;
    [SerializeField] private float _beamWidth = 0.5f;
    private Vector2 _currentAim;

    private RuntimeDebugVisual DebugVisual => ServiceProvider.Instance.GetService<RuntimeDebugVisual>();
    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    public override void Execute(Vector2 aimDir)
    {
        if (character.IsGrounded)
            return;

        isExecuting = true;

        _currentAim = aimDir;
    }

    public override void Tick()
    {
        if (!isExecuting)
            return;

        if (character.IsGrounded)
        {
            Cancel();
            return;
        }

        _currentAim = character.CurrentAimDir;

        Vector2 start = character.transform.position;

        RaycastHit2D groundHit = Physics2D.Raycast(start, _currentAim, _beamRange, _groundLayer);

        Vector2 beamCenter = start + _currentAim * (_beamRange * 0.5f);
        Vector2 beamSize = new(_beamRange, _beamWidth);
        float beamAngle = Mathf.Atan2(_currentAim.y, _currentAim.x) * Mathf.Rad2Deg;

        if (DebugVisual)
            DebugVisual.DrawOrientedBox(beamCenter, beamSize, beamAngle, Color.red, Time.deltaTime, _beamLineThickness <= 0f ? 0.05f : _beamLineThickness);

        foreach (Enemy enemy in EntityRegistry.GetAllEntitiesInBox<Enemy>(beamCenter, beamSize, beamAngle))
        {
            EventBus.Raise<OnCombatDamage>(enemy.ID, damage);

            if (!enemy.HasEffect<BurnEffect>())
                enemy.ApplyEffect(new BurnEffect(_burnDuration, _burnDamagePerSecond));
        }

        if (groundHit.collider != null)
        {
            Vector2 rangeSize = new(_fireSpreadLength, _fireSpreadHeight);

            DealDamageToTargets<Enemy>(EntityRegistry.GetAllEntitiesInBox<Enemy>(groundHit.point, rangeSize), damage * Time.deltaTime);

            DebugVisual.DrawBox(groundHit.point, rangeSize, Color.orange, Time.deltaTime);
        }
    }
}

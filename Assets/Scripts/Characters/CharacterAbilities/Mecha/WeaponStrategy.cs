using GreenAbyss.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using UnityEngine;
using UnityEngine.InputSystem;

public enum WeaponSlot { Melee, Ranged }

public abstract class WeaponStrategy : ScriptableObject
{
    [Header("Base Weapon Config")]
    [SerializeField] protected float damage;
    [SerializeField] protected float range;
    [SerializeField] protected float cooldown;

    [Header("Slot Restriction")]
    [SerializeField] private WeaponSlot _allowedSlot;

    protected Character character;
    protected float lastFireTime = float.NegativeInfinity;

    public bool IsOnCooldown => Time.time - lastFireTime < cooldown;
    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    public virtual float CooldownProgress
    {
        get
        {
            if (cooldown <= 0f) return 1f;
            float elapsed = Time.time - lastFireTime;
            return Mathf.Clamp01(elapsed / cooldown);
        }
    }
    public WeaponSlot AllowedSlot => _allowedSlot;

    public virtual bool HasCharge => false;
    public virtual float ChargeProgress => 1f;

    public virtual void Initialize(Character character)
    {
        this.character = character;
    }

    public abstract void OnPressed(InputAction.CallbackContext context, Vector2 aimDir);
    public virtual void OnReleased(InputAction.CallbackContext context, Vector2 aimDir) { }
    public virtual void Tick() { }
    public virtual void FixedTick() { }
    public virtual void Cancel() { }

    protected void DealDamageInArea(Vector2 center, float radius)
    {
        foreach (Enemy enemy in EntityRegistry.GetAllEntitiesInRadius<Enemy>(center, radius))
            EventBus.Raise<OnCombatDamage>(enemy.ID, damage);
    }
}

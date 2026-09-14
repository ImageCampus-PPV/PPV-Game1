using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using UnityEngine;

public class ShieldDome : DamageableEntity
{
    private float _currentHp;
    private float _maxHp;
    private float _minHp;

    private SpriteRenderer _sr;
    private CircleCollider2D _col;

    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
    public float CurrentHp => _currentHp;
    public float MaxHp => _maxHp;
    public float HpPercent => _currentHp / _maxHp;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _col = GetComponent<CircleCollider2D>();
    }

    public void Initialize(float maxHp, float minHp, float radius, Collider2D[] friendlyColliders)
    {
        base.Init();

        _maxHp = maxHp;
        _minHp = minHp;
        _currentHp = maxHp;

        if (_col != null)
            _col.radius = radius;

        float spriteBaseDiameter = 1f;
        float scale = (radius * 2f) / spriteBaseDiameter;
        transform.localScale = Vector3.one * scale;

        if (_col != null && friendlyColliders != null)
        {
            foreach (Collider2D friendly in friendlyColliders)
            {
                if (friendly != null)
                    Physics2D.IgnoreCollision(_col, friendly, true);
            }
        }

        if (_sr != null)
            _sr.color = new Color(0f, 1f, 1f, 0.35f);
    }

    public override void TakeDamage(float damage)
    {
        if (_currentHp <= _minHp)
            return;

        _currentHp = Mathf.Max(_minHp, _currentHp - damage);

        if (_sr != null)
            _sr.color = Color.Lerp(new Color(1f, 0f, 0f, 0.35f), new Color(0f, 1f, 1f, 0.35f), HpPercent);

        if (_currentHp <= _minHp)
            EventBus.Raise<OnShieldBroken>(ID);

        EventBus.Raise<OnCombatDamage>(ID, damage);
    }

    public void Restore()
    {
        _currentHp = _maxHp;

        if (_sr != null)
            _sr.color = new Color(0f, 1f, 1f, 0.35f);
    }
}


public struct OnShieldBroken : IEvent
{
    public uint shieldID;

    public void Assign(params object[] parameters)
    {
        shieldID = (uint)parameters[0];
    }

    public void Reset()
    {
        shieldID = default(uint);
    }
}

public struct OnShieldDispatched : IEvent
{
    public uint shieldID;

    public void Assign(params object[] parameters)
    {
        shieldID = (uint)parameters[0];
    }

    public void Reset()
    {
        shieldID = default(uint);
    }
}
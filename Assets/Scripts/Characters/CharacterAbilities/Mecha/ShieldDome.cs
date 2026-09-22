using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using UnityEngine;

public class ShieldDome : DamageableEntity
{
    private SpriteRenderer _sr;
    private CircleCollider2D _col;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _col = GetComponent<CircleCollider2D>();
    }

    public void Initialize(float maxHp, float minHp, float radius, Collider2D[] friendlyColliders)
    {
        _maxHealth = maxHp;

        _currentHealth = maxHp;

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

    public void Restore()
    {
        Heal(_maxHealth);

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
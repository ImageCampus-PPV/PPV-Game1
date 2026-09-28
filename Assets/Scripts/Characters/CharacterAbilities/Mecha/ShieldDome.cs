using GreenAbyss.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using UnityEngine;

public class ShieldDome : DamageableEntity
{
    private SpriteRenderer _spriteRenderer;
    private CircleCollider2D _collider;

    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

    public override Vector2 Center => transform.position;

    public override Vector2 Size => _collider.bounds.size;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<CircleCollider2D>();
    }

    public void Initialize(float maxHp, float minHp, float radius)
    {
        maxHealth = maxHp;

        currentHealth = maxHp;

        if (_collider != null)
            _collider.radius = radius;

        float spriteBaseDiameter = 1f;
        float scale = (radius * 2f) / spriteBaseDiameter;
        transform.localScale = Vector3.one * scale;

        if (_collider != null)
        {
            foreach (Character character in EntityRegistry.FilterEntities<Character>())
            {
                Physics2D.IgnoreCollision(_collider, character.Collider, true);
            }
        }

        if (_spriteRenderer != null)
            _spriteRenderer.color = new Color(0f, 1f, 1f, 0.35f);
    }

    public void Restore()
    {
        Heal(maxHealth);

        if (_spriteRenderer != null)
            _spriteRenderer.color = new Color(0f, 1f, 1f, 0.35f);
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
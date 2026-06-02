using System;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class CharacterAbility : ScriptableObject
{
    protected Character Character { get; set; }
    protected Rigidbody2D Rb { get; set; }

    public virtual void Initialize(Character character, Rigidbody2D rb)
    {
        Character = character;
        Rb = rb;
    }

    public virtual void ProcessMove(Vector2 input) { }
    public virtual void ProcessJump(InputAction.CallbackContext context) { }
    public virtual void ProcessAction(InputAction.CallbackContext context) { }
    public virtual void ProcessSkill(InputAction.CallbackContext context) { }
    public virtual void Tick() { }
    public virtual void FixedTick() { }
    public virtual void CharCollisionStay(Collision2D collision) { }
}

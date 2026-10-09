using GreenAbyss.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;

public class Character : DamageableEntity
{
    private CoopCameraController CoopCameraController => ServiceProvider.Instance.ContainsService<CoopCameraController>() ? ServiceProvider.Instance.GetService<CoopCameraController>() : null;

    [Header("Ground checks")]
    [SerializeField] private float _coyoteTime = 0.12f;
    //TODO: Make ground check with unity
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius = 1.2f;

    [SerializeField] private CharacterDebugInfo info;

    private Vector2 _rawAimInput;
    private bool _isOnGamepad;
    private MovementAbility _activeMovement;
    private JumpAbility _activeJump;
    private List<CharacterAbility> _activeAbilities = new();

    private Rigidbody2D _rb;
    private Collider2D _ownCollider;

    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
    private RuntimeDebugVisual DebugVisual => ServiceProvider.Instance.GetService<RuntimeDebugVisual>();

    public bool IsGrounded { get; internal set; }
    public float LastGroundedTime { get; private set; }
    public float CoyoteTime => _coyoteTime;
    public bool IsIgnoringInput { get; set; }
    public bool IsBlockingAbilities { get; set; }
    public bool IsBlockingRotation { get; set; }
    public bool IsBlockingJump { get; set; }
    public Vector2 CurrentAimDir { get; private set; }
    public Rigidbody2D Rb => _rb;
    public Collider2D Collider => _ownCollider;
    public MovementAbility ActiveMovement => _activeMovement;
    public JumpAbility ActiveJump => _activeJump;
    public List<CharacterAbility> ActiveAbilities => _activeAbilities;
    public override Vector2 Center => transform.position;
    public override Vector2 Size => _ownCollider == null ? Vector2.zero : _ownCollider.bounds.size;

    public override void Init()
    {
        base.Init();

        _rb = GetComponent<Rigidbody2D>();
        _ownCollider = GetComponent<Collider2D>();
        CurrentAimDir = Vector2.right;

        EquipCharacter(info);

        if (TryGetComponent<CharacterDebugger>(out CharacterDebugger debugger))
        {
            debugger.DebugInfo = info;
            debugger.UpdateInfo();
        }
    }

    public void EquipCharacter(CharacterDebugInfo info)
    {
        IsIgnoringInput = false;
        IsBlockingRotation = false;

        CleanUpAbilities();
        _activeAbilities.Clear();

        if (info.MovementAbility != null)
        {
            _activeMovement = Instantiate(info.MovementAbility);
            _activeMovement.Initialize(this, _rb);
        }
        if (info.JumpAbility != null)
        {
            _activeJump = Instantiate(info.JumpAbility);
            _activeJump.Initialize(this, _rb);
        }

        foreach (CharacterAbility ability in info.Abilities)
        {
            if (ability == null)
            {
                Debug.LogError($"Null ability in {info.name}");
                continue;
            }

            CharacterAbility clonedAbility = Instantiate(ability);
            clonedAbility.Initialize(this, _rb);
            _activeAbilities.Add(clonedAbility);
        }
    }

    private void CleanUpAbilities()
    {
        if (_activeMovement != null)
            Destroy(_activeMovement);

        if (_activeJump != null)
            Destroy(_activeJump);

        foreach (CharacterAbility ability in _activeAbilities)
            if (ability != null)
                Destroy(ability);
    }
    public void OnAim(Vector2 dir)
    {
        if (IsIgnoringInput || IsBlockingRotation)
            return;

        _rawAimInput = dir;

        foreach (CharacterAbility ability in _activeAbilities)
            ability.ProcessAim(dir);
    }

    public void OnMove(Vector2 dir)
    {
        if (IsIgnoringInput)
            return;

        _activeMovement?.ProcessMove(dir);

        foreach (CharacterAbility ability in _activeAbilities)
            ability.ProcessMove(dir);
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (IsIgnoringInput || IsBlockingAbilities)
            return;

        _isOnGamepad = context.control.device is Gamepad;
        _activeJump?.ProcessJump(context);

        foreach (CharacterAbility ability in _activeAbilities)
            ability.ProcessJump(context);
    }

    public void OnPrimaryAction(InputAction.CallbackContext context)
    {
        if (IsIgnoringInput || IsBlockingAbilities)
            return;
        foreach (CharacterAbility ability in _activeAbilities)
        {
            ability.ProcessAction(context);
        }
    }

    public void OnSecondaryAction(InputAction.CallbackContext context)
    {
        if (IsIgnoringInput || IsBlockingAbilities)
            return;
        foreach (CharacterAbility ability in _activeAbilities)
        {
            ability.ProcessSkill(context);
        }
    }

    public void OnShield(InputAction.CallbackContext context)
    {
        if (IsIgnoringInput || IsBlockingAbilities)
            return;
        foreach (CharacterAbility ability in _activeAbilities)
        {
            if (ability is MechaCombat mechaCombat)
            {
                mechaCombat.ProcessShield(context);
                break;
            }
        }
    }

    public void OnCycleSlot1(InputAction.CallbackContext context)
    {
        if (IsIgnoringInput || IsBlockingAbilities)
            return;
        foreach (CharacterAbility ability in _activeAbilities)
        {
            if (ability is MechaCombat mechaCombat)
            {
                mechaCombat.OnCycleSlot1(context);
                break;
            }
        }
    }

    public void OnCycleSlot2(InputAction.CallbackContext context)
    {
        if (IsIgnoringInput || IsBlockingAbilities)
            return;
        foreach (CharacterAbility ability in _activeAbilities)
        {
            if (ability is MechaCombat mechaCombat)
            {
                mechaCombat.OnCycleSlot2(context);
                break;
            }
        }
    }

    public void OnSkillAction(InputAction.CallbackContext context)
    {
        if (IsIgnoringInput || IsBlockingAbilities)
            return;
        foreach (CharacterAbility ability in _activeAbilities)
        {
            ability.ProcessSkill(context);
        }
    }

    private void Update()
    {
        CheckGrounded();
        CalculateAim();
        _activeMovement?.Tick();
        _activeJump?.Tick();

        foreach (CharacterAbility ability in _activeAbilities)
            ability.Tick();
    }

    private void CalculateAim()
    {
        if (CoopCameraController == null)
            return;

        if (IsBlockingRotation)
            return;

        if (_isOnGamepad)
        {
            if (_rawAimInput.sqrMagnitude > 0.05f)
            {
                CurrentAimDir = _rawAimInput.normalized;
            }
        }

        else
        {
            if (CoopCameraController.Camera && Mouse.current != null)
            {
                Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
                //Debug.Log(mouseScreenPos);
                float depthDist = Mathf.Abs(CoopCameraController.Camera.transform.position.z);
                Vector3 mouseWorldPos = CoopCameraController.Camera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, depthDist));
                //Debug.Log(mouseWorldPos);
                CurrentAimDir = ((Vector2)mouseWorldPos - (Vector2)transform.position).normalized;
                //Debug.Log(CurrentAimDir);
            }
        }

        if (CurrentAimDir == Vector2.zero)
            CurrentAimDir = Vector2.right;
    }

    private void FixedUpdate()
    {
        _activeMovement?.FixedTick();
        _activeJump?.FixedTick();

        foreach (CharacterAbility ability in _activeAbilities)
            ability.FixedTick();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        _activeMovement?.CharacterCollisionStay(collision);
        _activeJump?.CharacterCollisionStay(collision);
        foreach (CharacterAbility ability in _activeAbilities)
        {
            ability.CharacterCollisionStay(collision);
        }
    }

    private void CheckGrounded()
    {
        DebugVisual.DrawCircle(_groundCheck.position, _groundCheckRadius, Color.red, Time.deltaTime);
        int groundCount = EntityRegistry.GetAllEntitiesInRadius<Ground>(_groundCheck.position, _groundCheckRadius).Count();
        Debug.Log("Ground detected: " + groundCount);
        SetGrounded(groundCount != 0);
    }

    public void ForceSetGrounded(bool grounded)
    {
        IsGrounded = grounded;

        if (!grounded)
            LastGroundedTime = float.NegativeInfinity;
    }

    private void SetGrounded(bool grounded)
    {
        bool wasGrounded = IsGrounded;
        IsGrounded = grounded;

        if (!grounded)
            return;

        LastGroundedTime = Time.time;

        if (!wasGrounded)
            EventBus.Raise<OnCharacterTouchedGround>(ID);
    }

    private float ClampScreenMovement(float xVel)
    {
        if (CoopCameraController == null)
            return 0f;

        CameraBounds bounds = CoopCameraController.GetBounds();
        float posX = _rb.position.x;

        if ((posX <= bounds.left + bounds.margin && xVel < 0) || (posX >= bounds.right - bounds.margin && xVel > 0))
            xVel = 0;

        return xVel;
    }
    public void ApplyHVelocity(float xVel)
    {
        xVel = ClampScreenMovement(xVel);
        Vector2 vel = _rb.linearVelocity;
        vel.x = xVel;
        _rb.linearVelocity = vel;
    }

    public override void Dispose()
    {
        base.Dispose();
        _activeMovement?.Dispose();
        _activeJump?.Dispose();

        foreach (CharacterAbility ability in _activeAbilities)
        {
            ability.Dispose();
        }
    }
}

public struct OnCharacterTouchedGround : IEvent
{
    public uint characterID;

    public void Assign(params object[] parameters)
    {
        characterID = (uint)parameters[0];
    }

    public void Reset()
    {
        characterID = default(uint);
    }
}

public struct OnCharacterJumpPressed : IEvent
{
    public uint characterID;

    public void Assign(params object[] parameters)
    {
        characterID = (uint)parameters[0];
    }

    public void Reset()
    {
        characterID = default(uint);
    }
}

public struct OnCharacterJumpReleased : IEvent
{
    public uint characterID;

    public void Assign(params object[] parameters)
    {
        characterID = (uint)parameters[0];
    }

    public void Reset()
    {
        characterID = default(uint);
    }
}
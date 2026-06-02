using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Character : MonoBehaviour
{
    [Header("Ground checks")]
    [SerializeField] private float _coyoteTime = 0.12f;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask _groundLayer;

    private MovementAbility _activeMovement;
    private JumpAbility _activeJump;
    private List<CharacterAbility> _activeAbilities = new List<CharacterAbility>();

    private Rigidbody2D _rb;
    private Collider2D _ownCollider;
    private ICoopCameraService _camService;

    public Action TouchGroundEvent;
    public Action JumpPressedEvent;
    public Action JumpReleasedEvent;
    public bool IsGrounded { get; internal set; }
    public float LastGroundedTime { get; private set; }
    public float CoyoteTime => _coyoteTime;
    public bool IsIgnoringInput { get; set; }

    public Rigidbody2D Rb => _rb;

    public MovementAbility ActiveMovement => _activeMovement;
    public JumpAbility ActiveJump => _activeJump;
    public List<CharacterAbility> ActiveAbilities => _activeAbilities;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _ownCollider = GetComponent<Collider2D>();
    }

    protected virtual void Start()
    {
        _camService = ServiceProvider.Instance.ContainsService<ICoopCameraService>() ?
            ServiceProvider.Instance.GetService<ICoopCameraService>() :
            null;
    }

    public void EquipCharacter(CharacterDebugInfo info)
    {
        IsIgnoringInput = false;

        Debug.Log(_rb);

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
        {
            if (ability != null) 
                Destroy(ability);
        }         
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (IsIgnoringInput) 
            return;

        _activeMovement?.ProcessMove(context.ReadValue<Vector2>());

        foreach (CharacterAbility ability in _activeAbilities)
        {
            ability.ProcessMove(context.ReadValue<Vector2>());
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (IsIgnoringInput) 
            return;

        _activeJump?.ProcessJump(context);

        foreach (CharacterAbility ability in _activeAbilities)
        {
            ability.ProcessJump(context);
        }       
    }

    public void OnPrimaryAction(InputAction.CallbackContext context)
    {
        if (IsIgnoringInput) 
            return;

        foreach (CharacterAbility ability in _activeAbilities)
        {
            ability.ProcessAction(context);
        }      
    }

    public void OnSecondaryAction(InputAction.CallbackContext context)
    {
        if (IsIgnoringInput) 
            return;

        foreach (CharacterAbility ability in _activeAbilities)
        {
            ability.ProcessSkill(context);
        }         
    }

    public void OnSkillAction(InputAction.CallbackContext context)
    {
        if (IsIgnoringInput) 
            return;

        foreach (CharacterAbility ability in _activeAbilities)
        {
            ability.ProcessSkill(context);
        }
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        if (IsIgnoringInput) 
            return;

        Vector2 aim = context.ReadValue<Vector2>();

        foreach (CharacterAbility ability in _activeAbilities)
        {
            ability.ProcessAim(aim);
        }
    }

    private void Update()
    {
        CheckGrounded();

        _activeMovement?.Tick();
        _activeJump?.Tick();

        foreach (CharacterAbility ability in _activeAbilities)
        {
            ability.Tick();
        }
    }

    private void FixedUpdate()
    {
        _activeMovement?.FixedTick();
        _activeJump?.FixedTick();

        foreach (CharacterAbility ability in _activeAbilities)
        {
            ability.FixedTick();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        _activeMovement?.CharCollisionStay(collision);
        _activeJump?.CharCollisionStay(collision);

        foreach (CharacterAbility ability in _activeAbilities)
        {
            ability.CharCollisionStay(collision);
        }
    }

    private void CheckGrounded()
    {
        var hits = Physics2D.OverlapCircleAll(_groundCheck.position, _groundCheckRadius, _groundLayer);

        bool grounded = System.Array.Exists(hits, col => col != _ownCollider);

        SetGrounded(grounded);
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
            TouchGroundEvent?.Invoke();
    }

    private float ClampScreenMovement(float xVel)
    {
        CameraBounds bounds = _camService.GetBounds();
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
}

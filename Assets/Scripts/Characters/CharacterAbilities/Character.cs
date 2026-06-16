using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(PlayerInput))]
public class Character : MonoBehaviour
{
    [Header("Ground checks")]
    [SerializeField] private float _coyoteTime = 0.12f;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask _groundLayer;

    private Vector2 _rawAimInput;
    private bool _isOnGamepad;
    private MovementAbility _activeMovement;
    private JumpAbility _activeJump;
    private List<CharacterAbility> _activeAbilities = new();
    private ICoopCameraService _camService;
    private Rigidbody2D _rb;
    private Collider2D _ownCollider;
    private PlayerInput _playerInput;
    public Action TouchGroundEvent;
    public Action JumpPressedEvent;
    public Action JumpReleasedEvent;

    public bool IsGrounded { get; internal set; }
    public float LastGroundedTime { get; private set; }
    public float CoyoteTime => _coyoteTime;
    public bool IsIgnoringInput { get; set; }
    public bool IsBlockingRotation { get; set; }
    public bool IsBlockingJump { get; set; }
    public Vector2 CurrentAimDir { get; private set; }
    public Rigidbody2D Rb => _rb;
    public MovementAbility ActiveMovement => _activeMovement;
    public JumpAbility ActiveJump => _activeJump;
    public List<CharacterAbility> ActiveAbilities => _activeAbilities;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _ownCollider = GetComponent<Collider2D>();
        CurrentAimDir = Vector2.right;
        _playerInput = GetComponent<PlayerInput>();
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
        IsBlockingRotation = false;

        //Debug.Log(_rb);

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

    public void SetInputDevice(InputDevice device)
    {
        _isOnGamepad = device is Gamepad;
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

    public void OnAim(InputAction.CallbackContext context)
    {
        if (IsIgnoringInput || IsBlockingRotation)
            return;

        _rawAimInput = context.ReadValue<Vector2>();
        _isOnGamepad = context.control.device is Gamepad;

        foreach (CharacterAbility ability in _activeAbilities)
        {
            ability.ProcessAim(_rawAimInput);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (IsIgnoringInput)
            return;

        _isOnGamepad = context.control.device is Gamepad;
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

        _isOnGamepad = context.control.device is Gamepad;
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

    public void OnShield(InputAction.CallbackContext context)
    {
        if (IsIgnoringInput)
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

    public void OnSkillAction(InputAction.CallbackContext context)
    {
        if (IsIgnoringInput)
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
        {
            ability.Tick();
        }
    }

    private void CalculateAim()
    {
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
            if (Camera.main != null && Mouse.current != null)
            {
                Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
                //Debug.Log(mouseScreenPos);
                float depthDist = Mathf.Abs(Camera.main.transform.position.z);
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new(mouseScreenPos.x, mouseScreenPos.y, depthDist));
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
        Collider2D[] hits = Physics2D.OverlapCircleAll(_groundCheck.position, _groundCheckRadius, _groundLayer);
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
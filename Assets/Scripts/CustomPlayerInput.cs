using System;
using CustomInputClass;
using UnityEngine.InputSystem;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using Assets.Scripts.Entities;
using UnityEngine;

public struct UserRequestJumpEvent : IEvent
{
    uint entityID;

    public void Assign(params object[] parameters)
    {
        entityID = (uint)parameters[0];
    }

    public void Reset()
    {
        entityID = default(uint);
    }
}

public sealed class CustomPlayerInput : IInitiable, ITickable, IDisposable
{
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
    private ControllerMapping ControllerMapping => ServiceProvider.Instance.GetService<ControllerMapping>();
    private InputSystem_Actions EntityInput => ControllerMapping.GetPlayerInputMaps(entityID);

    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

    private Character ControllerCharacter => EntityRegistry.GetAs<Character>(entityID);

    private uint entityID = 0;
    public uint EntityID => entityID;

    public CustomPlayerInput(uint entityID)
    {
        this.entityID = entityID;
    }

    public void Init()
    {
        EntityInput.Constant.Pause.started += OnPause;

        EntityInput.Player.Jump.Enable();
        EntityInput.Player.PrimaryAction.Enable();
        EntityInput.Player.SecondaryAction.Enable();
        EntityInput.Player.SkillAction.Enable();

        EntityInput.Player.Jump.started += OnJump;
        EntityInput.Player.Jump.performed += OnJump;
        EntityInput.Player.Jump.canceled += OnJump;

        EntityInput.Player.PrimaryAction.started += OnPrimaryAction;

        EntityInput.Player.SecondaryAction.started += OnSecondaryAction;
        EntityInput.Player.SecondaryAction.performed += OnSecondaryAction;
        EntityInput.Player.SecondaryAction.canceled += OnSecondaryAction;

        EntityInput.Player.SkillAction.started += OnSkillAction;
        EntityInput.Player.SkillAction.performed += OnSkillAction;
        EntityInput.Player.SkillAction.canceled += OnSkillAction;
    }

    public void LateInit()
    {

    }

    public void Tick(float deltaTime)
    {
        ControllerCharacter.OnMove(EntityInput.Player.Move.ReadValue<Vector2>());
        ControllerCharacter.OnAim(EntityInput.Player.Look.ReadValue<Vector2>());
    }

    private void OnPause(InputAction.CallbackContext ctx)
    {
        EventBus.Raise<ToggleGamePauseStateEvent>();
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        ControllerCharacter.OnJump(ctx); //EventBus.Raise<UserRequestJumpEvent>(entityID);
    }

    private void OnSkillAction(InputAction.CallbackContext ctx)
    {
        ControllerCharacter.OnSkillAction(ctx);
    }

    private void OnPrimaryAction(InputAction.CallbackContext ctx)
    {
        ControllerCharacter.OnPrimaryAction(ctx);
    }

    private void OnSecondaryAction(InputAction.CallbackContext ctx)
    {
        ControllerCharacter.OnSecondaryAction(ctx);
    }

    public void Dispose()
    {
        EntityInput.Player.Jump.Disable();
        EntityInput.Player.PrimaryAction.Disable();
        EntityInput.Player.SecondaryAction.Disable();
        EntityInput.Player.SkillAction.Disable();

        EntityInput.Constant.Pause.started -= OnPause;

        EntityInput.Player.Jump.started -= OnJump;
        EntityInput.Player.Jump.performed -= OnJump;
        EntityInput.Player.Jump.canceled -= OnJump;

        EntityInput.Player.PrimaryAction.started -= OnPrimaryAction;

        EntityInput.Player.SecondaryAction.started -= OnSecondaryAction;
        EntityInput.Player.SecondaryAction.performed -= OnSecondaryAction;
        EntityInput.Player.SecondaryAction.canceled -= OnSecondaryAction;

        EntityInput.Player.SkillAction.started += OnSkillAction;
        EntityInput.Player.SkillAction.performed += OnSkillAction;
        EntityInput.Player.SkillAction.canceled += OnSkillAction;
    }
}
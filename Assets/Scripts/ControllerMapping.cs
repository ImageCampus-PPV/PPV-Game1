using Assets.Scripts.Entities;
using CustomInputClass;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using Pawgineers.Gameplay.Inputs;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public sealed class ControllerMapping : IService, IInitiable, ITickable
{
    public bool IsPersistance => false;

    public const int MAX_PLAYERS_LIMIT = 2;
    public const int MinPlayerToStartGame = 1;

    InputSystem_Actions inputActions;

    EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
    EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

    private Dictionary<InputDevice, uint> deviceToPlayerMapping;
    private Dictionary<uint, InputSystem_Actions> playerToInputActionMap;

    private InputDevice pendingDeviceToMap;
    public InputDevice PendingDeviceToMap => pendingDeviceToMap;

    public Dictionary<uint, InputUser> playerToInputUser;

    private Dictionary<uint, CustomPlayerInput> playerToCustomPlayerInput;
    public Dictionary<uint, CustomPlayerInput> PlayerToCustomPlayerInput => playerToCustomPlayerInput;

    public void Init()
    {
        inputActions = new InputSystem_Actions();

        inputActions.Enable();

        inputActions.Constant.JoinGame.Enable();
        inputActions.Constant.JoinGame.performed += OnJoinPlayerAction;

        InputSystem.onDeviceChange += OnDeviceChange;

        playerToInputActionMap = new Dictionary<uint, InputSystem_Actions>();
        playerToCustomPlayerInput = new Dictionary<uint, CustomPlayerInput>();
        playerToInputUser = new Dictionary<uint, InputUser>();
        deviceToPlayerMapping = new Dictionary<InputDevice, uint>();
    }

    public void LateInit()
    {

    }

    public void Tick(float deltaTime)
    {
        foreach (CustomPlayerInput customPlayerController in playerToCustomPlayerInput.Values)
            customPlayerController.Tick(deltaTime);
    }

    private void TryMapDevice(InputDevice pendingDeviceToMap, uint entityID)
    {
        if (pendingDeviceToMap == null)
            throw new NullReferenceException($"{nameof(InputDevice)} is NOT set to an instance of an object.");

        if (playerToCustomPlayerInput.ContainsKey(entityID))
            throw new Exception("The playerID is already registered to a device.");

        RegisterPlayer(pendingDeviceToMap, entityID);

        playerToCustomPlayerInput.Add(entityID, new CustomPlayerInput(entityID));

        playerToCustomPlayerInput[entityID].Init();
        playerToCustomPlayerInput[entityID].LateInit();

        pendingDeviceToMap = null;
    }

    private void RegisterPlayer(InputDevice device, uint entityID)
    {
        if (device == null)
            throw new NullReferenceException("Device null");

        deviceToPlayerMapping.Add(device, entityID);

        InputSystem_Actions newInputActionMap = new();

        InputUser user = InputUser.CreateUserWithoutPairedDevices();
        InputUser.PerformPairingWithDevice(device, user);

        user.AssociateActionsWithUser(newInputActionMap);
        newInputActionMap.Enable();

        newInputActionMap.Constant.JoinGame.Disable();

        playerToInputUser.Add(entityID, user);
        playerToInputActionMap.Add(entityID, newInputActionMap);
    }

    private void OnJoinPlayerAction(InputAction.CallbackContext context)
    {
        pendingDeviceToMap = context.control.device;

        if (deviceToPlayerMapping.ContainsKey(pendingDeviceToMap))
            return;

        if (playerToInputUser.Count >= MAX_PLAYERS_LIMIT)
            Debug.Log($"Player limit reached. Current players: {deviceToPlayerMapping.Count}");

        foreach (Character character in EntityRegistry.FilterEntities<Character>())
        {
            if (playerToInputUser.ContainsKey(character.ID))
                continue;

            TryMapDevice(pendingDeviceToMap, character.ID);
            break;
        }

        if (!deviceToPlayerMapping.ContainsKey(pendingDeviceToMap))
        {
            if (EntityRegistry.GetEntityCount<Character>() == MinPlayerToStartGame)
            {
                //TODO: Add Event to start game or instead ignore the event and add a property that return bool if deviceToPlayerMapping.Count() >= MinPlayerToStartGame || EntityRegistry.GetEntityCount<Character>() >= MinPlayerToStartGame
                //EventBus.Raise<GirdStartTickingEvent>();
            }

        }
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange inputDeviceState)
    {
        if (inputDeviceState == InputDeviceChange.Added)
        {

        }
        if (inputDeviceState == InputDeviceChange.Disconnected)
        {
            foreach (KeyValuePair<InputDevice, uint> deviceToPlayer in deviceToPlayerMapping)
            {
                if (deviceToPlayer.Key == device)
                {
                    //Todo maybe do a timer for delete if theres a timeout
                    if (ServiceProvider.Instance.ContainsService<EventBus>())
                        EventBus.Raise<InputDeviceDisconnectRequestEvent>(deviceToPlayer.Key);

                    deviceToPlayerMapping.Remove(device);
                    break;
                }
            }
        }
    }

    public bool IsDeviceConnectedToPlayer(InputDevice inputDevice, uint entityId)
    {
        if (inputDevice == null)
            throw new NullReferenceException("Input Device is null");

        if (!deviceToPlayerMapping.ContainsKey(inputDevice))
            throw new Exception($"Device not registered");

        return deviceToPlayerMapping[inputDevice] == entityId;
    }

    public InputSystem_Actions GetPlayerInputMaps(uint entityID)
    {
        return playerToInputActionMap[entityID];
    }

    public void Dispose()
    {
        inputActions.Player.Jump.performed -= OnJoinPlayerAction;
        inputActions.Player.Jump.Disable();
        inputActions.Player.Jump.Dispose();

        InputSystem.onDeviceChange -= OnDeviceChange;

        foreach (CustomPlayerInput keys in playerToCustomPlayerInput.Values)
        {
            keys.Dispose();
        }
        playerToCustomPlayerInput.Clear();

        foreach (InputSystem_Actions inputActionMap in playerToInputActionMap.Values)
        {
            inputActionMap.Disable();
            inputActionMap.Dispose();
        }
        playerToInputActionMap.Clear();
    }
}
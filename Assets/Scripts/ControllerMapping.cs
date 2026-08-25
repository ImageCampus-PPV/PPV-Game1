using Assets.Scripts.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace Pawgineers.Gameplay.Inputs
{
    [Serializable]
    public sealed class ControllerMapping : IService, IDisposable
    {
        private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

        [SerializeField] private InputActionAsset inputActionAsset;
        private Dictionary<InputDevice, uint> deviceToPlayerMapping;

        private Dictionary<uint, InputActionAsset> playerToInputActionAsset;

        private InputDevice pendingDeviceToMap;

        public Dictionary<uint, InputUser> playerToInput;

        public bool IsPersistance => true;

        public const int MinPlayerToStartGame = 1;
        public void Init()
        {
            inputActionAsset.Enable();
  
            InputSystem.onDeviceChange += OnDeviceChange;

            EventBus.Subscribe<DeviceMappingRequestEvent>(TryMapDevice);

            playerToInputActionAsset = new Dictionary<uint, InputActionAsset>();
            playerToInput = new Dictionary<uint, InputUser>();
            deviceToPlayerMapping = new Dictionary<InputDevice, uint>();
        }

        private void TryMapDevice(in DeviceMappingRequestEvent deviceMappingRequestEvent)
        {
            if (pendingDeviceToMap != null)
            {
                if (!playerToGridInput.ContainsKey(deviceMappingRequestEvent.playerId) &&
                    !playerToWorkshopInput.ContainsKey(deviceMappingRequestEvent.playerId))
                {
                    RegisterPlayer(pendingDeviceToMap, deviceMappingRequestEvent.playerId);

                    playerToGridInput.Add(deviceMappingRequestEvent.playerId,
                        new PlayerGridControllerInput(deviceMappingRequestEvent.playerId));
                    playerToGridInput[deviceMappingRequestEvent.playerId].ConfigInputBindings();
                    playerToWorkshopInput.Add(deviceMappingRequestEvent.playerId,
                        new PlayerWorkshopControllerInput(deviceMappingRequestEvent.playerId));
                    playerToWorkshopInput[deviceMappingRequestEvent.playerId].ConfigInputBindings();
                    pendingDeviceToMap = null;
                }
                else
                {
                    throw new Exception("The playerID is already registered to a device.");
                }
            }
            else
            {
                if (!playerToGridInput.ContainsKey(deviceMappingRequestEvent.playerId) &&
                    !playerToWorkshopInput.ContainsKey(deviceMappingRequestEvent.playerId))
                {
                    playerToGridInput.Add(deviceMappingRequestEvent.playerId,
                        new PlayerGridNetworkInput(deviceMappingRequestEvent.playerId));
                    playerToWorkshopInput.Add(deviceMappingRequestEvent.playerId,
                        new PlayerWorkshopNetworkInput(deviceMappingRequestEvent.playerId));
                }
                else
                {
                    throw new Exception("The playerID is already registered");
                }
            }
        }

        private void RegisterPlayer(InputDevice device, uint playerId)
        {
            if (device == null)
                throw new NullReferenceException("Device null");

            if (!deviceToPlayerMapping.TryAdd(device, playerId))
                throw new Exception($"Device already assigned to entity: {deviceToPlayerMapping[device]}");

            InputActionAsset assetClone = UnityEngine.Object.Instantiate(inputActionAsset);

            InputUser user = InputUser.CreateUserWithoutPairedDevices();
            InputUser.PerformPairingWithDevice(device, user);

            user.AssociateActionsWithUser(assetClone);
            assetClone.Enable();

            playerToInput.Add(playerId, user);
            playerToInputActionAsset.Add(playerId, assetClone);
        }

        private void OnJoinPlayerAction(InputAction.CallbackContext context)
        {
            pendingDeviceToMap = context.control.device;

            if (EntityRegistry.GetEntityCount<Player>() >= 2/*GameplayEntityRegistry.MAX_PLAYERS_LIMIT*/)
            {
                Debug.Log($"Player limit reached. Current players: {deviceToPlayerMapping.Count}");
            }
            else if (!deviceToPlayerMapping.ContainsKey(pendingDeviceToMap))
            {
                EventBus.Raise<InputDeviceRegisterControllerEvent>(pendingDeviceToMap);
                if (EntityRegistry.GetEntityCount<Player>() == MinPlayerToStartGame)
                {
                    EventBus.Raise<Pawgineers.Architecture.Entities.Events.GirdStartTickingEvent>();
                }

            }
        }

        private void OnPressPauseAction(InputAction.CallbackContext context)
        {
            pendingDeviceToMap = context.control.device;
            if (deviceToPlayerMapping.ContainsKey(pendingDeviceToMap))
            {
                EventBus.Raise<ToggleGamePauseStateEvent>();
            }
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange inputDeviceState)
        {
            if (inputDeviceState == InputDeviceChange.Disconnected)
            {
                foreach (KeyValuePair<InputDevice, uint> deviceToPlayer in deviceToPlayerMapping)
                {
                    if (deviceToPlayer.Key == device)
                    {
                        //Todo maybe do a timer for delete if theres a timeout
                        //EventBus.Raise<InputDeviceDisconnectRequestEvent>(deviceToPlayer.Key);
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

        public InputActionMap GetPlayerInputMaps(uint playerId, int mapIndex)
        {
            return playerToInputActionAsset[playerId].actionMaps[mapIndex];
        }

        public void Dispose()
        {
            controllerConfiguration.JoinPlayerAction.action.performed -= OnJoinPlayerAction;
            controllerConfiguration.JoinPlayerAction.action.Disable();
            controllerConfiguration.JoinPlayerAction.action.Dispose();
            controllerConfiguration.PausePlayerAction.action.performed -= OnPressPauseAction;
            controllerConfiguration.PausePlayerAction.action.Disable();
            controllerConfiguration.PausePlayerAction.action.Dispose();
            InputSystem.onDeviceChange -= OnDeviceChange;
            EventBus.Unsubscribe<DeviceMappingRequestEvent>(TryMapDevice);

            foreach (uint keys in playerToInputActionAsset.Keys)
            {
                playerToInputActionAsset[keys].Disable();
                UnityEngine.Object.Destroy(playerToInputActionAsset[keys]);
            }

            playerToInputActionAsset.Clear();
        }
    }

}
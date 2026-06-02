using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

public class GameStarter : MonoBehaviour
{
    [Header("Assignment")]
    [SerializeField] private PlayerAssignment _assignment;

    [Header("Characters")]
    [SerializeField] private Character _dragonCharacter;
    [SerializeField] private Character _mechaCharacter;

    [Header("CharacterDebugInfo")]
    [SerializeField] private CharacterDebugInfo _dragonInfo;
    [SerializeField] private CharacterDebugInfo _mechaInfo;

    private void Start()
    {
        EquipAbilities();
        AssignDevices();
    }

    private void EquipAbilities()
    {
        ApplyCharacterInfo(_dragonCharacter, _dragonInfo);
        ApplyCharacterInfo(_mechaCharacter, _mechaInfo);
    }

    private void ApplyCharacterInfo(Character character, CharacterDebugInfo info)
    {
        character.EquipCharacter(info);

        if (character.TryGetComponent<CharacterDebugger>(out var debugger))
        {
            debugger.DebugInfo = info;
            debugger.UpdateInfo();
        }
    }

    private void AssignDevices()
    {
        AssignDevice(_dragonCharacter, PlayerAssignment.DragonDevice);
        AssignDevice(_mechaCharacter, PlayerAssignment.MechaDevice);
    }

    private void AssignDevice(Character character, InputDevice device)
    {
        PlayerInput playerInput = character.GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError($"[GameStarter] {character.name} no PlayerInput detected.");
            return;
        }

        playerInput.enabled = true;

        if (device == null)
        {
            Debug.LogWarning($"[GameStarter] No controller detected for {character.name}.");
            return;
        }

        playerInput.user.UnpairDevices();
        InputUser.PerformPairingWithDevice(device, playerInput.user);

        string controlScheme = (device is Gamepad) ? "Gamepad" : "Keyboard&Mouse";

        playerInput.SwitchCurrentControlScheme(controlScheme, device);

        Debug.Log($"[GameStarter] {character.name} → {device.displayName}");
    }
}


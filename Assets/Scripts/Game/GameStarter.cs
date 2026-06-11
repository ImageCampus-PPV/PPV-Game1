using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

[DefaultExecutionOrder(100)]
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
        AssignDevice(_dragonCharacter, _assignment.GetDragonDevice());
        AssignDevice(_mechaCharacter, _assignment.GetMechaDevice());
    }

    private void AssignDevice(Character character, InputDevice device)
    {
        PlayerInput playerInput = character.GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError($"[GameStarter] {character.name} no tiene PlayerInput.");
            return;
        }

        playerInput.enabled = true;

        if (device == null)
        {
            Debug.LogWarning($"[GameStarter] No hay dispositivo asignado para {character.name}. Usando default.");
            return;
        }

        if (device is Keyboard)
        {
            InputUser.PerformPairingWithDevice(device, playerInput.user);

            if (Mouse.current != null)
                InputUser.PerformPairingWithDevice(Mouse.current, playerInput.user);

            playerInput.SwitchCurrentControlScheme("Keyboard&Mouse", device, Mouse.current);
        }
        else
        {
            playerInput.SwitchCurrentControlScheme(device);
        }
        character.SetInputDevice(device);

        //Debug.Log($"[GameStarter] {character.name} → {device.displayName} (gamepad: {device is Gamepad})");
    }
}

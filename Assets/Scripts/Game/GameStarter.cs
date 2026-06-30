using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
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

    [Header("Deposit")]
    [SerializeField] private DepositData _depositData;
    [SerializeField] private int[] _initialEquippedWeaponSlots = new int[] { 0, 2 };

    private void Start()
    {
        EquipAbilities();
        InitDeposit();
        StartCoroutine(AssignDevicesCoroutine());
    }

    private void InitDeposit()
    {
        if (_depositData == null) return;
        //_depositData.InitWeapons(_initialEquippedWeaponSlots);
    }

    private IEnumerator AssignDevicesCoroutine()
    {
        yield return null;
        yield return null;

        AssignDevice(_dragonCharacter, _assignment.GetDragonDevice());
        AssignDevice(_mechaCharacter, _assignment.GetMechaDevice());
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

    private void AssignDevice(Character character, InputDevice device)
    {
        PlayerInput playerInput = character.GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError($"[GameStarter] {character.name} no tiene PlayerInput.");
            return;
        }

        if (device == null)
        {
            Debug.LogWarning($"[GameStarter] No hay dispositivo para {character.name}.");
            playerInput.enabled = true;
            return;
        }

        playerInput.enabled = false;

        if (device is Keyboard)
        {
            playerInput.defaultControlScheme = "Keyboard&Mouse";
            playerInput.defaultActionMap = null;
        }
        else if (device is Gamepad)
        {
            playerInput.defaultControlScheme = "Gamepad";
        }

        playerInput.enabled = true;

        StartCoroutine(SwitchSchemeNextFrame(playerInput, character, device));
    }

    private IEnumerator SwitchSchemeNextFrame(PlayerInput playerInput, Character character, InputDevice device)
    {
        yield return null;

        if (!playerInput.user.valid)
        {
            Debug.LogError($"[GameStarter] Sigue siendo invalido para {character.name}. Revisar configuracion del PlayerInput.");
            yield return null;
        }

        if (device is Keyboard)
        {
            if (Mouse.current != null)
                playerInput.SwitchCurrentControlScheme("Keyboard&Mouse", device, Mouse.current);
            else
                playerInput.SwitchCurrentControlScheme("Keyboard&Mouse", device);
        }
        else
        {
            playerInput.SwitchCurrentControlScheme(device);
        }

        character.SetInputDevice(device);
        Debug.Log($"[GameStarter] {character.name} → {device.displayName}");
    }
}

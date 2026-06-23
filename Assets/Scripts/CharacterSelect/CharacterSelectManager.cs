using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem.Utilities;

public class CharacterSelectManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private PlayerAssignment _assignment;
    [SerializeField] private string _gameSceneName = "Game";

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _dragonStatusText;
    [SerializeField] private TextMeshProUGUI _mechaStatusText;
    [SerializeField] private TextMeshProUGUI _instructionsText;
    [SerializeField] private GameObject _startPrompt;

    private void OnEnable()
    {
        _assignment.Reset();

        InputSystem.onAnyButtonPress.CallOnce(OnAnyButton);

        RefreshUI();
    }

    private void OnDisable()
    {
        InputSystem.onAnyButtonPress.CallOnce(_ => { });
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
            ActivateKeyboardDebugMode();
    }

    private void ActivateKeyboardDebugMode()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            Debug.LogWarning("[CharacterSelect] No se encontro teclado.");
            return;
        }

        _assignment.DragonDeviceId = keyboard.deviceId;
        _assignment.MechaDeviceId = keyboard.deviceId;

        Debug.Log("[CharacterSelect] Modo debug activado: ambos jugadores usan el teclado.");

        RefreshUI();
        SceneManager.LoadScene(_gameSceneName);
    }

    private void OnAnyButton(InputControl control)
    {
        InputDevice device = control.device;

        if (control.name == "escape")
            return;

        HandleDeviceInput(device, control);

        if (!_assignment.BothAssigned)
            InputSystem.onAnyButtonPress.CallOnce(OnAnyButton);
    }

    private void HandleDeviceInput(InputDevice device, InputControl control)
    {
        int deviceId = device.deviceId;

        if (_assignment.DragonDeviceId == deviceId || _assignment.MechaDeviceId == deviceId)
        {
            if (control.name is "buttonSouth" or "space")
            {
                CancelAssignment(deviceId);
                RefreshUI();
                InputSystem.onAnyButtonPress.CallOnce(OnAnyButton);
            }
            return;
        }

        if (control.name is "buttonWest" or "q")
        {
            if (_assignment.DragonDeviceId < 0)
                _assignment.DragonDeviceId = deviceId;
        }
        else if (control.name is "buttonEast" or "e")
        {
            if (_assignment.MechaDeviceId < 0)
                _assignment.MechaDeviceId = deviceId;
        }

        RefreshUI();

        if (_assignment.BothAssigned)
            ShowStartPrompt();
    }

    private void CancelAssignment(int deviceId)
    {
        if (_assignment.DragonDeviceId == deviceId)
        {
            _assignment.DragonDeviceId = -1;
            Debug.Log("[CharacterSelect] Dragon deseleccionado.");
        }
        else if (_assignment.MechaDeviceId == deviceId)
        {
            _assignment.MechaDeviceId = -1;
            Debug.Log("[CharacterSelect] Mecha deseleccionado.");
        }
    }

    public void StartGame()
    {
        if (!_assignment.BothAssigned)
        {
            Debug.Log("[CharacterSelect] Faltan asignar dispositivos.");
            return;
        }
        SceneManager.LoadScene(_gameSceneName);
    }

    public void OnStartPressed(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        StartGame();
    }

    private void ShowStartPrompt()
    {
        if (_startPrompt != null)
            _startPrompt.SetActive(true);

        if (_instructionsText != null)
            _instructionsText.gameObject.SetActive(false);
    }

    private void RefreshUI()
    {
        if (_dragonStatusText != null)
        {
            _dragonStatusText.text = _assignment.DragonDeviceId >= 0
                ? $"Dragon\n<color=green>{GetDeviceName(_assignment.DragonDeviceId)}</color>"
                : "Dragon\n<color=grey>Sin asignar\n(presioná Q)</color>";
        }

        if (_mechaStatusText != null)
        {
            _mechaStatusText.text = _assignment.MechaDeviceId >= 0
                ? $"Mecha\n<color=green>{GetDeviceName(_assignment.MechaDeviceId)}</color>"
                : "Mecha\n<color=grey>Sin asignar\n(presioná E)</color>";
        }

        if (_startPrompt != null && !_assignment.BothAssigned)
            _startPrompt.SetActive(false);

        if (_instructionsText != null && !_assignment.BothAssigned)
            _instructionsText.gameObject.SetActive(true);
    }

    private string GetDeviceName(int deviceId)
    {
        var device = InputSystem.GetDeviceById(deviceId);

        return device != null ? device.displayName : "Desconocido";
    }
}

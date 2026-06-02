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

    private void OnAnyButton(InputControl control)
    {
        InputDevice device = control.device;

        if (control.name == "escape") 
            return;

        HandleDeviceInput(device, control);

        if (!_assignment.BothAssigned)
            InputSystem.onAnyButtonPress.CallOnce(OnAnyButton);
    }

    private int GetDeviceIndex(InputDevice device)
    {
        ReadOnlyArray<InputDevice> devices = InputSystem.devices;

        for (int i = 0; i < devices.Count; i++)
        {
            if (devices[i] == device) 
                return i;
        }
            
        return -1;
    }

    private void HandleDeviceInput(InputDevice device, InputControl control)
    {
        bool cancelled = control.name is "buttonSouth" or "space" or "button1";
        bool left = control.name is "buttonWest" or "q" or "button3";
        bool right = control.name is "buttonEast" or "e" or "button2";

        if (PlayerAssignment.DragonDevice == device || PlayerAssignment.MechaDevice == device)
        {
            if (cancelled)
            {
                CancelAssignment(device);
                RefreshUI();
                InputSystem.onAnyButtonPress.CallOnce(OnAnyButton);
            }

            return;
        }

        if (left)
        {
            if (PlayerAssignment.DragonDevice == null)
            {
                PlayerAssignment.DragonDevice = device;

                Debug.Log($"[CharacterSelect] {device.displayName} assigned to Dragon");
            }
        }
        else if (right)
        {
            if (PlayerAssignment.MechaDevice == null)
            {
                PlayerAssignment.MechaDevice = device;
                Debug.Log($"[CharacterSelect] {device.displayName} assigned to Mecha");
            }
        }

        RefreshUI();

        if (_assignment.BothAssigned)
            ShowStartPrompt();
    }

    private void CancelAssignment(InputDevice device)
    {
        if (PlayerAssignment.DragonDevice == device)
        {
            PlayerAssignment.DragonDevice = null;
            Debug.Log("[CharacterSelect] Dragon unselected.");
        }
        else if (PlayerAssignment.MechaDevice == device)
        {
            PlayerAssignment.MechaDevice = null;
            Debug.Log("[CharacterSelect] Mecha unselected.");
        }
    }

    public void StartGame()
    {
        if (!_assignment.BothAssigned)
        {
            Debug.Log("[CharacterSelect] Unassigned characters.");
            return;
        }

        SceneManager.LoadScene(_gameSceneName);
    }

    public void OnStartPressed(InputAction.CallbackContext context)
    {
        if (!context.performed) 
            return;

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
            _dragonStatusText.text = PlayerAssignment.DragonDevice != null
                ? $"Dragon\n<color=green>{PlayerAssignment.DragonDevice.displayName}</color>"
                : "Dragon\n<color=grey>Unassigned\n(Left button / Q)</color>";
        }

        if (_mechaStatusText != null)
        {
            _mechaStatusText.text = PlayerAssignment.MechaDevice != null
                ? $"Mecha\n<color=green>{PlayerAssignment.MechaDevice.displayName}</color>"
                : "Mecha\n<color=grey>Unassigned\n(Right button / E)</color>";
        }

        if (_startPrompt != null && !_assignment.BothAssigned)
            _startPrompt.SetActive(false);

        if (_instructionsText != null && !_assignment.BothAssigned)
            _instructionsText.gameObject.SetActive(true);
    }
}

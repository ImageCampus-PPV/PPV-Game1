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
        int deviceIndex = GetDeviceIndex(device);

        if (_assignment.DragonDeviceIndex == deviceIndex || _assignment.MechaDeviceIndex == deviceIndex)
        {
            if (control.name is "buttonSouth" or "space")
            {
                CancelAssignment(deviceIndex);
                RefreshUI();
                InputSystem.onAnyButtonPress.CallOnce(OnAnyButton);
            }

            return;
        }

        if (control.name is "buttonWest" or "q")
        {
            if (_assignment.DragonDeviceIndex < 0)
            {
                _assignment.DragonDeviceIndex = deviceIndex;

                Debug.Log($"[CharacterSelect] {device.displayName} assigned to Dragon");
            }
        }
        else if (control.name is "buttonEast" or "e")
        {
            if (_assignment.MechaDeviceIndex < 0)
            {
                _assignment.MechaDeviceIndex = deviceIndex;
                Debug.Log($"[CharacterSelect] {device.displayName} assigned to Mecha");
            }
        }

        RefreshUI();

        if (_assignment.BothAssigned)
            ShowStartPrompt();
    }

    private void CancelAssignment(int deviceIndex)
    {
        if (_assignment.DragonDeviceIndex == deviceIndex)
        {
            _assignment.DragonDeviceIndex = -1;
            Debug.Log("[CharacterSelect] Dragon unselected.");
        }
        else if (_assignment.MechaDeviceIndex == deviceIndex)
        {
            _assignment.MechaDeviceIndex = -1;
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
            _dragonStatusText.text = _assignment.DragonDeviceIndex >= 0
                ? $"Dragon\n<color=green>{GetDeviceName(_assignment.DragonDeviceIndex)}</color>"
                : "Dragon\n<color=grey>Unassigned\n(Left button / Q)</color>";
        }

        if (_mechaStatusText != null)
        {
            _mechaStatusText.text = _assignment.MechaDeviceIndex >= 0
                ? $"Mecha\n<color=green>{GetDeviceName(_assignment.MechaDeviceIndex)}</color>"
                : "Mecha\n<color=grey>Unassigned\n(Right button / E)</color>";
        }

        if (_startPrompt != null && !_assignment.BothAssigned)
            _startPrompt.SetActive(false);

        if (_instructionsText != null && !_assignment.BothAssigned)
            _instructionsText.gameObject.SetActive(true);
    }

    private string GetDeviceName(int index)
    {
        ReadOnlyArray<InputDevice> devices = InputSystem.devices;

        if (index < 0 || index >= devices.Count) 
            return "Unknown";

        return devices[index].displayName;
    }
}

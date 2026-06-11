using UnityEngine;
using UnityEngine.InputSystem;


[DefaultExecutionOrder(-100)]
public class CharacterTypeSwitcher : MonoBehaviour
{
    [SerializeField] private CharacterDebugInfo[] _charactersToSwitch;
    [SerializeField] private CharacterDebugger _characterDebugger;

    private int _currentIndex;

    private void Start()
    {
        if (_charactersToSwitch.Length > 0)
        {
            _currentIndex = 0;
            UpdateDebugInfo();
        }
    }

    public void OnCycleCharacter(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        _currentIndex = (_currentIndex + 1) % _charactersToSwitch.Length;
        UpdateDebugInfo();
    }

    private void UpdateDebugInfo()
    {
        _characterDebugger.DebugInfo = _charactersToSwitch[_currentIndex];
        _characterDebugger.UpdateInfo();
    }
}

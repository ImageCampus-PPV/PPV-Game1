using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemTypeSwitcher : MonoBehaviour
{
    [SerializeField] private ItemFilter[] _filtersToTest;
    [SerializeField] private TextMeshProUGUI _statusText;

    private ItemCollector _collector;
    private int _currentIndex;

    private void Awake()
    {
        _collector = GetComponent<ItemCollector>();
        if (_filtersToTest.Length > 0)
            _collector.Filter = _filtersToTest[0];
        UpdateStatusText();

    }

    public void OnCycleFilter(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        _currentIndex = (_currentIndex + 1) % _filtersToTest.Length;
        _collector.Filter = _filtersToTest[_currentIndex];
        UpdateStatusText();
    }

    private void UpdateStatusText()
    {
        if (_statusText != null)
            _statusText.text = $"Filter: {_collector.Filter.name}";
    }
}

using UnityEngine;
using UnityEngine.InputSystem;


public class Deposit : MonoBehaviour
{
    [SerializeField] private DepositData _depositData;
    [SerializeField] private string _collectActionName = "Collect";
    [SerializeField] private string _cancelActionName = "Cancel";
    [SerializeField] private ItemPickupPrompt _prompt;
    [SerializeField] private DepositUI _depositUI;

    private ItemCollector _playerInRange;
    private PlayerInput _playerInput;
    private bool _uiOpen;

    private void Awake()
    {
        _prompt?.Hide();

        if (_depositUI != null)
            _depositUI.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_playerInRange != null) return;
        if (!other.TryGetComponent<ItemCollector>(out var collector)) return;
        if (!other.TryGetComponent<PlayerInput>(out var input)) return;

        _playerInRange = collector;
        _playerInput = input;

        SubscribeInput();
        UpdatePrompt();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.TryGetComponent<ItemCollector>(out var collector)) return;
        if (collector != _playerInRange) return;

        UnsubscribeInput();
        _prompt?.Hide();
        CloseUI();

        _playerInRange = null;
        _playerInput = null;
    }

    private void SubscribeInput()
    {
        if (_playerInput == null) return;

        var collectAction = _playerInput.actions.FindAction(_collectActionName);
        if (collectAction != null)
            collectAction.performed += OnInteract;

        var cancelAction = _playerInput.actions.FindAction(_cancelActionName);
        if (cancelAction != null)
            cancelAction.performed += OnCancel;
    }

    private void UnsubscribeInput()
    {
        if (_playerInput == null) return;

        var collectAction = _playerInput.actions.FindAction(_collectActionName);
        if (collectAction != null)
            collectAction.performed -= OnInteract;

        var cancelAction = _playerInput.actions.FindAction(_cancelActionName);
        if (cancelAction != null)
            cancelAction.performed -= OnCancel;
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (_uiOpen)
        {
            CloseUI();
            return;
        }

        var inventory = ImageCampus.ToolBox.Services.ServiceProvider.Instance.ContainsService<MyInventory>()
            ? ImageCampus.ToolBox.Services.ServiceProvider.Instance.GetService<MyInventory>()
            : null;

        if (inventory != null && inventory.IsFull)
        {
            Item item = inventory.TakeItem();
            if (item == null) return;

            _depositData.Deposit(item.Type);
            Destroy(item.gameObject);

            UpdatePrompt();
        }
        else
        {
            OpenUI();
        }
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        if (_uiOpen)
            CloseUI();
    }

    private void OpenUI()
    {
        if (_depositUI == null) return;

        _uiOpen = true;
        _depositUI.gameObject.SetActive(true);
        _prompt?.Hide();

        Time.timeScale = 0f;
        BlockPlayerInput(true);
    }

    private void CloseUI()
    {
        if (!_uiOpen) return;

        _uiOpen = false;
        _depositUI?.gameObject.SetActive(false);

        Time.timeScale = 1f;
        BlockPlayerInput(false);

        UpdatePrompt();
    }

    private void BlockPlayerInput(bool block)
    {
        foreach (var character in FindObjectsByType<Character>(FindObjectsSortMode.None))
            character.IsIgnoringInput = block;
    }

    private void UpdatePrompt()
    {
        if (_uiOpen) return;

        var inventory = ImageCampus.ToolBox.Services.ServiceProvider.Instance.ContainsService<MyInventory>()
            ? ImageCampus.ToolBox.Services.ServiceProvider.Instance.GetService<MyInventory>()
            : null;

        string buttonName = GetActionButtonName();

        if (inventory != null && inventory.IsFull)
            _prompt?.Show(buttonName, "Depositar");
        else
            _prompt?.Show(buttonName, "Ver Depósito");
    }

    private string GetActionButtonName()
    {
        if (_playerInput == null) return "?";

        var action = _playerInput.actions.FindAction(_collectActionName);
        if (action == null) return "?";

        string scheme = _playerInput.currentControlScheme;
        foreach (var binding in action.bindings)
        {
            if (binding.isComposite || binding.isPartOfComposite) continue;
            if (!string.IsNullOrEmpty(scheme) && !binding.groups.Contains(scheme)) continue;

            string display = InputControlPath.ToHumanReadableString(
                binding.effectivePath,
                InputControlPath.HumanReadableStringOptions.UseShortNames);

            if (!string.IsNullOrEmpty(display))
                return display;
        }

        return "?";
    }
}

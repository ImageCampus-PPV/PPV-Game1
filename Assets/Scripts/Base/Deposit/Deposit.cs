using UnityEngine;
using UnityEngine.InputSystem;


public class Deposit : MonoBehaviour
{
    [SerializeField] private DepositData _depositData;
    [SerializeField] private string _collectActionName = "Collect";
    [SerializeField] private ItemPickupPrompt _prompt;

    private ItemCollector _playerInRange;
    private PlayerInput _playerInput;

    private void Awake()
    {
        _prompt?.Hide();
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

        _playerInRange = null;
        _playerInput = null;
    }

    private void SubscribeInput()
    {
        if (_playerInput == null) return;
        var action = _playerInput.actions.FindAction(_collectActionName);
        if (action != null)
            action.performed += OnInteract;
    }

    private void UnsubscribeInput()
    {
        if (_playerInput == null) return;
        var action = _playerInput.actions.FindAction(_collectActionName);
        if (action != null)
            action.performed -= OnInteract;
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        var inventory = ImageCampus.ToolBox.Services.ServiceProvider.Instance.GetService<MyInventory>();
        if (inventory == null || !inventory.IsFull) return;

        Item item = inventory.TakeItem();
        if (item == null) return;

        _depositData.Deposit(item.Type);

        Destroy(item.gameObject);

        UpdatePrompt();
    }

    private void UpdatePrompt()
    {
        var inventory = ImageCampus.ToolBox.Services.ServiceProvider.Instance.ContainsService<MyInventory>()
            ? ImageCampus.ToolBox.Services.ServiceProvider.Instance.GetService<MyInventory>()
            : null;

        if (inventory != null && inventory.IsFull)
            _prompt?.Show(GetActionButtonName(), "Depositar");
        else
            _prompt?.Hide();
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

            string display = UnityEngine.InputSystem.InputControlPath.ToHumanReadableString(
                binding.effectivePath,
                UnityEngine.InputSystem.InputControlPath.HumanReadableStringOptions.UseShortNames);

            if (!string.IsNullOrEmpty(display))
                return display;
        }

        return "?";
    }
}

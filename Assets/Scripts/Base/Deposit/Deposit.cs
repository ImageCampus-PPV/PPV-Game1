using UnityEngine;
using UnityEngine.InputSystem;

public class Deposit : MonoBehaviour
{
    [SerializeField] private DepositData _depositData;
    [SerializeField] private DepositUI _depositUI;
    [SerializeField] private string _pickupActionName = "Pickup";

    private ItemCollector _playerInRange;
    private PlayerInput _playerInput;
    private bool _uiOpen;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Deposit] OnTriggerEnter2D: {other.gameObject.name}");

        if (_playerInRange != null)
        {
            Debug.Log("[Deposit] Ya hay un jugador en rango, ignorando.");
            return;
        }

        if (!other.TryGetComponent<ItemCollector>(out var collector))
        {
            Debug.Log($"[Deposit] {other.gameObject.name} no tiene ItemCollector.");
            return;
        }

        if (!other.TryGetComponent<PlayerInput>(out var input))
        {
            Debug.Log($"[Deposit] {other.gameObject.name} no tiene PlayerInput.");
            return;
        }

        _playerInRange = collector;
        _playerInput = input;

        Debug.Log($"[Deposit] Jugador en rango: {other.gameObject.name}. Suscribiendo input '{_pickupActionName}'.");
        SubscribeInput();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.TryGetComponent<ItemCollector>(out var collector)) return;
        if (collector != _playerInRange) return;

        Debug.Log($"[Deposit] Jugador salio del rango.");
        UnsubscribeInput();
        CloseUI();

        _playerInRange = null;
        _playerInput = null;
    }

    private void SubscribeInput()
    {
        if (_playerInput == null) return;

        var action = _playerInput.actions.FindAction(_pickupActionName);
        if (action == null)
        {
            Debug.LogWarning($"[Deposit] Action '{_pickupActionName}' no encontrada en el PlayerInput.");
            return;
        }

        action.performed += OnInteract;
        Debug.Log($"[Deposit] Suscrito a '{_pickupActionName}'.");
    }

    private void UnsubscribeInput()
    {
        if (_playerInput == null) return;
        var action = _playerInput.actions.FindAction(_pickupActionName);
        if (action != null)
            action.performed -= OnInteract;
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        Debug.Log("[Deposit] OnInteract disparado.");

        if (_uiOpen)
            CloseUI();
        else
            OpenUI();
    }

    private void OpenUI()
    {
        if (_depositUI == null)
        {
            Debug.LogWarning("[Deposit] _depositUI no asignado.");
            return;
        }

        if (_depositData == null)
        {
            Debug.LogWarning("[Deposit] _depositData no asignado.");
            return;
        }

        _uiOpen = true;
        _depositUI.Open(_depositData, _playerInRange);
        Debug.Log("[Deposit] UI abierta.");
    }

    private void CloseUI()
    {
        _uiOpen = false;
        _depositUI?.Close();
        Debug.Log("[Deposit] UI cerrada.");
    }
}

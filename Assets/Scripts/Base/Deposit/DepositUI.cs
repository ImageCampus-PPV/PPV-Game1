using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DepositUI : MonoBehaviour
{
    [Header("Materiales")]
    [SerializeField] private DepositSlotUI _materialSlotPrefab;
    [SerializeField] private Transform _materialContainer;

    [Header("Armas")]
    [SerializeField] private DepositSlotUI _weaponSlotPrefab;
    [SerializeField] private Transform _weaponContainer;

    [Header("Inventario (para depositar)")]
    [SerializeField] private MyInventory _inventory;

    private DepositData _data;
    private ItemCollector _player;
    private PlayerInput _playerInput;

    private readonly List<DepositSlotUI> _materialSlots = new();
    private readonly List<DepositSlotUI> _weaponSlots = new();

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Open(DepositData data, ItemCollector player)
    {
        _data = data;
        _player = player;
        _playerInput = player?.GetComponent<PlayerInput>();

        gameObject.SetActive(true);
        _data.OnChanged += Refresh;

        SubscribeCancelInput();
        Refresh();
    }

    public void Close()
    {
        if (_data != null)
            _data.OnChanged -= Refresh;

        UnsubscribeCancelInput();
        gameObject.SetActive(false);
    }

    private void Refresh()
    {
        RefreshMaterials();
        RefreshWeapons();
    }

    private void RefreshMaterials()
    {
        while (_materialSlots.Count < _data.MaterialSlots)
        {
            DepositSlotUI slot = Instantiate(_materialSlotPrefab, _materialContainer);
            _materialSlots.Add(slot);
        }

        var materials = _data.Materials;

        for (int i = 0; i < _materialSlots.Count; i++)
        {
            if (i < materials.Count)
            {
                InventoryStack stack = materials[i];
                int capturedIndex = i;

                _materialSlots[i].SetMaterial(
                    stack,
                    onWithdraw: () => WithdrawMaterial(stack.Type),
                    onDestroy: () => DestroyMaterial(stack.Type));
            }
            else
            {
                _materialSlots[i].Clear();
            }
        }

        int firstEmpty = materials.Count;

        if (firstEmpty < _materialSlots.Count && _inventory != null && _inventory.StackCount > 0)
        {
            _materialSlots[firstEmpty].SetMaterial(
                _inventory.Stacks[0],
                onWithdraw: () => DepositFromInventory(),
                onDestroy: null);
        }
    }

    private void RefreshWeapons()
    {
        while (_weaponSlots.Count < _data.WeaponSlots)
        {
            DepositSlotUI slot = Instantiate(_weaponSlotPrefab, _weaponContainer);
            _weaponSlots.Add(slot);
        }

        for (int i = 0; i < _data.WeaponSlots; i++)
        {
            int capturedIndex = i;
            WeaponStrategy weapon = _data.GetWeapon(i);
            _weaponSlots[i].SetWeapon(i, weapon, onEquip: () => EquipWeapon(capturedIndex));
        }
    }

    private void DepositFromInventory()
    {
        if (_inventory == null || _inventory.StackCount == 0) 
            return;

        InventoryStack stack = _inventory.Stacks[0];

        if (_data.TryDeposit(stack))
            _inventory.RemoveStackAt(0);
    }

    private void WithdrawMaterial(ItemType type)
    {
        if (_inventory == null) 
            return;

        _data.TryWithdraw(type);
    }

    private void DestroyMaterial(ItemType type)
    {
        _data.DestroyMaterial(type);
    }

    private void EquipWeapon(int slot)
    {
        if (_player == null) 
            return;

        if (!_player.TryGetComponent<Character>(out var character)) 
            return;

        foreach (CharacterAbility ability in character.ActiveAbilities)
        {
            if (ability is MechaCombat mechaCombat)
            {
                WeaponStrategy weapon = _data.GetWeapon(slot);

                if (weapon == null) 
                    return;

                if (slot < 2)
                    mechaCombat.TryEquipSlot1(weapon);
                else
                    mechaCombat.TryEquipSlot2(weapon);

                break;
            }
        }
    }

    private void SubscribeCancelInput()
    {
        if (_playerInput == null) 
            return;
        InputAction action = _playerInput.actions.FindAction("Cancel");

        if (action != null)
            action.performed += OnCancel;
    }

    private void UnsubscribeCancelInput()
    {
        if (_playerInput == null) 
            return;

        InputAction action = _playerInput.actions.FindAction("Cancel");

        if (action != null)
            action.performed -= OnCancel;
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        Close();
    }
}

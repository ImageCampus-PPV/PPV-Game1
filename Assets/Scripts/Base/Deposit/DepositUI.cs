using UnityEngine;
using UnityEngine.InputSystem;

public class DepositUI : MonoBehaviour
{
    [Header("Slots de Materiales (asignar en orden en el Inspector)")]
    [SerializeField] private DepositSlotUI[] _materialSlots;

    [Header("Slots de Armas (asignar en orden en el Inspector, uno por arma)")]
    [SerializeField] private DepositSlotUI[] _weaponSlots;

    [Header("Config")]
    [SerializeField] private int _unlockedMaterialSlots = 4;

    [Header("Inventario")]
    [SerializeField] private MyInventory _inventory;

    private DepositData _data;
    private ItemCollector _player;
    private PlayerInput _playerInput;
    private MechaCombat _mechaCombat;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Open(DepositData data, ItemCollector player)
    {
        _data = data;
        _player = player;
        _playerInput = player?.GetComponent<PlayerInput>();
        _mechaCombat = FindMechaCombat(player);

        gameObject.SetActive(true);
        _data.OnChanged += Refresh;

        InitSlotStates();
        SubscribeCancelInput();
        Refresh();
    }

    public void Close()
    {
        if (_data != null) _data.OnChanged -= Refresh;
        UnsubscribeCancelInput();
        gameObject.SetActive(false);
    }

    private void InitSlotStates()
    {
        for (int i = 0; i < _materialSlots.Length; i++)
            _materialSlots[i].SetBlocked(i >= _unlockedMaterialSlots);
    }

    public void UnlockNextMaterialSlot()
    {
        if (_unlockedMaterialSlots >= _materialSlots.Length) return;
        _materialSlots[_unlockedMaterialSlots].Unlock();
        _unlockedMaterialSlots++;
        Refresh();
    }

    private void Refresh()
    {
        RefreshMaterials();
        RefreshWeapons();
    }

    private void RefreshMaterials()
    {
        var materials = _data.Materials;

        for (int i = 0; i < _materialSlots.Length; i++)
        {
            if (_materialSlots[i].IsBlocked) continue;

            if (i < materials.Count)
            {
                InventoryStack stack = materials[i];
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
        if (firstEmpty < _unlockedMaterialSlots &&
            firstEmpty < _materialSlots.Length &&
            _inventory != null &&
            _inventory.StackCount > 0)
        {
            _materialSlots[firstEmpty].SetMaterial(
                _inventory.Stacks[0],
                onWithdraw: () => DepositFromInventory(),
                onDestroy: null);
        }
    }

    private void RefreshWeapons()
    {
        for (int i = 0; i < _weaponSlots.Length && i < _data.WeaponCount; i++)
        {
            int capturedIndex = i;
            WeaponStrategy weapon = _data.GetWeapon(i);
            bool inDeposit = _data.IsInDeposit(i);

            _weaponSlots[i].SetWeapon(
                weapon,
                isEquipped: !inDeposit,
                onSwap: () => SwapWeapon(capturedIndex));
        }
    }

    private void SwapWeapon(int depositSlotIndex)
    {
        if (_mechaCombat == null) return;

        WeaponStrategy weaponToEquip = _data.GetWeapon(depositSlotIndex);
        if (weaponToEquip == null) return;

        bool isMelee = weaponToEquip.AllowedSlot == WeaponSlot.Melee;

        if (isMelee)
        {
            for (int i = 0; i < _data.WeaponCount; i++)
            {
                if (i == depositSlotIndex) continue;
                WeaponStrategy other = _data.GetWeapon(i);
                if (other != null && other.AllowedSlot == WeaponSlot.Melee && !_data.IsInDeposit(i))
                {
                    _data.SetWeaponEquipped(i, false);
                    break;
                }
            }
            _mechaCombat.TryEquipSlot1(weaponToEquip);
        }
        else
        {
            for (int i = 0; i < _data.WeaponCount; i++)
            {
                if (i == depositSlotIndex) continue;
                WeaponStrategy other = _data.GetWeapon(i);
                if (other != null && other.AllowedSlot == WeaponSlot.Ranged && !_data.IsInDeposit(i))
                {
                    _data.SetWeaponEquipped(i, false);
                    break;
                }
            }
            _mechaCombat.TryEquipSlot2(weaponToEquip);
        }

        _data.SetWeaponEquipped(depositSlotIndex, true);
    }

    private void DepositFromInventory()
    {
        if (_inventory == null || _inventory.StackCount == 0) return;
        InventoryStack stack = _inventory.Stacks[0];
        if (_data.TryDeposit(stack))
            _inventory.RemoveStackAt(0);
    }

    private void WithdrawMaterial(ItemType type) => _data.TryWithdraw(type);
    private void DestroyMaterial(ItemType type) => _data.DestroyMaterial(type);

    private MechaCombat FindMechaCombat(ItemCollector player)
    {
        if (player == null) return null;
        if (!player.TryGetComponent<Character>(out var character)) return null;

        foreach (CharacterAbility ability in character.ActiveAbilities)
            if (ability is MechaCombat mc) return mc;

        return null;
    }

    private void SubscribeCancelInput()
    {
        if (_playerInput == null) return;
        InputAction action = _playerInput.actions.FindAction("Cancel");
        if (action != null) action.performed += OnCancel;
    }

    private void UnsubscribeCancelInput()
    {
        if (_playerInput == null) return;
        InputAction action = _playerInput.actions.FindAction("Cancel");
        if (action != null) action.performed -= OnCancel;
    }

    private void OnCancel(InputAction.CallbackContext context) => Close();
}

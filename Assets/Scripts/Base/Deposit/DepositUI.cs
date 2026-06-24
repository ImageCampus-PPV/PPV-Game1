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
    private InventoryDepositMediator _mediator;

    private void Awake()
    {
        gameObject.SetActive(false);
        // NO buscar el mediador aqui — Awake no corre si el objeto arranca desactivado
    }

    public void Open(DepositData data, ItemCollector player)
    {
        _data = data;
        _player = player;
        _playerInput = player?.GetComponent<PlayerInput>();
        _mechaCombat = FindMechaCombat(player);

        // Buscar el mediador aqui, cuando el objeto ya esta activo
        if (_mediator == null)
            _mediator = GetComponent<InventoryDepositMediator>();

        if (_mediator == null)
        {
            Debug.LogError("[DepositUI] InventoryDepositMediator no encontrado. Agregalo al mismo GameObject que DepositUI.");
            return;
        }

        _mediator.Initialize(_inventory, _data);
        _inventory.SetMediator(_mediator);

        gameObject.SetActive(true);
        _data.OnChanged += Refresh;

        InitSlotStates();
        SubscribeCancelInput();
        Refresh();

        BlockPlayerInput(true);
    }

    public void Close()
    {
        if (_data != null)
            _data.OnChanged -= Refresh;

        UnsubscribeCancelInput();
        gameObject.SetActive(false);

        BlockPlayerInput(false);
    }

    private void BlockPlayerInput(bool block)
    {
        foreach (var character in FindObjectsByType<Character>(FindObjectsSortMode.None))
            character.IsIgnoringInput = block;
    }

    private void InitSlotStates()
    {
        for (int i = 0; i < _materialSlots.Length; i++)
            _materialSlots[i].SetBlocked(i >= _unlockedMaterialSlots);
    }

    public void UnlockNextMaterialSlot()
    {
        if (_unlockedMaterialSlots >= _materialSlots.Length)
            return;

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

            InventoryStack stack = i < materials.Count ? materials[i] : null;

            if (stack != null)
            {
                _materialSlots[i].SetMaterial(
                    stack,
                    onWithdraw: () => WithdrawMaterial(stack.Type),
                    onDestroy: () => DestroyMaterial(stack.Type));
            }
            else
            {
                _materialSlots[i].Clear();
            }

            var slotData = new SlotData { Stack = stack, Owner = SlotOwner.Deposit, Index = i };

            DraggableSlot draggable = _materialSlots[i].GetComponent<DraggableSlot>();
            DroppableSlot droppable = _materialSlots[i].GetComponent<DroppableSlot>();

            if (draggable != null) draggable.SlotData = slotData;

            if (droppable != null)
            {
                droppable.SlotData = slotData;
                droppable.Mediator = _mediator;
            }
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

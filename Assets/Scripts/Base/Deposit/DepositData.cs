using System;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(menuName = "Base/DepositData")]
public class DepositData : ScriptableObject
{
    [Header("Config")]
    [SerializeField] private int _materialSlots = 10;
    [SerializeField] private int _weaponSlots = 4;

    private readonly List<InventoryStack> _materials = new();

    private readonly WeaponStrategy[] _weapons = new WeaponStrategy[4];

    public int MaterialSlots => _materialSlots;
    public int WeaponSlots => _weaponSlots;
    public IReadOnlyList<InventoryStack> Materials => _materials;
    public WeaponStrategy[] Weapons => _weapons;

    public bool IsMaterialFull => _materials.Count >= _materialSlots;

    public Action OnChanged;

    public bool TryDeposit(InventoryStack stack)
    {
        if (stack == null) 
            return false;

        InventoryStack existing = _materials.Find(s => s.Type == stack.Type);
        if (existing != null)
        {
            existing.Count += stack.Count;
            OnChanged?.Invoke();
            return true;
        }

        if (IsMaterialFull) 
            return false;

        _materials.Add(new InventoryStack(stack.Sample) { Count = stack.Count });
        OnChanged?.Invoke();
        return true;
    }

    public bool TryWithdraw(ItemType type)
    {
        InventoryStack stack = _materials.Find(s => s.Type == type);

        if (stack == null) 
            return false;

        stack.Count--;

        if (stack.Count <= 0)
            _materials.Remove(stack);

        OnChanged?.Invoke();
        return true;
    }

    public void DestroyMaterial(ItemType type)
    {
        _materials.RemoveAll(s => s.Type == type);
        OnChanged?.Invoke();
    }

    public void SetWeapon(int slot, WeaponStrategy weapon)
    {
        if (slot < 0 || slot >= _weapons.Length) 
            return;

        _weapons[slot] = weapon;
        OnChanged?.Invoke();
    }

    public WeaponStrategy GetWeapon(int slot)
    {
        if (slot < 0 || slot >= _weapons.Length) 
            return null;

        return _weapons[slot];
    }

    public void Reset()
    {
        _materials.Clear();

        for (int i = 0; i < _weapons.Length; i++)
            _weapons[i] = null;

        OnChanged?.Invoke();
    }
}

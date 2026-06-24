using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Base/DepositData")]
public class DepositData : ScriptableObject
{
    [Header("Materiales")]
    [SerializeField] private int _materialSlots = 7;

    [Header("Armas fijas (asignar los 4 assets en orden)")]
    [SerializeField] private WeaponStrategy[] _allWeapons = new WeaponStrategy[4];

    private bool[] _weaponEquipped;

    private readonly List<InventoryStack> _materials = new();

    public int MaterialSlots => _materialSlots;
    public IReadOnlyList<InventoryStack> Materials => _materials;
    public bool IsMaterialFull => _materials.Count >= _materialSlots;
    public int WeaponCount => _allWeapons.Length;

    public Action OnChanged;

    private void EnsureArrayInitialized()
    {
        if (_weaponEquipped == null || _weaponEquipped.Length != _allWeapons.Length)
            _weaponEquipped = new bool[_allWeapons.Length];
    }

    public void InitWeapons(int[] equippedSlots)
    {
        _weaponEquipped = new bool[_allWeapons.Length];

        foreach (int slot in equippedSlots)
            if (slot >= 0 && slot < _weaponEquipped.Length)
                _weaponEquipped[slot] = true;

        Debug.Log($"[DepositData] InitWeapons: equipadas={string.Join(",", equippedSlots)}");
        OnChanged?.Invoke();
    }

    public WeaponStrategy GetWeapon(int index)
    {
        if (index < 0 || index >= _allWeapons.Length) return null;
        return _allWeapons[index];
    }

    public bool IsInDeposit(int index)
    {
        EnsureArrayInitialized();
        if (index < 0 || index >= _weaponEquipped.Length) return true;
        return !_weaponEquipped[index];
    }

    public void SetWeaponEquipped(int index, bool equipped)
    {
        EnsureArrayInitialized();
        if (index < 0 || index >= _weaponEquipped.Length) return;
        _weaponEquipped[index] = equipped;
        OnChanged?.Invoke();
    }


    public bool TryDeposit(InventoryStack stack)
    {
        if (stack == null) return false;

        InventoryStack existing = _materials.Find(s => s.Type == stack.Type);
        if (existing != null)
        {
            existing.Count += stack.Count;
            OnChanged?.Invoke();
            return true;
        }

        if (IsMaterialFull) return false;

        _materials.Add(new InventoryStack(stack.Sample) { Count = stack.Count });
        OnChanged?.Invoke();
        return true;
    }

    public bool TryWithdraw(ItemType type)
    {
        InventoryStack stack = _materials.Find(s => s.Type == type);
        if (stack == null) return false;

        stack.Count--;
        if (stack.Count <= 0) _materials.Remove(stack);

        OnChanged?.Invoke();
        return true;
    }

    public void DestroyMaterial(ItemType type)
    {
        _materials.RemoveAll(s => s.Type == type);
        OnChanged?.Invoke();
    }

    public void Reset()
    {
        _materials.Clear();
        _weaponEquipped = new bool[_allWeapons.Length];
        OnChanged?.Invoke();
    }

    public void SetStackAt(int index, InventoryStack stack)
    {
        while (_materials.Count <= index)
            _materials.Add(null);

        _materials[index] = stack;

        for (int i = _materials.Count - 1; i >= 0; i--)
        {
            if (_materials[i] == null)
                _materials.RemoveAt(i);
            else
                break;
        }

        OnChanged?.Invoke();
    }

    public void NotifyChanged()
    {
        OnChanged?.Invoke();
    }
}

using System;
using UnityEngine;


[CreateAssetMenu(menuName = "Base/DepositData")]
public class DepositData : ScriptableObject
{
    [Header("Materiales requeridos (orden = orden de los slots)")]
    [SerializeField] private ItemType[] _requiredTypes = new ItemType[3];

    // true = ese material ya fue depositado
    private bool[] _hasMaterial;

    public Action OnChanged;
    public Action OnBombUnlocked;

    public int RequiredCount => _requiredTypes.Length;
    public bool IsBombUnlocked { get; private set; }
    public bool IsBombBuilt { get; private set; }

    private void EnsureInitialized()
    {
        if (_hasMaterial == null || _hasMaterial.Length != _requiredTypes.Length)
            _hasMaterial = new bool[_requiredTypes.Length];
    }

    public ItemType GetRequiredType(int index)
    {
        if (index < 0 || index >= _requiredTypes.Length) return null;
        return _requiredTypes[index];
    }

    public bool HasMaterial(int index)
    {
        EnsureInitialized();
        if (index < 0 || index >= _hasMaterial.Length) return false;
        return _hasMaterial[index];
    }

    public bool Deposit(ItemType type)
    {
        EnsureInitialized();

        int index = Array.IndexOf(_requiredTypes, type);
        if (index < 0) return false;

        _hasMaterial[index] = true;
        OnChanged?.Invoke();

        CheckBombUnlock();
        return true;
    }

    private void CheckBombUnlock()
    {
        if (IsBombUnlocked) return;

        EnsureInitialized();
        foreach (bool has in _hasMaterial)
            if (!has) return;

        IsBombUnlocked = true;
        OnBombUnlocked?.Invoke();
        OnChanged?.Invoke();
    }

    public bool TryBuildBomb()
    {
        if (!IsBombUnlocked || IsBombBuilt) return false;

        EnsureInitialized();
        for (int i = 0; i < _hasMaterial.Length; i++)
            _hasMaterial[i] = false;

        IsBombUnlocked = false;
        IsBombBuilt = true;

        OnChanged?.Invoke();
        return true;
    }

    public void Reset()
    {
        EnsureInitialized();
        for (int i = 0; i < _hasMaterial.Length; i++)
            _hasMaterial[i] = false;

        IsBombUnlocked = false;
        IsBombBuilt = false;
        OnChanged?.Invoke();
    }
}

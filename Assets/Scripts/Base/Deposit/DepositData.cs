using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Base/DepositData")]
public class DepositData : ScriptableObject
{
    [Serializable]
    public class MaterialCount
    {
        public ItemType Type;
        public int Count;
    }

    [SerializeField] private List<MaterialCount> _materials = new();

    public IReadOnlyList<MaterialCount> Materials => _materials;
    public Action OnChanged;

    public void Deposit(ItemType type)
    {
        if (type == null) return;

        var entry = _materials.Find(m => m.Type == type);
        if (entry != null)
            entry.Count++;
        else
            _materials.Add(new MaterialCount { Type = type, Count = 1 });

        OnChanged?.Invoke();
    }

    public int GetCount(ItemType type)
    {
        var entry = _materials.Find(m => m.Type == type);
        return entry != null ? entry.Count : 0;
    }

    public bool TrySpend(ItemType type, int amount)
    {
        var entry = _materials.Find(m => m.Type == type);
        if (entry == null || entry.Count < amount) return false;

        entry.Count -= amount;
        OnChanged?.Invoke();
        return true;
    }

    public void Reset()
    {
        _materials.Clear();
        OnChanged?.Invoke();
    }
}

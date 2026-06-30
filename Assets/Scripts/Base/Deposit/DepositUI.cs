using UnityEngine;


public class DepositUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private DepositData _data;

    [Header("Slots de materiales (mismo orden que DepositData)")]
    [SerializeField] private DepositSlotUI[] _materialSlots;

    [Header("Slot de la bomba")]
    [SerializeField] private DepositSlotUI _bombSlot;

    [Header("Iconos (opcional)")]
    [SerializeField] private ItemIconEntry[] _itemIcons;

    [System.Serializable]
    public struct ItemIconEntry
    {
        public ItemType type;
        public Sprite icon;
    }

    private void OnEnable()
    {
        if (_data != null)
            _data.OnChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (_data != null)
            _data.OnChanged -= Refresh;
    }

    private void Refresh()
    {
        if (_data == null) return;

        for (int i = 0; i < _materialSlots.Length && i < _data.RequiredCount; i++)
        {
            ItemType type = _data.GetRequiredType(i);
            bool hasMaterial = _data.HasMaterial(i);
            Sprite icon = GetIcon(type);

            _materialSlots[i].SetMaterial(type, hasMaterial, icon);
        }

        if (_bombSlot != null)
            _bombSlot.SetBombSlot(_data.IsBombUnlocked, _data.IsBombBuilt);
    }

    private Sprite GetIcon(ItemType type)
    {
        if (type == null || _itemIcons == null) return null;

        foreach (var entry in _itemIcons)
            if (entry.type == type)
                return entry.icon;

        return null;
    }
}

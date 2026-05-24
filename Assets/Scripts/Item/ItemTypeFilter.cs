using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/ItemTypeFilter")]
public class ItemTypeFilter : ItemFilter
{
    [SerializeField] private List<ItemType> _allowedTypes;

    public override bool CanCollect(Item item)
    {
        return item != null && _allowedTypes.Contains(item.Type);
    }
}
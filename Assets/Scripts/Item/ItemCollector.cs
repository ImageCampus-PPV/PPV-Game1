using System;
using UnityEngine;

public class ItemCollector : MonoBehaviour
{
    [SerializeField] private ItemFilter _filter;
    public Action OnItemCollected;

    public ItemFilter Filter { get => _filter; set => _filter = value; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Item>(out var item))
        {
            if (_filter.CanCollect(item))
            {
                item.Collect();
                OnItemCollected?.Invoke();
            }
            else
                item.Reject();
        }
    }
}

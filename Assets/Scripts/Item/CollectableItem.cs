using UnityEngine;
using ImageCampus.ToolBox.Services;


public class CollectableItem : Item
{
    public enum CollectionMode { Manual, Automatic }

    [Header("Config")]
    [SerializeField] private CollectionMode _collectionMode = CollectionMode.Manual;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_collectionMode != CollectionMode.Automatic) 
            return;

        if (!other.TryGetComponent<ItemCollector>(out var collector)) 
            return;

        if (!collector.Filter.CanCollect(this)) 
            return;

        Collect();
        collector.OnItemCollected?.Invoke();
    }

    public override void UseItem()
    {
        Debug.Log($"[CollectableItem] Usando {name}.");
        // TODO: implementar efecto del item al usarse
    }
}

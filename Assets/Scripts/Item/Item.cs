using NUnit.Framework;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public abstract class Item : MonoBehaviour
{
    [SerializeField] private ItemType _itemType;
    public ItemType Type => _itemType;

    public virtual void Collect(ItemCollector collector = null)
    {
        if (!gameObject.activeSelf)
            return;

        if (collector != null && collector.TryGetComponent<MyInventory>(out var inventory))
        {
            Debug.Log("Collecting item");

            if (!inventory.TryAdd(this))
                Reject();
        }
        //else
        //{
        //    gameObject.SetActive(false);
        //}
    }

    public abstract void UseItem();

    public virtual void Reject()
    {
        Debug.Log("Item rejected");
        //TODO: Visual rejection feedback.
    }
}

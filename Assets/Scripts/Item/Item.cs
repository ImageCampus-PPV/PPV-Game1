using NUnit.Framework;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public abstract class Item : MonoBehaviour
{
    [SerializeField] private ItemType _itemType;
    public ItemType Type => _itemType;

    public virtual void Collect()
    {
        if (!gameObject.activeSelf)
            return;

        //TODO: Add to inventory here.
        gameObject.SetActive(false);
    }

    public abstract void UseItem();

    public virtual void Reject()
    {
        //TODO: Visual rejection feedback.
    }
}

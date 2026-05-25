using UnityEngine;
using ImageCampus.ToolBox.Services;

[RequireComponent(typeof(BoxCollider2D))]
public abstract class Item : MonoBehaviour
{
    [SerializeField] private ItemType _itemType;
    public ItemType Type => _itemType;

    public virtual void Collect()
    {
        if (!gameObject.activeSelf)
            return;

        if (ServiceProvider.Instance.ContainsService<MyInventory>())
        {
            var inventory = ServiceProvider.Instance.GetService<MyInventory>();
            if (!inventory.TryAdd(this))
                Reject();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public abstract void UseItem();

    public virtual void Reject()
    {
        Debug.Log("Item rejected");
        //TODO: Visual rejection feedback.
    }
}

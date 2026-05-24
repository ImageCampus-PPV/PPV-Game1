using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public abstract class Item : MonoBehaviour
{
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

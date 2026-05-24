using UnityEngine;

public abstract class ItemFilter : ScriptableObject
{
    public abstract bool CanCollect(Item item);
}

using UnityEngine;

[RequireComponent(typeof(Item))]
public class ItemPickupArea : MonoBehaviour
{
    [SerializeField] private ItemPickupPrompt _prompt;

    public Item Item { get; private set; }

    private void Awake()
    {
        Item = GetComponent<Item>();
        _prompt?.Hide();
    }

    public void ShowPrompt(string buttonName)
    {
        string itemName = Item.Type != null ? Item.Type.name : gameObject.name;
        _prompt?.Show(buttonName, itemName);
    }

    public void HidePrompt()
    {
        _prompt?.Hide();
    }
}

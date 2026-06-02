using UnityEngine;


[RequireComponent(typeof(Item))]
public class ItemPickupArea : MonoBehaviour
{
    [SerializeField] private string _itemDisplayName = "Item";
    [SerializeField] private ItemPickupPrompt _prompt;

    public Item Item { get; private set; }

    private void Awake()
    {
        Item = GetComponent<Item>();
        _prompt?.Hide();
    }

    public void ShowPrompt(string buttonName)
    {
        _prompt?.Show(buttonName, _itemDisplayName);
    }

    public void HidePrompt()
    {
        _prompt?.Hide();
    }
}

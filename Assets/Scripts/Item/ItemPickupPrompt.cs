using TMPro;
using UnityEngine;


public class ItemPickupPrompt : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _buttonText;
    [SerializeField] private TextMeshProUGUI _itemNameText;

    public void Show(string buttonName, string itemName)
    {
        gameObject.SetActive(true);

        if (_buttonText != null)
            _buttonText.text = $"[{buttonName}]";

        if (_itemNameText != null)
            _itemNameText.text = itemName;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}

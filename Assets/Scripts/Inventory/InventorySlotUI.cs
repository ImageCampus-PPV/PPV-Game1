using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private GameObject _emptyIndicator;

    public void SetItem(Item item)
    {
        if (_emptyIndicator != null)
            _emptyIndicator.SetActive(false);

        if (_nameText != null)
            _nameText.text = item.name;

        if (_icon != null)
        {
            var sr = item.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                _icon.sprite = sr.sprite;
                _icon.enabled = true;
            }
        }
    }

    public void Clear()
    {
        if (_icon != null)
        {
            _icon.sprite = null;
            _icon.enabled = false;
        }

        if (_nameText != null)
            _nameText.text = string.Empty;

        if (_emptyIndicator != null)
            _emptyIndicator.SetActive(true);
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _countText;
    [SerializeField] private GameObject _emptyIndicator;

    public void SetStack(InventoryStack stack)
    {
        if (_emptyIndicator != null)
            _emptyIndicator.SetActive(false);

        if (_nameText != null)
            _nameText.text = stack.Type != null ? stack.Type.name : stack.Sample.name;

        if (_countText != null)
        {
            _countText.gameObject.SetActive(true);
            _countText.text = $"x{stack.Count}";
        }

        if (_icon != null)
        {
            var sr = stack.Sample.GetComponent<SpriteRenderer>();
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

        if (_countText != null)
        {
            _countText.gameObject.SetActive(false);
            _countText.text = string.Empty;
        }

        if (_emptyIndicator != null)
            _emptyIndicator.SetActive(true);
    }
}

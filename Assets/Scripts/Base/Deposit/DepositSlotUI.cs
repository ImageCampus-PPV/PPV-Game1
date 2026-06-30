using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class DepositSlotUI : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private GameObject _emptyOverlay;
    [SerializeField] private GameObject _blockedOverlay;

    public void SetMaterial(ItemType type, bool hasMaterial, Sprite icon)
    {
        if (_blockedOverlay != null) _blockedOverlay.SetActive(false);

        if (_nameText != null)
            _nameText.text = type != null ? type.name : "?";

        if (_icon != null)
        {
            _icon.sprite = icon;
            _icon.color = hasMaterial ? Color.white : new Color(1, 1, 1, 0.3f);
        }

        if (_emptyOverlay != null)
            _emptyOverlay.SetActive(!hasMaterial);
    }

    public void SetBombSlot(bool unlocked, bool built)
    {
        if (_nameText != null)
            _nameText.text = built ? "Bomba lista" : unlocked ? "Bomba" : "Bloqueado";

        if (_blockedOverlay != null)
            _blockedOverlay.SetActive(!unlocked);

        if (_emptyOverlay != null)
            _emptyOverlay.SetActive(false);

        if (_icon != null)
            _icon.color = unlocked ? Color.white : new Color(1, 1, 1, 0.3f);
    }
}

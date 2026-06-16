using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class WeaponSlotUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private Image _cooldownFill;
    [SerializeField] private GameObject _cooldownOverlay;
    [SerializeField] private TextMeshProUGUI _slotLabel;


    public void UpdateSlot(WeaponStrategy weapon, float cooldownProgress)
    {
        if (weapon == null)
        {
            SetEmpty();
            return;
        }

        if (_nameText != null)
            _nameText.text = weapon.name.Replace("Weapon", "").Replace("(Clone)", "").Trim();

        if (_icon != null && _icon.sprite == null)
            _icon.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        bool isOnCooldown = cooldownProgress < 1f;

        if (_cooldownFill != null)
            _cooldownFill.fillAmount = cooldownProgress;

        if (_cooldownOverlay != null)
            _cooldownOverlay.SetActive(isOnCooldown);
    }

    public void SetIcon(Sprite sprite)
    {
        if (_icon == null) 
            return;

        _icon.sprite = sprite;
        _icon.color = Color.white;
    }

    public void SetSlotLabel(string label)
    {
        if (_slotLabel != null)
            _slotLabel.text = label;
    }

    private void SetEmpty()
    {
        if (_nameText != null) 
            _nameText.text = "Vacío";

        if (_icon != null)
        { 
            _icon.sprite = null; 
            _icon.color = new Color(0.2f, 0.2f, 0.2f, 1f); 
        }

        if (_cooldownFill != null) 
            _cooldownFill.fillAmount = 1f;

        if (_cooldownOverlay != null)
            _cooldownOverlay.SetActive(false);
    }
}

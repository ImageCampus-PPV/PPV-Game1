using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponSlotUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private Image _cooldownFill;
    [SerializeField] private Image _chargeFill;
    [SerializeField] private GameObject _cooldownOverlay;
    [SerializeField] private TextMeshProUGUI _slotLabel;

    public void UpdateSlot(WeaponStrategy weapon, float cooldownProgress, float chargeProgress = 1f)
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

        UpdateCooldown(cooldownProgress);
        UpdateCharge(chargeProgress);
    }

    public void UpdateCooldown(float cooldownProgress)
    {
        bool isOnCooldown = cooldownProgress < 1f;

        if (_cooldownFill != null)
            _cooldownFill.fillAmount = cooldownProgress;

        if (_cooldownOverlay != null)
            _cooldownOverlay.SetActive(isOnCooldown);
    }

    public void UpdateCharge(float chargeProgress, bool showCharge = true)
    {
        if (_chargeFill == null) return;

        _chargeFill.gameObject.SetActive(showCharge);

        if (showCharge)
            _chargeFill.fillAmount = chargeProgress;
    }

    public void SetIcon(Sprite sprite)
    {
        if (_icon == null) return;
        _icon.sprite = sprite;
        _icon.color = Color.white;
    }

    public void SetSlotLabel(string label)
    {
        if (_slotLabel != null)
            _slotLabel.text = label;
    }

    public void SetName(string name)
    {
        if (_nameText != null)
            _nameText.text = name;
    }

    private void SetEmpty()
    {
        if (_nameText != null) _nameText.text = "Vacío";
        if (_icon != null) { _icon.sprite = null; _icon.color = new Color(0.2f, 0.2f, 0.2f, 1f); }
        if (_cooldownFill != null) _cooldownFill.fillAmount = 1f;
        if (_chargeFill != null) _chargeFill.fillAmount = 0f;
        if (_cooldownOverlay != null) _cooldownOverlay.SetActive(false);
    }
}

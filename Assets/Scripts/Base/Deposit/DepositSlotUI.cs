using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class DepositSlotUI : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _countText;
    [SerializeField] private Button _actionButton;
    [SerializeField] private TextMeshProUGUI _actionButtonText;
    [SerializeField] private GameObject _emptyIndicator;

    private InventoryStack _stack;
    private WeaponStrategy _weapon;
    private System.Action _onAction;

    public void SetMaterial(InventoryStack stack, System.Action onWithdraw, System.Action onDestroy)
    {
        _stack = stack;
        _weapon = null;

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
            SpriteRenderer sr = stack.Sample.GetComponent<SpriteRenderer>();

            if (sr != null) 
            { 
                _icon.sprite = sr.sprite;
                _icon.enabled = true; 
            }
        }

        _onAction = onWithdraw;

        if (_actionButton != null)
        {
            _actionButton.gameObject.SetActive(true);
            _actionButton.onClick.RemoveAllListeners();
            _actionButton.onClick.AddListener(() => _onAction?.Invoke());

            if (_actionButtonText != null) 
                _actionButtonText.text = "Retirar";
        }
    }

    public void SetWeapon(int slotIndex, WeaponStrategy weapon, System.Action onEquip)
    {
        _stack = null;
        _weapon = weapon;

        if (_emptyIndicator != null) 
            _emptyIndicator.SetActive(weapon == null);

        if (_nameText != null)
            _nameText.text = weapon != null
                ? weapon.name.Replace("Weapon", "").Replace("(Clone)", "").Trim()
                : $"Ranura {slotIndex + 1}";

        if (_countText != null)
            _countText.gameObject.SetActive(false);

        if (_icon != null)
            _icon.enabled = false;

        _onAction = onEquip;

        if (_actionButton != null)
        {
            _actionButton.gameObject.SetActive(weapon != null);
            _actionButton.onClick.RemoveAllListeners();
            _actionButton.onClick.AddListener(() => _onAction?.Invoke());

            if (_actionButtonText != null) 
                _actionButtonText.text = "Equipar";
        }
    }

    public void Clear()
    {
        _stack = null;
        _weapon = null;

        if (_nameText != null) 
            _nameText.text = string.Empty;

        if (_countText != null) 
        { 
            _countText.text = string.Empty; 
            _countText.gameObject.SetActive(false); 
        }

        if (_icon != null) 
        { 
            _icon.sprite = null; 
            _icon.enabled = false; 
        }

        if (_actionButton != null) 
            _actionButton.gameObject.SetActive(false);

        if (_emptyIndicator != null) 
            _emptyIndicator.SetActive(true);
    }
}

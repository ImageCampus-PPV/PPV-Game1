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

    [Header("Blocked State")]
    [SerializeField] private GameObject _blockedOverlay;

    [Header("Equipped Indicator (solo para slots de arma)")]
    [SerializeField] private GameObject _equippedIndicator;

    private System.Action _onAction;

    public bool IsBlocked { get; private set; }


    public void SetMaterial(InventoryStack stack, System.Action onWithdraw, System.Action onDestroy)
    {
        if (IsBlocked) 
            return;

        if (_emptyIndicator != null) 
            _emptyIndicator.SetActive(false);

        if (_equippedIndicator != null) 
            _equippedIndicator.SetActive(false);

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

    public void SetWeapon(WeaponStrategy weapon, bool isEquipped, System.Action onSwap)
    {
        if (_emptyIndicator != null) 
            _emptyIndicator.SetActive(weapon == null);

        if (_countText != null) 
            _countText.gameObject.SetActive(false);

        if (weapon == null)
        {
            if (_nameText != null) 
                _nameText.text = "Vacío";
            if (_icon != null) 
                _icon.enabled = false;
            if (_actionButton != null) 
                _actionButton.gameObject.SetActive(false);
            if (_equippedIndicator != null) 
                _equippedIndicator.SetActive(false);
            return;
        }

        if (_nameText != null)
            _nameText.text = weapon.name.Replace("Weapon", "").Replace("(Clone)", "").Trim();

        if (_icon != null) 
            _icon.enabled = false;

        if (isEquipped)
        {
            if (_equippedIndicator != null) 
                _equippedIndicator.SetActive(true);

            if (_actionButton != null) 
                _actionButton.gameObject.SetActive(false);
        }
        else
        {
            if (_equippedIndicator != null) 
                _equippedIndicator.SetActive(false);

            _onAction = onSwap;

            if (_actionButton != null)
            {
                _actionButton.gameObject.SetActive(true);
                _actionButton.onClick.RemoveAllListeners();
                _actionButton.onClick.AddListener(() => _onAction?.Invoke());

                if (_actionButtonText != null) 
                    _actionButtonText.text = "Equipar";
            }
        }
    }


    public void SetBlocked(bool blocked)
    {
        IsBlocked = blocked;

        if (_blockedOverlay != null) 
            _blockedOverlay.SetActive(blocked);

        if (blocked)
        {
            if (_nameText != null) 
                _nameText.text = "Bloqueado";
            if (_countText != null) 
                _countText.gameObject.SetActive(false);
            if (_icon != null) 
                _icon.enabled = false;
            if (_actionButton != null) 
                _actionButton.gameObject.SetActive(false);
            if (_emptyIndicator != null) 
                _emptyIndicator.SetActive(false);
            if (_equippedIndicator != null) 
                _equippedIndicator.SetActive(false);
        }
    }

    public void Unlock()
    {
        IsBlocked = false;

        if (_blockedOverlay != null) 
            _blockedOverlay.SetActive(false);

        Clear();
    }

    public void Clear()
    {
        if (IsBlocked) 
            return;

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
        if (_equippedIndicator != null) 
            _equippedIndicator.SetActive(false);
    }
}

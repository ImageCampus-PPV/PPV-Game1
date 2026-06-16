using UnityEngine;


public class WeaponHUDController : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private WeaponSlotUI _slot1UI;
    [SerializeField] private WeaponSlotUI _slot2UI;
    [SerializeField] private WeaponSlotUI _shieldSlotUI;

    [Header("Mecha")]
    [SerializeField] private Character _mechaCharacter;

    [Header("Weapon Icons (opcional)")]
    [SerializeField] private WeaponIconEntry[] _weaponIcons;
    [SerializeField] private Sprite _shieldIcon;

    [System.Serializable]
    public struct WeaponIconEntry
    {
        public string weaponTypeName;
        public Sprite icon;
    }

    private MechaCombat _mechaCombat;
    private ShieldAbility _shieldAbility;

    private void Start()
    {
        _slot1UI?.SetSlotLabel("1");
        _slot2UI?.SetSlotLabel("2");
        _shieldSlotUI?.SetSlotLabel("E");

        if (_mechaCharacter == null)
        {
            Debug.LogWarning("[WeaponHUDController] Mecha character no asignado.");
            return;
        }

        FindMechaCombat();
    }

    private void Update()
    {
        if (_mechaCombat == null)
        {
            FindMechaCombat();
            return;
        }

        RefreshSlot(_slot1UI, _mechaCombat.Slot1Weapon);
        RefreshSlot(_slot2UI, _mechaCombat.Slot2Weapon);
        RefreshShieldSlot();
    }

    private void FindMechaCombat()
    {
        if (_mechaCharacter == null) return;

        foreach (var ability in _mechaCharacter.ActiveAbilities)
        {
            if (ability is MechaCombat combat)
            {
                _mechaCombat = combat;
                OnWeaponsFound();
            }

            if (ability is ShieldAbility shield)
            {
                _shieldAbility = shield;

                if (_shieldSlotUI != null)
                {
                    //_shieldSlotUI.SetIcon(_shieldIcon);
                    _shieldSlotUI.SetName("Escudo");
                }
            }
        }
    }

    private void OnWeaponsFound()
    {
        ApplyIcon(_slot1UI, _mechaCombat.Slot1Weapon);
        ApplyIcon(_slot2UI, _mechaCombat.Slot2Weapon);
    }

    private void RefreshSlot(WeaponSlotUI slotUI, WeaponStrategy weapon)
    {
        if (slotUI == null) return;

        float cooldownProgress = weapon != null ? weapon.CooldownProgress : 1f;
        bool hasCharge = weapon != null && weapon.HasCharge;
        float chargeProgress = hasCharge ? weapon.ChargeProgress : 1f;

        slotUI.UpdateSlot(weapon, cooldownProgress, chargeProgress);
        slotUI.UpdateCharge(chargeProgress, showCharge: hasCharge);
        //slotUI.UpdateSlot(weapon, cooldownProgress);
    }

    private void RefreshShieldSlot()
    {
        if (_shieldSlotUI == null || _shieldAbility == null) 
            return;

        float cooldown = _shieldAbility.IsOnCooldown ? _shieldAbility.CooldownProgress : 1f;
        _shieldSlotUI.UpdateCooldown(cooldown);

        float charge = _shieldAbility.IsActive ? _shieldAbility.ShieldHpProgress : 1f;
        _shieldSlotUI.UpdateCharge(charge, showCharge: true);

        if (_shieldAbility.IsActive)
            _shieldSlotUI.SetName("Activo");
        else if (_shieldAbility.IsOnCooldown)
            _shieldSlotUI.SetName("Recargando");
        else
            _shieldSlotUI.SetName("Escudo");
    }

    private void ApplyIcon(WeaponSlotUI slotUI, WeaponStrategy weapon)
    {
        if (slotUI == null || weapon == null || _weaponIcons == null) return;

        string typeName = weapon.GetType().Name;
        foreach (var entry in _weaponIcons)
        {
            if (entry.weaponTypeName == typeName)
            {
                slotUI.SetIcon(entry.icon);
                return;
            }
        }
    }
}

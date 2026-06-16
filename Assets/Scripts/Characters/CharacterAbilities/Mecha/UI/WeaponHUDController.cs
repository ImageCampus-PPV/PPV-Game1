using UnityEngine;


public class WeaponHUDController : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private WeaponSlotUI _slot1UI;
    [SerializeField] private WeaponSlotUI _slot2UI;

    [Header("Mecha")]
    [SerializeField] private Character _mechaCharacter;

    [Header("Weapon Icons (opcional)")]
    [SerializeField] private WeaponIconEntry[] _weaponIcons;

    [System.Serializable]
    public struct WeaponIconEntry
    {
        public string weaponTypeName;
        public Sprite icon;
    }

    private MechaCombat _mechaCombat;

    private void Start()
    {
        _slot1UI?.SetSlotLabel("1");
        _slot2UI?.SetSlotLabel("2");

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
                break;
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

        float cooldownProgress = 1f;

        if (weapon != null)
        {
            cooldownProgress = weapon.CooldownProgress;
        }

        slotUI.UpdateSlot(weapon, cooldownProgress);
    }

    private void ApplyIcon(WeaponSlotUI slotUI, WeaponStrategy weapon)
    {
        if (slotUI == null || weapon == null || _weaponIcons == null) 
            return;

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

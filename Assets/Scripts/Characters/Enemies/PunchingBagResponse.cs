using TMPro;
using UnityEngine;

public class PunchingBagResponse : DamageResponse
{
    [SerializeField] private TextMeshProUGUI _damageText;
    [SerializeField] private float _totalDamageReceived = 0f;
    [SerializeField] private int _hitCount = 0;

    private void Start()
    {
        UpdateUI(0);
    }

    public override void ReactToDamage(float damage)
    {
        _totalDamageReceived += damage;
        _hitCount++;
        UpdateUI(damage);
    }

    private void UpdateUI(float damage)
    {
        if (_damageText!= null)
            _damageText.text = $"Hits: {_hitCount}\n Dmg: {damage}\n Total Dmg: {_totalDamageReceived}";
    }
}
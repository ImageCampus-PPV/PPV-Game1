using TMPro;
using UnityEngine;

public class PunchingBagResponse : DamageResponse
{
    [SerializeField] private TextMeshProUGUI _damageText;
    [SerializeField] private float _totalDamageReceived = 0f;
    [SerializeField] private int _hitCount = 0;
    private float _lastDamageTaken = 0;
    private Enemy _enemy;
    private string _currentEffectsText;

    private void Start()
    {
        UpdateUI();
        _enemy = GetComponent<Enemy>();
    }

    public override void ReactToDamage(float damage)
    {
        _totalDamageReceived += damage;
        _hitCount++;
        _lastDamageTaken = damage;
        UpdateUI();
    }

    private void Update()
    {
        if (_enemy == null)
            return;

        string effectsText = "";

        foreach (StatusEffect effect in _enemy.ActiveEffects)
        {
            effectsText += $"{effect.DisplayName}: {effect.RemainingTime}\n";
        }

        _currentEffectsText = effectsText;

        UpdateUI();
    }

    private string UpdateUI()
    {
        if (_damageText!= null)
        {
            _damageText.text = $"Hits: {_hitCount}\n Dmg: {_lastDamageTaken}\n Total Dmg: {_totalDamageReceived}\n" + _currentEffectsText;
            return _damageText.text;
        }

        return "";
    }
}
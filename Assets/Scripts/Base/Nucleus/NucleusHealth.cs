using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NucleusHealth : DamageableEntity
{
    [Header("Config")]
    [SerializeField] private float _maxHp = 500f;
    [SerializeField] private string _gameOverSceneName = "GameOver";

    [Header("UI")]
    [SerializeField] private UnityEngine.UI.Slider _hpSlider;

    private float _currentHp;
    private bool _isDead;

    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    public float CurrentHp => _currentHp;
    public float MaxHp => _maxHp;
    public float HpPercent => _currentHp / _maxHp;
    public bool IsDead => _isDead;

    private void Awake()
    {
        _currentHp = _maxHp;
        RefreshUI();
    }

    private void Start()
    {
        base.Init();
    }

    public override void TakeDamage(float amount)
    {
        if (_isDead)
            return;

        _currentHp = Mathf.Max(0f, _currentHp - amount);
        EventBus.Raise<OnCombatDamage>(ID, amount);

        RefreshUI();

        Debug.Log($"[Nucleus] Recibio {amount} de dano. HP: {_currentHp}/{_maxHp}");

        if (_currentHp <= 0f)
            TriggerGameOver();
    }

    public void OnHordeComplete()
    {
        if (_isDead) return;

        _currentHp = _maxHp;
        RefreshUI();
        EventBus.Raise<OnHordeEnded>();

        Debug.Log("[Nucleus] Horda terminada. Vida restaurada.");
    }

    private void TriggerGameOver()
    {
        _isDead = true;
        EventBus.Raise<OnNucleusDestroyed>();

        Debug.Log("[Nucleus] Destruido. Game Over.");

        Invoke(nameof(LoadGameOver), 1.5f);
    }

    private void LoadGameOver()
    {
        SceneManager.LoadScene(_gameOverSceneName);
    }

    private void RefreshUI()
    {
        if (_hpSlider != null)
            _hpSlider.value = HpPercent;
    }
}


public struct OnNucleusDestroyed : IEvent
{
    public void Assign(params object[] parameters)
    {
    }

    public void Reset()
    {
    }
}
public struct OnHordeEnded : IEvent
{
    public void Assign(params object[] parameters)
    {
    }

    public void Reset()
    {
    }
}
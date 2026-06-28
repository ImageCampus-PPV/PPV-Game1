using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NucleusHealth : MonoBehaviour, IDamageable
{
    [Header("Config")]
    [SerializeField] private float _maxHp = 500f;
    [SerializeField] private string _gameOverSceneName = "GameOver";

    [Header("UI")]
    [SerializeField] private UnityEngine.UI.Slider _hpSlider;

    private float _currentHp;
    private bool _isDead;

    public float CurrentHp => _currentHp;
    public float MaxHp => _maxHp;
    public float HpPercent => _currentHp / _maxHp;
    public bool IsDead => _isDead;

    public Action<float> OnTakeDamage { get; set; }

    public Action OnNucleusDestroyed;
    public Action OnHordeEnded;

    private void Awake()
    {
        _currentHp = _maxHp;
        RefreshUI();
    }

    public void TakeDamage(float amount)
    {
        if (_isDead) return;

        _currentHp = Mathf.Max(0f, _currentHp - amount);
        OnTakeDamage?.Invoke(amount);

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
        OnHordeEnded?.Invoke();

        Debug.Log("[Nucleus] Horda terminada. Vida restaurada.");
    }

    private void TriggerGameOver()
    {
        _isDead = true;
        OnNucleusDestroyed?.Invoke();

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

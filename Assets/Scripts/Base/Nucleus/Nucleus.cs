using Assets.Scripts.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using UnityEngine;

public class Nucleus : DamageableEntity
{
    [Header("Config")]
    [SerializeField] private float _maxHp = 500f;

    [Header("UI")]
    [SerializeField] private UnityEngine.UI.Slider _hpSlider;

    private float _currentHp;

    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    private void Start()
    {
        _currentHp = _maxHp;
    }

    public override void TakeDamage(float amount)
    {
        _currentHp = Mathf.Max(0f, _currentHp - amount);
        EventBus.Raise<OnCombatDamage>(ID, amount);

        if (_currentHp <= 0f)
            EntityRegistry.Remove(this);
    }

    public void OnHordeComplete()
    {
        _currentHp = _maxHp;
    }
}

public struct OnHordeEndedEvent : IEvent
{
    public void Assign(params object[] parameters)
    {
    }

    public void Reset()
    {
    }
}
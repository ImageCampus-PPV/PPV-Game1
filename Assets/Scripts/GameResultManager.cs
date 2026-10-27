using UnityEngine;
using TaskScheduler = GreenAbyss.Scheduling.TaskScheduler;
using ServiceProvider = GreenAbyss.Services.ServiceProvider;
using EntityRegistry = GreenAbyss.Entities.EntityRegistry;
using EventBus = GreenAbyss.Events.EventBus;

public class GameResultManager : ITickable
{
    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
    private TaskScheduler TaskScheduler => ServiceProvider.Instance.GetService<TaskScheduler>();
    private GameCanvas GameCanvas => ServiceProvider.Instance.GetService<GameCanvas>();

    private float _restartDelay = 4f;
    private bool _restarted = false;
    private GameObject _lostGameGO;

    public GameResultManager(GameObject lostGameGO)
    {
        _lostGameGO = lostGameGO;
    }

    public void Tick(float deltaTime)
    {
        if (EntityRegistry.GetCountOf<Character>() == 0 && !_restarted)
        {
            _restarted = true;
            Debug.Log("All characters died.");
            GameObject lostGO = GameObject.Instantiate(_lostGameGO);
            GameCanvas.AddDynmic("LostGO", lostGO);
            EventBus.Raise<OnLoseEvent>();
            TaskScheduler.Schedule(RestartGame, _restartDelay);
        }
    }

    private void RestartGame()
    {
        _restarted = false;
        EventBus.Raise<OnRestartEvent>();
    }
}

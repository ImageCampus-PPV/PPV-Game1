using Systems;
using UnityEngine;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;

public class Main : MonoBehaviour
{
    [SerializeField] private SceneRef _gamePlayScene;
    [SerializeField] private GameplayLogic _gameplayLogic;
    [SerializeField] private PrefabsRegistry _prefabsRegistry;

    private void Awake()
    {
        ServiceProvider.Instance.AddService<PrefabsRegistry>(_prefabsRegistry);
        ServiceProvider.Instance.AddService<EventBus>(new EventBus());

        _gameplayLogic = new GameplayLogic(_gamePlayScene);
        _gameplayLogic.Init();

    }

    private void Start()
    {
        _gameplayLogic.LateInit();
    }

    private void Update()
    {
        _gameplayLogic.Tick(Time.deltaTime);
    }

    private void OnApplicationQuit()
    {
        ServiceProvider.Instance.ClearAllServices();
    }
}

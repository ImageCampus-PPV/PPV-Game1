using Systems;
using UnityEngine;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;

public class Main : MonoBehaviour
{
    [SerializeField] private SceneRef _gamePlayScene;
    [SerializeField] private GameplayLogic _gameplayLogic;
    [SerializeField] private GameCanvas _gameCanvas;
    [SerializeField] private PrefabsRegistry _prefabsRegistry;

    private void Awake()
    {
        ServiceProvider.Instance.AddService<PrefabsRegistry>(_prefabsRegistry);
        ServiceProvider.Instance.AddService<GameCanvas>(_gameCanvas);
        ServiceProvider.Instance.AddService<EventBus>(new EventBus());

        _gameplayLogic = new GameplayLogic(_gamePlayScene);

        _gameCanvas.Init();
        _gameplayLogic.Init();

    }

    private void Start()
    {
        _gameCanvas.LateInit();
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
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using System.Collections;
using Systems;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour, IInitiable, ITickable, IDisposable
{
    [SerializeField] private SceneRef _gamePlayScene;
    [SerializeField] private SceneRef _bootScene;
    [SerializeField] private GameCanvas _gameCanvas;
    [SerializeField] private PrefabsRegistry _prefabsRegistry;
    [SerializeField] private GameObject _lostGO;

    private GameplayLogic _gameplayLogic;
    private bool _isRestarting;
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        LateInit();   
    }

    private void Update()
    {
        if (_isRestarting)
            return;

        Tick(Time.deltaTime);
    }

    private void OnApplicationQuit()
    {
        Dispose();
    }

    public void Init()
    {
        ServiceProvider.Instance.AddService<PrefabsRegistry>(_prefabsRegistry);
        ServiceProvider.Instance.AddService<GameCanvas>(_gameCanvas);
        ServiceProvider.Instance.AddService<EventBus>(new EventBus());

        EventBus.Subscribe<OnRestartEvent>(RestartGame);

        _gameplayLogic = new GameplayLogic(_gamePlayScene, _lostGO);

        _gameCanvas.Init();
        _gameplayLogic.Init();
    }

    private void RestartGame(in OnRestartEvent onRestartEvent)
    {
        if (_isRestarting)
            return;

        _isRestarting = true;
        StartCoroutine(RestartGameCoroutine());
    }

    private IEnumerator RestartGameCoroutine()
    {
        Dispose();

        Scene gameplayScene = SceneManager.GetSceneByBuildIndex(_gamePlayScene.Index);

        if (gameplayScene.isLoaded)
        {
            AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(gameplayScene);
            yield return unloadOperation;
        }

        SceneManager.LoadScene(_bootScene.Index, LoadSceneMode.Single);
    }

    public void LateInit()
    {
        _gameCanvas.LateInit();
        _gameplayLogic.LateInit();
    }

    public void Tick(float deltaTime)
    {
        _gameplayLogic.Tick(deltaTime);
    }

    public void Dispose()
    {
        if (_gameplayLogic == null)
            return;

        EventBus.Unsubscribe<OnRestartEvent>(RestartGame);
        _gameplayLogic.Dispose();
        _gameplayLogic = null;
    }
}
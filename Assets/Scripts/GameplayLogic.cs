using Assets.Scripts.Entities;
using ImageCampus.ToolBox.Services;
using System;
using Systems;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayLogic : IInitiable, ITickable, IDisposable
{
    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
    private EntityFactory EntityFactory => ServiceProvider.Instance.GetService<EntityFactory>();
    private CoopCameraController CoopCameraController => ServiceProvider.Instance.GetService<CoopCameraController>();
    private ControllerMapping ControllerMapping => ServiceProvider.Instance.GetService<ControllerMapping>();

    private SceneRef _gamePlayScene;

    public GameplayLogic(SceneRef _gamePlayScene)
    {
        this._gamePlayScene = _gamePlayScene;
    }

    public void Init()
    {
        SceneManager.LoadScene(_gamePlayScene.Index, LoadSceneMode.Additive);

        ServiceProvider.Instance.AddService<ControllerMapping>(new ControllerMapping());
        ServiceProvider.Instance.AddService<EntityFactory>(new EntityFactory());
        ServiceProvider.Instance.AddService<EntityRegistry>(new EntityRegistry());
        ServiceProvider.Instance.AddService<CoopCameraController>(new CoopCameraController());
        ServiceProvider.Instance.AddService<CoopCameraController>(new Inventory());

        ControllerMapping.Init();
        EntityFactory.Init();
        CoopCameraController.Init();
    }

    public void LateInit()
    {
        ControllerMapping.LateInit();
        EntityFactory.LateInit();
        CoopCameraController.LateInit();

        EntityFactory.Create<Mecha>();
        EntityFactory.Create<Dragon>();

        for (int i = 0; i < 5; ++i)
            EntityFactory.Create<Wasp>(new Vector2(UnityEngine.Random.Range(-50, 50), UnityEngine.Random.Range(0, 10)));
    }

    public void Tick(float deltaTime)
    {
        ControllerMapping.Tick(deltaTime);
        CoopCameraController.Tick(deltaTime);
    }

    public void Dispose()
    {
        SceneManager.UnloadSceneAsync(_gamePlayScene.Index);
        ServiceProvider.Instance.ClearAllServices();
    }
}
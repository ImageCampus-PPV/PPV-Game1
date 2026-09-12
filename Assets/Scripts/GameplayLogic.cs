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
    private NearestObjectDetector PossibleInteractions => ServiceProvider.Instance.GetService<NearestObjectDetector>();

    private InteractionLogic _interactionLogic;
    private InteractionController _interactionController;
    private DepositLogic _depositLogic;
    private DepositUI _depositUI;

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
        ServiceProvider.Instance.AddService<InventoryLogic>(new InventoryLogic());
        ServiceProvider.Instance.AddService<NearestObjectDetector>(new NearestObjectDetector());


        _interactionLogic = new InteractionLogic();
        _interactionController = new InteractionController();
        _depositLogic = new DepositLogic();
        _depositUI = new DepositUI();

        _interactionController.Init();
        _interactionLogic.Init();
        ControllerMapping.Init();
        EntityFactory.Init();
        CoopCameraController.Init();
        _depositUI.Init();
    }

    public void LateInit()
    {
        _interactionController.Init();
        _interactionLogic.LateInit();
        ControllerMapping.LateInit();
        EntityFactory.LateInit();
        CoopCameraController.LateInit();
        _depositUI.LateInit();

        EntityFactory.Create<Mecha>();
        EntityFactory.Create<Dragon>();
        EntityFactory.Create<Deposit>();

        for (int i = 0; i < 5; ++i)
            EntityFactory.Create<Wasp>(new Vector2(UnityEngine.Random.Range(-50, 50), UnityEngine.Random.Range(0, 10)));
    }

    public void Tick(float deltaTime)
    {
        ControllerMapping.Tick(deltaTime);
        CoopCameraController.Tick(deltaTime);
        _depositLogic.Tick(deltaTime);
        PossibleInteractions.Tick(deltaTime);
    }

    public void Dispose()
    {
        SceneManager.UnloadSceneAsync(_gamePlayScene.Index);
        ServiceProvider.Instance.ClearAllServices();
    }
}
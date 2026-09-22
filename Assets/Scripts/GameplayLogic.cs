using Assets.Scripts.Entities;
using GreenAbyss.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Scheduling;
using ImageCampus.ToolBox.Services;
using System;
using Systems;
using UnityEngine;
using UnityEngine.SceneManagement;
using TaskScheduler = ImageCampus.ToolBox.Scheduling.TaskScheduler;

public class GameplayLogic : IInitiable, ITickable, IDisposable
{
    private EntityFactory EntityFactory => ServiceProvider.Instance.GetService<EntityFactory>();
    private CoopCameraController CoopCameraController => ServiceProvider.Instance.GetService<CoopCameraController>();
    private ControllerMapping ControllerMapping => ServiceProvider.Instance.GetService<ControllerMapping>();
    private NearestObjectDetector NearestObjectDetector => ServiceProvider.Instance.GetService<NearestObjectDetector>();
    private TaskScheduler TaskScheduler => ServiceProvider.Instance.GetService<TaskScheduler>();
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    private NucleusLogic _nucleusLogic;
    private HatchLogic _hatchLogic;
    private InventoryController _inventoryController;
    private InteractionLogic _interactionLogic;
    private InteractionController _interactionController;
    private DepositUI _depositUI;
    private HordeLogic _hordeLogic;

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
        ServiceProvider.Instance.AddService<Inventory>(new Inventory());
        ServiceProvider.Instance.AddService<NearestObjectDetector>(new NearestObjectDetector());
        ServiceProvider.Instance.AddService<Inventory>(new Inventory());
        ServiceProvider.Instance.AddService<Wallet>(new Wallet());
        ServiceProvider.Instance.AddService<TaskScheduler>(new TaskScheduler());

        _hatchLogic = new HatchLogic();
        _nucleusLogic = new NucleusLogic();
        _inventoryController = new InventoryController();
        _interactionLogic = new InteractionLogic();
        _interactionController = new InteractionController();
        _depositUI = new DepositUI();
        _hordeLogic = new HordeLogic();

        _hatchLogic.Init();
        _nucleusLogic.Init();
        _inventoryController.Init();
        _interactionController.Init();
        _interactionLogic.Init();
        ControllerMapping.Init();
        EntityFactory.Init();
        CoopCameraController.Init();
        _depositUI.Init();
        _hordeLogic.Init();
    }

    public void LateInit()
    {
        _hatchLogic.LateInit();
        _nucleusLogic.LateInit();
        _inventoryController.LateInit();
        _interactionController.Init();
        _interactionLogic.LateInit();
        ControllerMapping.LateInit();
        EntityFactory.LateInit();
        CoopCameraController.LateInit();
        _depositUI.LateInit();
        _hordeLogic.LateInit();

        EntityFactory.Create<Mecha>();
        EntityFactory.Create<Dragon>();
        EntityFactory.Create<Deposit>();
        EntityFactory.Create<Nucleus>();

        void SpawnEntities<ItemType>(int amount) where ItemType : BaseEntity
        {
            for (int i = 0; i < amount; ++i)
            {
                EntityFactory.Create<ItemType>(new Vector2(UnityEngine.Random.Range(-50, 50), UnityEngine.Random.Range(0, 10)));
            }
        }
    }

    public void Tick(float deltaTime)
    {
        TaskScheduler.Tick(deltaTime);
        ControllerMapping.Tick(deltaTime);
        CoopCameraController.Tick(deltaTime);
        NearestObjectDetector.Tick(deltaTime);
    }

    public void Dispose()
    {
        _hatchLogic.Dispose();
        _nucleusLogic.Dispose();
        _inventoryController.Dispose();
        _interactionController.Dispose();
        _interactionLogic.Dispose();
        ControllerMapping.Dispose();
        _depositUI.Dispose();
        _hordeLogic.Dispose();

        SceneManager.UnloadSceneAsync(_gamePlayScene.Index);
        ServiceProvider.Instance.ClearAllServices();
    }
}
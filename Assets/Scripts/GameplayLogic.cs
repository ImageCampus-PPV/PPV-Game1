using Assets.Scripts.Entities;
using GreenAbyss.Entities;
using ImageCampus.ToolBox.Scheduling;
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
    private NearestObjectDetector NearestObjectDetector => ServiceProvider.Instance.GetService<NearestObjectDetector>();
    private InventoryLogic InventoryLogic => ServiceProvider.Instance.GetService<InventoryLogic>();
    private Wallet Wallet => ServiceProvider.Instance.GetService<Wallet>();

    private NucleusLogic _nucleusLogic;
    private HatchLogic _hatchLogic;
    private InventoryController _inventoryController;
    private InteractionLogic _interactionLogic;
    private InteractionController _interactionController;
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
        ServiceProvider.Instance.AddService<InventoryLogic>(new InventoryLogic());
        ServiceProvider.Instance.AddService<Wallet>(new Wallet());
        ServiceProvider.Instance.AddService<TaskScheduler>(new TaskScheduler());

        _hatchLogic = new HatchLogic();
        _nucleusLogic = new NucleusLogic();
        _inventoryController = new InventoryController();
        _interactionLogic = new InteractionLogic();
        _interactionController = new InteractionController();
        _depositUI = new DepositUI();

        _hatchLogic.Init();
        _nucleusLogic.Init();
        InventoryLogic.Init();
        _inventoryController.Init();
        _interactionController.Init();
        _interactionLogic.Init();
        ControllerMapping.Init();
        EntityFactory.Init();
        CoopCameraController.Init();
        _depositUI.Init();
    }

    public void LateInit()
    {
        _hatchLogic.LateInit();
        _nucleusLogic.LateInit();
        InventoryLogic.LateInit();
        _inventoryController.LateInit();
        _interactionController.Init();
        _interactionLogic.LateInit();
        ControllerMapping.LateInit();
        EntityFactory.LateInit();
        CoopCameraController.LateInit();
        _depositUI.LateInit();

        EntityFactory.Create<Mecha>();
        EntityFactory.Create<Dragon>();
        EntityFactory.Create<Deposit>();
        EntityFactory.Create<Nucleus>();

        SpawnEntities<MechaItem>(10);
        SpawnEntities<DragonItem>(10);
        SpawnEntities<Wasp>(5);

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
        ControllerMapping.Tick(deltaTime);
        CoopCameraController.Tick(deltaTime);
        NearestObjectDetector.Tick(deltaTime);
    }

    public void Dispose()
    {
        SceneManager.UnloadSceneAsync(_gamePlayScene.Index);
        ServiceProvider.Instance.ClearAllServices();
    }
}
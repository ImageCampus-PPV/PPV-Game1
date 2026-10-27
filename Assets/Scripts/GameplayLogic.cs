using System;
using Systems;
using UnityEngine;
using UnityEngine.SceneManagement;
using TaskScheduler = GreenAbyss.Scheduling.TaskScheduler;
using ServiceProvider = GreenAbyss.Services.ServiceProvider;
using EntityRegistry = GreenAbyss.Entities.EntityRegistry;
using IEvent = GreenAbyss.Events.IEvent;

public class GameplayLogic : IInitiable, ITickable, IDisposable
{
    private EntityFactory EntityFactory => ServiceProvider.Instance.GetService<EntityFactory>();
    private CoopCameraController CoopCameraController => ServiceProvider.Instance.ContainsService<CoopCameraController>() ? ServiceProvider.Instance.GetService<CoopCameraController>() : null;
    private ControllerMapping ControllerMapping => ServiceProvider.Instance.ContainsService<ControllerMapping>() ? ServiceProvider.Instance.GetService<ControllerMapping>() : null;
    private NearestObjectDetector NearestObjectDetector => ServiceProvider.Instance.ContainsService<NearestObjectDetector>() ? ServiceProvider.Instance.GetService<NearestObjectDetector>() : null;
    private TaskScheduler TaskScheduler => ServiceProvider.Instance.ContainsService<TaskScheduler>() ? ServiceProvider.Instance.GetService<TaskScheduler>() : null;

    private GameObject _lostGo;

    private NucleusLogic _nucleusLogic;
    private HatchLogic _hatchLogic;
    private InventoryController _inventoryController;
    private InteractionLogic _interactionLogic;
    private InteractionController _interactionController;
    private DepositUI _depositUI;
    private HordeLogic _hordeLogic;
    private CombatSystem _combatSystem;
    private GameResultManager _gameResultManager;
    private DepositView _depositView;
    private InventoryView _inventoryView;
    private SceneRef _gamePlayScene;
    private SceneRef _bootScene;

    public GameplayLogic(SceneRef gamePlayScene, GameObject lostGo)
    {
        _gamePlayScene = gamePlayScene;
        _lostGo = lostGo;
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
        _combatSystem = new CombatSystem();
        _gameResultManager = new GameResultManager(_lostGo);

        GameObject gameObject = new GameObject("invertoryView");
        _depositView = gameObject.AddComponent<DepositView>();

        gameObject = new GameObject(nameof(InventoryView));
        _inventoryView = gameObject.AddComponent<InventoryView>();

        _depositView.Init();
        _inventoryView.Init();
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
        _combatSystem.Init();

        NearestObjectDetector.Init();
    }

    public void LateInit()
    {
        _inventoryView.LateInit();
        _depositView.LateInit();
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
        _combatSystem.LateInit();

        EntityFactory.Create<Mecha>();
        EntityFactory.Create<Dragon>();
        EntityFactory.Create<Deposit>();
        EntityFactory.Create<Nucleus>();

        NearestObjectDetector.LateInit();

        void SpawnEntities<ItemType>(int amount) where ItemType : GreenAbyss.Entities.BaseEntity
        {
            for (int i = 0; i < amount; ++i)
            {
                EntityFactory.Create<ItemType>(new Vector2(UnityEngine.Random.Range(-50, 50), UnityEngine.Random.Range(0, 10)));
            }
        }
    }

    public void Tick(float deltaTime)
    {
        TaskScheduler?.Tick(deltaTime);
        ControllerMapping?.Tick(deltaTime);
        CoopCameraController?.Tick(deltaTime);
        NearestObjectDetector?.Tick(deltaTime);
        _gameResultManager?.Tick(deltaTime);
    }

    public void Dispose()
    {
        _inventoryView.Dispose();
        _depositView.Dispose();
        _hatchLogic.Dispose();
        _nucleusLogic.Dispose();
        _inventoryController.Dispose();
        _interactionController.Dispose();
        _interactionLogic.Dispose();
        ControllerMapping.Dispose();
        _depositUI.Dispose();
        _hordeLogic.Dispose();
        _combatSystem.Dispose();
        _gameResultManager = null;

        ServiceProvider.Instance.ClearAllServices();
    }
}

public struct OnWinEvent : IEvent
{
    public void Assign(params object[] parameters)
    {
    }

    public void Reset()
    {
    }
}

public struct OnLoseEvent : IEvent
{
    public void Assign(params object[] parameters)
    {
    }

    public void Reset()
    {
    }
}

public struct OnRestartEvent : IEvent
{
    public void Assign(params object[] parameters)
    {
    }

    public void Reset()
    {
    }
}
using Assets.Scripts.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using UnityEngine;

public struct CharacterIsNearDeposit : IEvent
{
    public uint _characterID;
    public uint _depositID;

    public void Assign(params object[] parameters)
    {
        _characterID = (uint)parameters[0];
        _depositID = (uint)parameters[1];
    }

    public void Reset()
    {
        _characterID = default(uint);
        _depositID = default(uint);
    }
}

public struct CharactersNotNearDeposit : IEvent
{
    public void Assign(params object[] parameters)
    {

    }

    public void Reset()
    {

    }
}

public class DepositLogic : IDisposable
{
    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    private float _tolerance = 5;

    public void Tick(float deltaTime)
    {
        foreach (Deposit deposit in EntityRegistry.FilterEntities<Deposit>())
            foreach (Character character in EntityRegistry.FilterEntities<Character>())
                if (Vector3.SqrMagnitude(character.transform.position - deposit.transform.position) < _tolerance * _tolerance)
                {
                    EventBus.Raise<CharacterIsNearDeposit>(character.ID, deposit.ID);
                    return;
                }

        EventBus.Raise<CharactersNotNearDeposit>();
    }

    public void Dispose()
    {

    }
}

public class DepositUI : IInitiable, IDisposable
{
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
    private PrefabsRegistry PrefabsRegistry => ServiceProvider.Instance.GetService<PrefabsRegistry>();
    private GameCanvas GameCanvas => ServiceProvider.Instance.GetService<GameCanvas>();

    private GameObject CanvasGameObject => GameCanvas.Get(nameof(DepositUI));

    public void Init()
    {
        EventBus.Subscribe<CharacterIsNearDeposit>(OnCharacterNearDeposit);
        EventBus.Subscribe<CharactersNotNearDeposit>(OnCharacterNotNear);

        GameCanvas.AddDynmic(nameof(DepositUI), UnityEngine.Object.Instantiate(PrefabsRegistry.FindPrefabByName(nameof(DepositUI))));
    }

    public void LateInit()
    {

    }

    private void OnCharacterNearDeposit(in CharacterIsNearDeposit characterIsNearDepositEvent)
    {
        CanvasGameObject?.SetActive(true);
    }

    private void OnCharacterNotNear(in CharactersNotNearDeposit charactersNotNearDepositEvent)
    {
        CanvasGameObject?.SetActive(false);
    }

    public void Dispose()
    {
        GameCanvas.RemoveDynmic(nameof(DepositUI));
    }
}
using Assets.Scripts.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using UnityEngine;

public class DepositUI : IInitiable, ITickable, IDisposable
{
    private PrefabsRegistry PrefabsRegistry => ServiceProvider.Instance.GetService<PrefabsRegistry>();
    private GameCanvas GameCanvas => ServiceProvider.Instance.GetService<GameCanvas>();
    private NearestObjectDetector NearestObjectDetector => ServiceProvider.Instance.GetService<NearestObjectDetector>();
    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

    private GameObject CanvasGameObject => GameCanvas.Get(nameof(DepositUI));

    public void Init()
    {
        GameCanvas.AddDynmic(nameof(DepositUI), UnityEngine.Object.Instantiate(PrefabsRegistry.FindPrefabByName(nameof(DepositUI))));
    }

    public void LateInit()
    {

    }

    public void Dispose()
    {
        GameCanvas.RemoveDynmic(nameof(DepositUI));
    }

    public void Tick(float deltaTime)
    {
        foreach (Character character in EntityRegistry.FilterEntities<Character>())
        {
            if (NearestObjectDetector[character.ID].objectType == typeof(Deposit))
            {
                CanvasGameObject?.SetActive(true);
                return;
            }
        }

        CanvasGameObject?.SetActive(false);
    }
}
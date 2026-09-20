using GreenAbyss.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System.Collections.Generic;
using System;
using UnityEngine;
using TaskScheduler = ImageCampus.ToolBox.Scheduling.TaskScheduler;

class NucleusLogic : IInitiable, IDisposable
{
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
    private EntityFactory EntityFactory => ServiceProvider.Instance.GetService<EntityFactory>();
    private Wallet Wallet => ServiceProvider.Instance.GetService<Wallet>();
    private TaskScheduler TaskScheduler => ServiceProvider.Instance.GetService<TaskScheduler>();

    private const float SapCooldown = 120.0f;

    private List<Vector3> _destroyedNucleusPos;

    public void Init()
    {
        _destroyedNucleusPos = new List<Vector3>();

        EventBus.Subscribe<EntityDestroyEvent<Nucleus>>(OnNucleusDestroy);
        EventBus.Subscribe<OnHordeEndedEvent>(OnHordeEndedEvent);

        StartSapProduction();
    }

    public void LateInit()
    {

    }

    private void SapGenerator()
    {
        if (EntityRegistry.GetCountOf<Nucleus>() == 0)
            return;

        Wallet.AddResource<Sap>(5);

        StartSapProduction();
    }

    private void OnNucleusDestroy(in EntityDestroyEvent<Nucleus> nucleusDestoyEvent)
    {
        _destroyedNucleusPos.Add(EntityRegistry.GetAs<Nucleus>(nucleusDestoyEvent.entityID).transform.position);
    }

    private void OnHordeEndedEvent(in OnHordeEndedEvent onHordeEndedEvent)
    {
        foreach (Vector3 position in _destroyedNucleusPos)
            EntityFactory.Create<Nucleus>(position);

        StartSapProduction();
    }

    private void StartSapProduction()
    {
        TaskScheduler.Schedule(SapGenerator, SapCooldown);
    }

    public void Dispose()
    {
        EventBus.Unsubscribe<EntityDestroyEvent<Nucleus>>(OnNucleusDestroy);
        EventBus.Unsubscribe<OnHordeEndedEvent>(OnHordeEndedEvent);
    }

}
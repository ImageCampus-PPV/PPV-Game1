using GreenAbyss.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;

public class HatchLogic : IInitiable, IDisposable
{
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();


    public void Init()
    {
        EventBus.Subscribe<OnHordeStartedEvent>(CloseAllHatches);
        EventBus.Subscribe<OnHordeEndedEvent>(OpenAllHatches);
    }

    private void CloseAllHatches(in OnHordeStartedEvent callback)
    {
        foreach (Hatch hatch in EntityRegistry.FilterEntities<Hatch>())
        {
            hatch.CloseHatch();
        }
    }

    private void OpenAllHatches(in OnHordeEndedEvent callback)
    {
        foreach (Hatch hatch in EntityRegistry.FilterEntities<Hatch>())
        {
            hatch.OpenHatch();
        }
    }

    public void LateInit()
    {
    }

    public void Dispose()
    {
        EventBus.Unsubscribe<OnHordeEndedEvent>(OpenAllHatches);
        EventBus.Unsubscribe<OnHordeStartedEvent>(CloseAllHatches);
    }
}
using GreenAbyss.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;

public class HatchLogic : IInitiable, IDisposable
{
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();


    public void Init()
    {
        EventBus.Subscribe<OnHordeStartedEvent>(CloseAllHatches);
        EventBus.Subscribe<OnHordeEndedEvent>(OpenAllHatches);
    }

    public void LateInit()
    {
        OpenHatches();
    }

    private void OpenAllHatches(in OnHordeEndedEvent _)
    {
        OpenHatches();
    }

    private void OpenHatches()
    {
        foreach (Hatch hatch in EntityRegistry.FilterEntities<Hatch>())
            hatch.OpenHatch();
    }


    private void CloseAllHatches(in OnHordeStartedEvent _)
    {
        foreach (Hatch hatch in EntityRegistry.FilterEntities<Hatch>())
            hatch.CloseHatch();
    }

    public void Dispose()
    {
        EventBus.Unsubscribe<OnHordeEndedEvent>(OpenAllHatches);
        EventBus.Unsubscribe<OnHordeStartedEvent>(CloseAllHatches);
    }
}
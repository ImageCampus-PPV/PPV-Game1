using GreenAbyss.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;

public class HatchLogic : IInitiable, IDisposable
{
    private class HatchSettings
    {
        public int maxPlatesToOpen;
        public int currentCount;

        public HatchSettings(int maxPlatesToOpen)
        {
            this.maxPlatesToOpen = maxPlatesToOpen;
            this.currentCount = 0;
        }
    }

    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

    private Dictionary<uint, uint> _hatchOfPressurePlate;
    private Dictionary<uint, HatchSettings> _pressurePlateCountByHatch;

    public void Init()
    {
        _hatchOfPressurePlate = new Dictionary<uint, uint>();
        _pressurePlateCountByHatch = new Dictionary<uint, HatchSettings>();

        EventBus.Subscribe<OnPressurePlatePress>(OnPressPressurePlate);
        EventBus.Subscribe<OnPressurePlateUnPress>(OnUnPressPressurePlate);
    }

    public void LateInit()
    {
        foreach (Hatch hatch in EntityRegistry.FilterEntities<Hatch>())
        {
            _pressurePlateCountByHatch.Add(hatch.ID, new HatchSettings(hatch.pressurePlatesToOpen.Length));

            foreach (PressurePlate pressurePlate in hatch.pressurePlatesToOpen)
            {
                _hatchOfPressurePlate.Add(pressurePlate.ID, hatch.ID);
            }
        }
    }

    private void OnPressPressurePlate(in OnPressurePlatePress callback)
    {
        HatchSettings hatchSettings = _pressurePlateCountByHatch[_hatchOfPressurePlate[callback.pressurePlateID]];

        if (++hatchSettings.currentCount >= hatchSettings.maxPlatesToOpen)
        {
            EntityRegistry.GetAs<Hatch>(_hatchOfPressurePlate[callback.pressurePlateID]).OpenHatch();
            EventBus.Raise<OnHatchOpen>(_hatchOfPressurePlate[callback.pressurePlateID]);
        }
    }

    private void OnUnPressPressurePlate(in OnPressurePlateUnPress callback)
    {
        HatchSettings hatchSettings = _pressurePlateCountByHatch[_hatchOfPressurePlate[callback.pressurePlateID]];

        if (hatchSettings.currentCount < hatchSettings.maxPlatesToOpen)
        {
            EntityRegistry.GetAs<Hatch>(_hatchOfPressurePlate[callback.pressurePlateID]).CloseHatch();
            EventBus.Raise<OnHatchClose>(_hatchOfPressurePlate[callback.pressurePlateID]);
        }
    }

    public void Dispose()
    {
        EventBus.Unsubscribe<OnPressurePlatePress>(OnPressPressurePlate);
        EventBus.Unsubscribe<OnPressurePlateUnPress>(OnUnPressPressurePlate);
    }
}
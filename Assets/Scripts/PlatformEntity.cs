using Assets.Scripts.Entities;
using ImageCampus.ToolBox.Services;
using UnityEngine;

public class PlatformEntity : BaseEntity
{
    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

    private void Start()
    {
        EntityRegistry.Add(this);
    }
}

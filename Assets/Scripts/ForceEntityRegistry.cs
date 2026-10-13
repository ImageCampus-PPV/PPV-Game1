using GreenAbyss.Services;
using UnityEngine;

public class ForceEntityRegistry : MonoBehaviour
{
    private EntityFactory EntityFactory => ServiceProvider.Instance.GetService<EntityFactory>();

    private void Awake()
    {
        foreach (GreenAbyss.Entities.BaseEntity entity in GetComponents<GreenAbyss.Entities.BaseEntity>())
            EntityFactory.RegisterEntity(entity);

        Destroy(this);
    }
}

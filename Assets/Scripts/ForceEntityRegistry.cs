using ImageCampus.ToolBox.Services;
using UnityEngine;

public class ForceEntityRegistry : MonoBehaviour
{
    private EntityFactory EntityFactory => ServiceProvider.Instance.GetService<EntityFactory>();

    private void Awake()
    {
        foreach (BaseEntity entity in GetComponents<BaseEntity>())
            EntityFactory.RegisterEntity(entity);

        Destroy(this);
    }
}

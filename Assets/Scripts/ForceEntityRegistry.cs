using ImageCampus.ToolBox.Services;
using UnityEngine;

public class ForceEntityRegistry : MonoBehaviour
{
    private EntityFactory EntityFactory => ServiceProvider.Instance.GetService<EntityFactory>();

    private void Start()
    {
        EntityFactory.RegisterEntity(GetComponent<BaseEntity>());
        Destroy(this);
    }
}

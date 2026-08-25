using Assets.Scripts.Entities;
using ImageCampus.ToolBox.Services;
using UnityEngine;

public class Main : MonoBehaviour
{
    void Start()
    {
        ServiceProvider.Instance.AddService<EntityRegistry>(new EntityRegistry());       
    }

    void Update()
    {
        
    }

    private void OnApplicationQuit()
    {
        ServiceProvider.Instance.ClearAllServices();
    }
}

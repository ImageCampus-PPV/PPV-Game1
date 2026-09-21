using GreenAbyss.Entities;
using ImageCampus.ToolBox.Services;
using System;
using System.Reflection;
using UnityEngine;

public class EntityFactory : IService, IInitiable
{
    public bool IsPersistance => false;

    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

    private MethodInfo setIDFunction;


    private PrefabsRegistry PrefabsRegistry => ServiceProvider.Instance.GetService<PrefabsRegistry>();

    private uint lastAssignedID = 0;

    public void Init()
    {
        setIDFunction = typeof(BaseEntity).GetMethod(BaseEntity.SetIDName, BindingFlags.NonPublic | BindingFlags.Instance);
    }

    public void LateInit()
    {

    }

    public EntityType Create<EntityType>(Vector3 position = default) where EntityType : BaseEntity
    {
        Type entityType = typeof(EntityType);

        GameObject objectToUse = PrefabsRegistry.FindPrefabByName(entityType.Name);

        GameObject gameObjectGo = UnityEngine.Object.Instantiate(objectToUse, position, rotation: Quaternion.identity);

        if (!gameObjectGo.TryGetComponent<BaseEntity>(out BaseEntity entity))
            entity = gameObjectGo.AddComponent<EntityType>();

        RegisterEntity(entity);

        return entity as EntityType;
    }

    public void RegisterEntity(BaseEntity entity)
    {
        setIDFunction.Invoke(entity, new object[] { ++lastAssignedID });

        EntityRegistry.Add(entity);

        entity.Init();

        entity.LateInit();
    }
}
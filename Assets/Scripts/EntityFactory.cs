using GreenAbyss.Entities;
using GreenAbyss.Services;
using System;
using System.Reflection;
using UnityEngine;

public class EntityFactory : IService, IInitiable
{
    public bool IsPersistance => false;

    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
    private PrefabsRegistry PrefabsRegistry => ServiceProvider.Instance.GetService<PrefabsRegistry>();

    private MethodInfo setIDFunction;

    private uint lastAssignedID = 0;

    public void Init()
    {
        setIDFunction = typeof(GreenAbyss.Entities.BaseEntity).GetMethod(GreenAbyss.Entities.BaseEntity.SetIDName, BindingFlags.NonPublic | BindingFlags.Instance);
    }

    public void LateInit()
    {

    }

    public EntityType Create<EntityType>(Vector3 position = default) where EntityType : GreenAbyss.Entities.BaseEntity
    {
        Type entityType = typeof(EntityType);

        GameObject objectToUse = PrefabsRegistry.FindPrefabByName(entityType.Name);

        GameObject gameObjectGo = UnityEngine.Object.Instantiate(objectToUse, position, rotation: Quaternion.identity);

        if (!gameObjectGo.TryGetComponent<GreenAbyss.Entities.BaseEntity>(out GreenAbyss.Entities.BaseEntity entity))
            entity = gameObjectGo.AddComponent<EntityType>();

        RegisterEntity(entity);

        return entity as EntityType;
    }

    public void RegisterEntity(GreenAbyss.Entities.BaseEntity entity)
    {
        setIDFunction.Invoke(entity, new object[] { ++lastAssignedID });

        EntityRegistry.Add(entity);

        entity.Init();

        entity.LateInit();
    }
}
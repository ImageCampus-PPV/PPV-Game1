using Assets.Scripts.Entities;
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

    public void Create<EntityType>(Vector3 position = default) where EntityType : BaseEntity
    {
        Type entityType = typeof(EntityType);

        GameObject objectToUse = FindPrefabByName(entityType.Name);

        GameObject gameObjectGo = UnityEngine.Object.Instantiate(objectToUse, position, rotation: Quaternion.identity);

        if (!gameObjectGo.TryGetComponent<BaseEntity>(out BaseEntity entity))
            entity = gameObjectGo.AddComponent<EntityType>();

        setIDFunction.Invoke(entity, new object[] { ++lastAssignedID });

        EntityRegistry.Add(entity);
    }

    private GameObject FindPrefabByName(string name)
    {
        foreach (PrefabByName prefabByName in PrefabsRegistry.prefab)
            if (prefabByName.TypeName.Equals(name))
                return prefabByName.gameObject;

        return null;
    }
}
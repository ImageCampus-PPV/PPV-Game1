using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;
using System.Reflection;

using Resource = GreenAbis.Resource;

public sealed class Deposit : IService, IDisposable
{
    public bool IsPersistance => false;

    private readonly Dictionary<Type, Resource> resources;

    public Dictionary<Type, Resource> Resources => resources;

    public Deposit()
    {
        resources = new Dictionary<Type, Resource>();

        foreach (Type type in Assembly.GetCallingAssembly().GetTypes())
        {
            if (!type.IsClass || type.IsAbstract)
                continue;

            if (!typeof(Collectable).IsAssignableFrom(type))
                continue;

            resources.Add(type, new Resource(type.Name, 0, 300, 0));
        }

    }

    public void AddResource<TRespuseType>(uint amount)
    {
        Type resourceType = typeof(TRespuseType);

        if (!resources.ContainsKey(resourceType))
            throw new KeyNotFoundException($"The {resourceType.Name} is not registred as a Resource.");

        resources[resourceType].AddResource(amount);
    }

    public void RemoveResource<TRespuseType>(uint amount)
    {
        Type resourceType = typeof(TRespuseType);

        if (!resources.ContainsKey(resourceType))
            throw new KeyNotFoundException($"The {resourceType.Name} is not registred as a Resource.");

        resources[resourceType].RemoveResource(amount);
    }

    public bool HasResourceAmount<TRespuseType>(uint amount)
    {
        return resources[typeof(TRespuseType)].CurrentValue >= amount;
    }

    public long GetResourceAmount<TRespuseType>()
    {
        return resources[typeof(TRespuseType)].CurrentValue;
    }

    public void Dispose()
    {

    }
}


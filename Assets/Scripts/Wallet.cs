using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;
using System.Reflection;

public sealed class Wallet : IService, IDisposable
{
    public bool IsPersistance => false;

    private readonly Dictionary<Type, ResourceData> _resources;

    public Dictionary<Type, ResourceData> Resources => _resources;

    public Wallet()
    {
        _resources = new Dictionary<Type, ResourceData>();

        foreach (Type type in Assembly.GetCallingAssembly().GetTypes())
        {
            if (!type.IsClass || type.IsAbstract)
                continue;

            if (!typeof(Item).IsAssignableFrom(type))
                continue;

            _resources.Add(type, new ResourceData(type.Name, 0, 300, 0));
        }

    }

    public void AddResource<ResourceType>(uint amount)
    {
        Type resourceType = typeof(ResourceType);

        if (!_resources.ContainsKey(resourceType))
            throw new KeyNotFoundException($"The {resourceType.Name} is not registred as a Resource.");

        _resources[resourceType].AddResource(amount);
    }

    public void RemoveResource<ResourceType>(uint amount)
    {
        Type resourceType = typeof(ResourceType);

        if (!_resources.ContainsKey(resourceType))
            throw new KeyNotFoundException($"The {resourceType.Name} is not registred as a Resource.");

        _resources[resourceType].RemoveResource(amount);
    }

    public bool HasResourceAmount<ResourceType>(uint amount)
    {
        return _resources[typeof(ResourceType)].CurrentValue >= amount;
    }

    public long GetResourceAmount<ResourceType>()
    {
        return _resources[typeof(ResourceType)].CurrentValue;
    }

    public void Dispose()
    {

    }
}

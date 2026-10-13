using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;
using System.Reflection;

public sealed class Wallet : IService, IDisposable
{
    public bool IsPersistance => false;

    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    private readonly Dictionary<string, ResourceData> _resources;

    public Dictionary<string, ResourceData> Resources => _resources;

    public Wallet()
    {
        _resources = new Dictionary<string, ResourceData>();

        foreach (Type type in Assembly.GetCallingAssembly().GetTypes())
        {
            if (!type.IsClass || type.IsAbstract)
                continue;

            if (!typeof(Item).IsAssignableFrom(type))
                continue;

            _resources.Add(type.Name, new ResourceData(type.Name, 0, 300, 0));
        }

    }

    public void AddResourceByType<ResourceType>(uint amount) where ResourceType : Item
    {
        string resourceName = typeof(ResourceType).Name;
        AddResource(amount, resourceName);
        EventBus.Raise<DepositItemModifiedEvent<ResourceType>>(_resources[resourceName].CurrentValue - amount, _resources[resourceName].CurrentValue);
    }

    public void AddResource(uint amount, string typeName)
    {
        if (!_resources.ContainsKey(typeName))
            throw new KeyNotFoundException($"The {typeName} is not registred as a Resource.");

        _resources[typeName].AddResource(amount);
    }

    public void RemoveResource<ResourceType>(uint amount) where ResourceType : Item
    {
        string resourceName = typeof(ResourceType).Name;
        RemoveResource(amount, resourceName);
        EventBus.Raise<DepositItemModifiedEvent<ResourceType>>(_resources[resourceName].CurrentValue - amount, _resources[resourceName].CurrentValue);
    }

    public void RemoveResource(uint amount, string typeName)
    {
        if (!_resources.ContainsKey(typeName))
            throw new KeyNotFoundException($"The {typeName} is not registred as a Resource.");

        _resources[typeName].RemoveResource(amount);
    }

    public bool HasResourceAmount<ResourceType>(uint amount) where ResourceType : Item
    {
        return HasResourceAmount(amount, typeof(ResourceType).Name);
    }
    public bool HasResourceAmount(uint amount, string typeName)
    {
        return _resources[typeName].CurrentValue >= amount;
    }

    public long GetResourceAmount<ResourceType>() where ResourceType : Item
    {
        return GetResourceAmount(typeof(ResourceType).Name);
    }

    public long GetResourceAmount(string typeName)
    {
        return _resources[typeName].CurrentValue;
    }

    public void Dispose()
    {

    }
}

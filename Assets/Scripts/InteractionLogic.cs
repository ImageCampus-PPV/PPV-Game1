using Assets.Scripts.Entities;
using GreenAbyss.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;
using System.Reflection;

public class InteractionLogic : IInitiable, IDisposable
{
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

    private Dictionary<string, Type> _entityClassNameToType;
    private Dictionary<Type, Action<uint, Interactable>> interactableTypeToAction;
    private Dictionary<string, Type> _acceptedEntityNameToType;

    private MethodInfo _raiseFromGenerToType;
    private MethodInfo _unsubscribeMethod;

    public void Init()
    {
        _entityClassNameToType = new Dictionary<string, Type>();
        _acceptedEntityNameToType = new Dictionary<string, Type>();

        _unsubscribeMethod = EventBus.GetType().GetMethod(nameof(EventBus.Unsubscribe), BindingFlags.Instance | BindingFlags.Public);
        _raiseFromGenerToType = GetType().GetMethod(nameof(OnUserInteractionAccepted), BindingFlags.NonPublic | BindingFlags.Instance);

        EventBus.Subscribe<PlayerRequestInteractionAcceptedGeneric>(RaiseEventAsGeneric);

        RegisterEntityMethods();
    }

    public void LateInit()
    {

    }

    private void RaiseEventAsGeneric(in PlayerRequestInteractionAcceptedGeneric userRequestInteractionAcceptedGenericEvent)
    {
        Type raisingType = _entityClassNameToType[userRequestInteractionAcceptedGenericEvent.interactableType];

        _raiseFromGenerToType.MakeGenericMethod(raisingType).Invoke(this, new object[] { userRequestInteractionAcceptedGenericEvent.characterID, userRequestInteractionAcceptedGenericEvent.interactableID });
    }

    private void OnUserInteractionAccepted<InteractedType>(uint characterID, uint interactableID) where InteractedType : Interactable
    {
        EventBus.Raise<PlayerRequestInteractionAccepted<InteractedType>>(characterID, interactableID);
    }

    private void RegisterEntityMethods()
    {
        foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (!type.IsClass || type.IsAbstract)
                continue;

            if (!typeof(Interactable).IsAssignableFrom(type))
                continue;

            _entityClassNameToType.Add(type.Name, type);
        }
    }

    public void Dispose()
    {
        EventBus.Unsubscribe<PlayerRequestInteractionAcceptedGeneric>(RaiseEventAsGeneric);
    }
}

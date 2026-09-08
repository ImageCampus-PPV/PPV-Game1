using Assets.Scripts.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;
using System.Reflection;

public class InteractionLogic : IInitiable, IDisposable
{
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

    private Dictionary<Type, object> _creationSubsctiptions;
    private Dictionary<string, Type> _entityClassNameToType;
    private Dictionary<Type, Action<uint, Interactable>> interactableTypeToAction;
    private Dictionary<string, Type> _acceptedEntityNameToType;

    private MethodInfo _subscribeToInteractionAcceptedMethod;
    private MethodInfo _unsubscribeMethod;

    public void Init()
    {
        _entityClassNameToType = new Dictionary<string, Type>();
        _creationSubsctiptions = new Dictionary<Type, object>();
        _acceptedEntityNameToType = new Dictionary<string, Type>();

        _subscribeToInteractionAcceptedMethod = GetType().GetMethod(nameof(SubscribeToCreation), BindingFlags.NonPublic | BindingFlags.Instance);
        _unsubscribeMethod = EventBus.GetType().GetMethod(nameof(EventBus.Unsubscribe), BindingFlags.Instance | BindingFlags.Public);

        EventBus.Subscribe<PlayerRequestInteractionAcceptedGeneric>(RaiseEventAsGeneric);

        RegisterEntityMethods();
    }

    public void LateInit()
    {

    }

    private void OnUserInteractionAccepted<InteractedType>(uint characterID, uint interactableID) where InteractedType : Interactable
    {
        InteractedType interactable = EntityRegistry.GetAs<InteractedType>(interactableID);


    }

    private void RaiseEventAsGeneric(in PlayerRequestInteractionAcceptedGeneric userRequestInteractionAcceptedGenericEvent)
    {
        Type raisingType = _entityClassNameToType[userRequestInteractionAcceptedGenericEvent.interactableType];
        _subscribeToInteractionAcceptedMethod.MakeGenericMethod(raisingType).Invoke(this, new object[] {});

        //_creationSubsctiptions[_acceptedEntityNameToType[userRequestInteractionAcceptedGenericEvent.interactableType]]
    }

    private void RegisterEntityMethods()
    {
        foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (type.IsClass && !type.IsAbstract)
            {
                if (typeof(Interactable).IsAssignableFrom(type))
                {
                    _subscribeToInteractionAcceptedMethod.MakeGenericMethod(type).Invoke(this, new object[0]);
                    _entityClassNameToType.Add(type.Name, type);
                }
            }
        }
    }

    private void SubscribeToCreation<InteractableType>() where InteractableType : Interactable
    {
        EventBus.EventCallback<PlayerRequestInteractionAccepted<InteractableType>> callback = EventBus.SubscribeAndReturn<PlayerRequestInteractionAccepted<InteractableType>>(InteractionAccepted);

        _creationSubsctiptions.Add(typeof(PlayerRequestInteractionAccepted<InteractableType>), callback);
        _acceptedEntityNameToType.Add(nameof(InteractableType), typeof(PlayerRequestInteractionAccepted<InteractableType>));

        void InteractionAccepted(in PlayerRequestInteractionAccepted<InteractableType> callback)
        {
            OnUserInteractionAccepted<InteractableType>(callback.characterID, callback.interactableID);
        }

    }

    public void Dispose()
    {
        UnsubscribeToCreation();
    }

    private void UnsubscribeToCreation()
    {
        foreach (KeyValuePair<Type, object> methodsToUnsubscribe in _creationSubsctiptions)
            _unsubscribeMethod.MakeGenericMethod(methodsToUnsubscribe.Key).Invoke(EventBus, new object[] { methodsToUnsubscribe.Value });
    }
}

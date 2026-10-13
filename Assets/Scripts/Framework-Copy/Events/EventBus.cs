using GreenAbyss.Pool;
using GreenAbyss.Services;
using System;
using System.Collections.Generic;

namespace GreenAbyss.Events
{
    public sealed class EventBus : IService
    {
        public delegate void EventCallback<EventType>(in EventType callback) where EventType : struct, IEvent;
        public bool IsPersistance => false;

        private readonly ConcurrentPool _eventPool = new ConcurrentPool();
        private readonly Dictionary<Type, List<Delegate>> _subscribers = new Dictionary<Type, List<Delegate>>();

        public void Subscribe<EventType>(EventCallback<EventType> callback) where EventType : struct, IEvent
        {
            Type eventType = typeof(EventType);
            if (!_subscribers.ContainsKey(eventType))
                _subscribers.Add(eventType, new List<Delegate>());
            _subscribers[eventType].Add(callback);
        }

        public EventCallback<EventType> SubscribeAndReturn<EventType>(EventCallback<EventType> callback) where EventType : struct, IEvent
        {
            Subscribe(callback);
            return callback;
        }

        public void Unsubscribe<EventType>(EventCallback<EventType> callback) where EventType : struct, IEvent
        {
            Type eventType = typeof(EventType);
            if (_subscribers.TryGetValue(eventType, out List<Delegate> subscriptions))
                subscriptions.Remove(callback);
        }

        public void Raise<EventType>(params object[] parameters) where EventType : struct, IEvent
        {
            Type eventType = typeof(EventType);
            EventType raisingEvent = _eventPool.Get<EventType>(parameters);
            if (_subscribers.TryGetValue(eventType, out List<Delegate> subscriptions))
            {
                foreach (Delegate callback in subscriptions.ToArray())
                {
                    ((EventCallback<EventType>)callback)?.Invoke(raisingEvent);
                }
            }
            _eventPool.Release(raisingEvent);
        }

        public void Clear()
        {
            _subscribers.Clear();
        }
    }
}

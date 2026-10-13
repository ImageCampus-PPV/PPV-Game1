using System;
using System.Collections.Concurrent;

namespace GreenAbyss.Pool
{
    public sealed class ConcurrentPool
    {
        private readonly ConcurrentDictionary<Type, ConcurrentStack<IResettable>> _concurrentPool =
            new ConcurrentDictionary<Type, ConcurrentStack<IResettable>>();

        public ResettableType Get<ResettableType>(params object[] parameters) where ResettableType : IResettable 
        {
            Type ressetteableType = typeof(ResettableType);
            if (!_concurrentPool.ContainsKey(ressetteableType))
                _concurrentPool.TryAdd(ressetteableType, new ConcurrentStack<IResettable>());

            ResettableType value;
            if (_concurrentPool[ressetteableType].Count > 0)
            {
                _concurrentPool[ressetteableType].TryPop(out IResettable resettable);
                value = (ResettableType)resettable;
            }
            else
            {
                value = (ResettableType)Activator.CreateInstance(ressetteableType);
            }

            value.Assign(parameters);
            return value;
        }

        public void Release<ResettableType>(ResettableType ressettable) where ResettableType : IResettable 
        {
            ressettable.Reset();
            _concurrentPool[typeof(ResettableType)].Push(ressettable);
        }
    }
}

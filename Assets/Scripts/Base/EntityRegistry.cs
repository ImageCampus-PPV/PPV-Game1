using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GreenAbyss.Entities
{
    public struct EntityDestroyEvent<EntityType> : IEvent where EntityType : BaseEntity
    {
        public uint entityID;

        public void Assign(params object[] parameters)
        {
            entityID = (uint)parameters[0];
        }

        public void Reset()
        {
            entityID = default(uint);
        }
    }

    public class EntityRegistry : IService
    {
        public bool IsPersistance => false;

        private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

        private Dictionary<uint, BaseEntity> _entities;
        private Dictionary<Type, List<uint>> _entityIdsPerType;

        private MethodInfo _entityDestroyEvent;

        public EntityRegistry()
        {
            _entityDestroyEvent = GetType().GetMethod(nameof(InvokeEntityDestroyEvent), BindingFlags.NonPublic | BindingFlags.Instance);

            _entities = new Dictionary<uint, BaseEntity>();
            _entityIdsPerType = new Dictionary<Type, List<uint>>();
        }

        public void Add(BaseEntity newEntity)
        {
            _entities.Add(newEntity.ID, newEntity);

            Type currentEntityType = null;

            do
            {
                currentEntityType = currentEntityType == null ? newEntity.GetType() : currentEntityType.BaseType;

                if (!_entityIdsPerType.ContainsKey(currentEntityType))
                    _entityIdsPerType.Add(currentEntityType, new List<uint>());

                _entityIdsPerType[currentEntityType].Add(newEntity.ID);

            } while (currentEntityType != typeof(BaseEntity));
        }

        public EntityType GetAs<EntityType>(uint ID) where EntityType : BaseEntity
        {
            if (ID == BaseEntity.NULL_BASE_ENTITY)
                throw new NullReferenceException("Unit id 0 represents a null entity");

            if (!_entities.ContainsKey(ID))
                throw new KeyNotFoundException(ID.ToString());

            if (_entities[ID] is not EntityType)
                throw new InvalidCastException($"An attempt was made to obtain a type {_entities[ID].GetType().Name}"
                                             + $"entity as type {typeof(EntityType).Name} from the EntityRegistry");

            return _entities[ID] as EntityType;
        }

        public IEnumerable<EntityType> FilterEntities<EntityType>() where EntityType : BaseEntity
        {
            if (_entityIdsPerType.ContainsKey(typeof(EntityType)))
            {
                foreach (uint ID in _entityIdsPerType[typeof(EntityType)])
                {
                    yield return _entities[ID] as EntityType;
                }
            }
        }

        public EntityType GetEntityAtIndex<EntityType>(int index) where EntityType : BaseEntity
        {
            if (index < 0 || _entityIdsPerType[typeof(EntityType)].Count < index)
                throw new IndexOutOfRangeException($"Index {index} is not in range for {nameof(EntityRegistry)}");

            return GetAs<EntityType>(_entityIdsPerType[typeof(EntityType)][index]);
        }

        public int GetCountOf<EntityType>() where EntityType : BaseEntity
        {
            return _entityIdsPerType[typeof(EntityType)].Count;
        }

        private void InvokeEntityDestroyEvent<EntityType>(uint ID) where EntityType : BaseEntity
        {
            EventBus.Raise<EntityDestroyEvent<EntityType>>(ID);
        }

        public void Remove(BaseEntity newEntity)
        {
            Type currentEntityType = null;

            do
            {
                currentEntityType = currentEntityType == null ? newEntity.GetType() : currentEntityType.BaseType;

                if (_entityIdsPerType.ContainsKey(currentEntityType))
                    _entityIdsPerType[currentEntityType].Remove(newEntity.ID);

                _entityDestroyEvent.MakeGenericMethod(currentEntityType).Invoke(this, new object[] { newEntity.ID });

            } while (currentEntityType != typeof(BaseEntity));

            _entities.Remove(newEntity.ID);

            UnityEngine.Object.Destroy(newEntity.gameObject);
        }


        public void RemoveAllOfType<EntityType>() where EntityType : BaseEntity
        {
            IEnumerable<EntityType> entitiesToRemove = FilterEntities<EntityType>();

            foreach (EntityType entity in entitiesToRemove)
                Remove(entity);
        }

        public EntityType GetRandomEntityOfType<EntityType>() where EntityType : BaseEntity
        {
            int randomIndex = UnityEngine.Random.Range(0, _entityIdsPerType[typeof(EntityType)].Count);

            return GetEntityAtIndex<EntityType>(randomIndex);
        }

        public bool Has(uint interactableID)
        {
            return _entities.ContainsKey(interactableID);
        }
    }
}

using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using UnityEngine.UIElements;

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

            newEntity.Dispose();

            _entities.Remove(newEntity.ID);

            UnityEngine.Object.Destroy(newEntity.gameObject);
        }


        public void RemoveAllOfType<EntityType>() where EntityType : BaseEntity
        {
            IEnumerable<EntityType> entitiesToRemove = FilterEntities<EntityType>();
            List<EntityType> entitiesToRemoveList = new List<EntityType>(entitiesToRemove);

            foreach (EntityType entity in entitiesToRemoveList)
                Remove(entity);
        }

        public EntityType GetRandomEntityOfType<EntityType>() where EntityType : BaseEntity
        {
            int randomIndex = UnityEngine.Random.Range(0, _entityIdsPerType[typeof(EntityType)].Count);

            return GetEntityAtIndex<EntityType>(randomIndex);
        }

        public IEnumerable<EntityType> GetAllEntitiesInRadius<EntityType>(UnityEngine.Vector2 center, float radius) where EntityType : BaseEntity
        {
            float radiusSquared = radius * radius;

            foreach (EntityType entity in FilterEntities<EntityType>())
            {
                UnityEngine.Vector2 distance = (UnityEngine.Vector2)entity.transform.position - center;

                if (distance.sqrMagnitude <= radiusSquared)
                    yield return entity;
            }
        }

        public IEnumerable<EntityType> GetAllEntitiesInBox<EntityType>(UnityEngine.Vector2 center, UnityEngine.Vector2 size, float rotationAngle = 0f) where EntityType : BaseEntity
        {
            UnityEngine.Vector2 halfSize = size * 0.5f;
            UnityEngine.Quaternion inverseRot = rotationAngle == 0f ? UnityEngine.Quaternion.identity : UnityEngine.Quaternion.Euler(0f, 0f, -rotationAngle);

            foreach (EntityType entity in FilterEntities<EntityType>())
            {
                UnityEngine.Vector2 offset = (UnityEngine.Vector2)entity.transform.position - center;
                UnityEngine.Vector2 rotatedOffset = inverseRot * ((UnityEngine.Vector2)entity.transform.position - center);

                UnityEngine.Vector2 offsetUsed = rotationAngle == 0f ? offset : rotatedOffset;

                if (Math.Abs(offsetUsed.x) <= halfSize.x && Math.Abs(offsetUsed.y) <= halfSize.y)
                    yield return entity;
            }
        }

        public bool Has(uint interactableID)
        {
            return _entities.ContainsKey(interactableID);
        }
    }
}

using ImageCampus.ToolBox.Pool;
using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace GreenAbyss.Entities
{
    public sealed class ConcurrentPoolEntities
    {
        private readonly ConcurrentDictionary<Type, ConcurrentStack<BaseEntity>> _concurrentPool =
            new ConcurrentDictionary<Type, ConcurrentStack<BaseEntity>>();

        private EntityFactory EntityFactory => ServiceProvider.Instance.GetService<EntityFactory>();

        public EntityType Get<EntityType>(Vector3 initPos = default) where EntityType : BaseEntity
        {
            Type entityType = typeof(EntityType);
            if (!_concurrentPool.ContainsKey(entityType))
                _concurrentPool.TryAdd(entityType, new ConcurrentStack<BaseEntity>());

            EntityType value;
            if (_concurrentPool[entityType].Count > 0)
            {
                _concurrentPool[entityType].TryPop(out BaseEntity entity);
                entity.transform.position = initPos;
                value = entity as EntityType;
            }
            else
            {
                value = EntityFactory.Create<EntityType>(initPos);
                value.gameObject.SetActive(true);
            }

            return value;
        }

        public void Release<EntityType>(EntityType entity) where EntityType : BaseEntity
        {
            entity.gameObject.SetActive(false);
            _concurrentPool[typeof(EntityType)].Push(entity);
        }
    }
}

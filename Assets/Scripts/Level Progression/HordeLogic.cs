using GreenAbyss.Entities;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using TaskScheduler = ImageCampus.ToolBox.Scheduling.TaskScheduler;

public class HordeLogic : IInitiable
{
    private const float HORDE_COOLDOWN = 120f;
    private const float HORDE_DURATION = HORDE_COOLDOWN / 3f;
    private const float ENEMIES_SPAWN_COOLDOWN = 3f;
    private const int ENEMIES_PER_HORDE = 50;

    private MethodInfo _createEnemiesMethod;
    private readonly List<Type> _enemyTypesList;

    private TaskScheduler TaskScheduler => ServiceProvider.Instance.GetService<TaskScheduler>();
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
    private EntityFactory EntityFactory => ServiceProvider.Instance.GetService<EntityFactory>();

    private int EnemyCounter = 0;
    private int EnemySpawnerCount => EntityRegistry.GetCountOf<EnemySpawner>();

    public HordeLogic()
    {
        _enemyTypesList = new List<Type>();

        GetEnemyTypes();

        void GetEnemyTypes()
        {
            foreach (Type type in GetType().Assembly.GetTypes())
            {
                if (!type.IsClass || type.IsAbstract)
                    continue;

                if (!typeof(Enemy).IsAssignableFrom(type))
                    continue;

                _enemyTypesList.Add(type);
            }
        }
    }

    public void Init()
    {
        _createEnemiesMethod = typeof(EntityFactory).GetMethod(nameof(EntityFactory.Create), BindingFlags.Public | BindingFlags.Instance);
    }

    public void LateInit()
    {
        TaskScheduler.Schedule(StartHorde, 2f);
    }

    private void StartNewHordeCountdown()
    {
        TaskScheduler.Schedule(StartHorde, HORDE_COOLDOWN);
    }

    private void StartHorde()
    {
        Debug.Log("Start horde :3");
        EnemyCounter = 0;
        SpawnEnemies();
        EventBus.Raise<OnHordeStartedEvent>();
        EndHordeCountdown();
    }

    private void ClearAllEnemies()
    {
        EntityRegistry.RemoveAllOfType<Enemy>();
    }

    private void SpawnEnemies()
    {
        if (EnemyCounter++ == ENEMIES_PER_HORDE)
            return;

        Vector3 enemySpawnPosition = EntityRegistry.GetRandomEntityOfType<EnemySpawner>().transform.position;
        Type enemyType = GetRandomEnemyType();
        _createEnemiesMethod.MakeGenericMethod(enemyType).Invoke(EntityFactory, new object[] { enemySpawnPosition });

        TaskScheduler.Schedule(SpawnEnemies, ENEMIES_SPAWN_COOLDOWN);
    }

    private Type GetRandomEnemyType()
    {
        int randomEnemyIndex = UnityEngine.Random.Range(0, _enemyTypesList.Count - 1);

        return _enemyTypesList[randomEnemyIndex];
    }

    private void EndHordeCountdown()
    {
        TaskScheduler.Schedule(EndHorde, HORDE_DURATION);
    }

    private void EndHorde()
    {
        ClearAllEnemies();
        EventBus.Raise<OnHordeEndedEvent>();
        StartNewHordeCountdown();
    }
}

using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private List<EnemySpawner> _spawners = new();
    [SerializeField] private List<Enemy> _activeEnemies = new();

    private void Awake()
    {
        foreach (var spawner in _spawners)
        {
            spawner.OnEnemySpawn += OnNewEnemySpawned;
        }

        foreach (var enemy in _activeEnemies)
        {
            if (enemy.TryGetComponent<Health>(out var health))
                health.OnDowned += OnEnemyDown;
        }
    }

    private void OnEnemyDown(MonoBehaviour damageable)
    {
        if (damageable is Enemy enemy)
        {
            Debug.Log("Enemy down detected");

            foreach (var spawner in _spawners)
            {
                if (spawner.ContainsEnemy(enemy))
                {
                    if (spawner.SpawnedCount >= spawner.MaxToSpawn)
                    {
                        spawner.Reset();
                        Debug.Log("Resetted spawner");
                        break;
                    }
                }
            }

            _activeEnemies.Remove(enemy);
            Destroy(enemy.gameObject);
        }
    }

    private void OnNewEnemySpawned(Enemy enemy)
    {
        _activeEnemies.Add(enemy);
        if (enemy.TryGetComponent<Health>(out var health))
            health.OnDowned += OnEnemyDown;
    }
}

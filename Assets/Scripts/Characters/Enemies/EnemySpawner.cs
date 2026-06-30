using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private float _timeToSpawn = 2f;
    //If _maxToSpawn < 0, spawns with no limit.
    [SerializeField] private int _maxToSpawn = 4;
    [SerializeField] private GameObject _enemyFactoryGO;

    private int _spawnedCount;
    private IEnemyFactory _enemyFactory;
    public event Action<Enemy> OnEnemySpawn;
    private Coroutine _spawnRoutine;
    private List<Enemy> _enemiesSpawned = new();

    public int SpawnedCount => _spawnedCount;
    public int MaxToSpawn => _maxToSpawn;

    private void Awake()
    {
        _enemyFactory = _enemyFactoryGO.GetComponent<IEnemyFactory>();
    }

    private void Start()
    {
        _spawnRoutine = StartCoroutine(SpawnCoroutine());
    }

    private IEnumerator SpawnCoroutine()
    {
        while (_spawnedCount < _maxToSpawn || _maxToSpawn < 0)
        {
            yield return new WaitForSeconds(_timeToSpawn);

            GameObject enemyGO = _enemyFactory?.CreateRandomEnemy(transform.position, Quaternion.identity);
            _spawnedCount++;

            if (enemyGO.TryGetComponent<Enemy>(out var enemy))
            {
                _enemiesSpawned.Add(enemy);
                OnEnemySpawn?.Invoke(enemy);
            }
        }
    }

    public void Reset()
    {
        _spawnedCount = 0;

        StopCoroutine(_spawnRoutine);

        _spawnRoutine = null;

        _spawnRoutine = StartCoroutine(SpawnCoroutine());
        _enemiesSpawned.Clear();
    }

    public bool ContainsEnemy(Enemy enemy)
    {
        foreach (Enemy spawnedEnemy in _enemiesSpawned)
        {
            if (enemy == spawnedEnemy)
                return true;
        }

        return false;
    }
}

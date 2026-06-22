using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private float _timeToSpawn = 2f;
    //If _maxToSpawn < 0, spawns with no limit.
    [SerializeField] private int _maxToSpawn = 4;
    [SerializeField] private GameObject _enemyFactoryGO;

    private int _spawnedCount;
    private IEnemyFactory _enemyFactory;

    private void Awake()
    {
        _enemyFactory = _enemyFactoryGO.GetComponent<IEnemyFactory>();
    }

    private void Start()
    {
        StartCoroutine(SpawnCoroutine());
    }

    private IEnumerator SpawnCoroutine()
    {
        while (_spawnedCount < _maxToSpawn || _maxToSpawn < 0)
        {
            yield return new WaitForSeconds(_timeToSpawn);

            _enemyFactory?.CreateRandomEnemy(transform.position, Quaternion.identity);
            _spawnedCount++;
        }
    }
}

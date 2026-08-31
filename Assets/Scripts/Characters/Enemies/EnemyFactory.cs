using System.Collections.Generic;
using UnityEngine;

public class EnemyFactory : MonoBehaviour, IEnemyFactory
{
    [SerializeField] List<GameObject> _enemyPrefabs = new();
    [SerializeField] Transform _enemiesParent;

    //TODO: Replace with entityRegistry ids
    private int _enemyCounter = 0;
    //TODO: remove spawner name
    public GameObject CreateRandomEnemy(Vector3 pos, Quaternion rot, string spawnerDebugName)
    {
        if (_enemyPrefabs == null || _enemyPrefabs.Count == 0)
        {
            Debug.LogWarning("No prefabs assigned on Enemy Factory.");
            return null;
        }
        int randomIndex = Random.Range(0, _enemyPrefabs.Count);
        pos.z = 0f;
        GameObject prefab = _enemyPrefabs[randomIndex];
        GameObject prefabInstance = Instantiate(prefab, pos, rot, _enemiesParent);
        prefabInstance.name = prefabInstance.name + " " + (++_enemyCounter).ToString() + " Spawner: " + spawnerDebugName;

        return prefabInstance;
    }
}
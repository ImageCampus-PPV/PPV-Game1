using System.Collections.Generic;
using UnityEngine;

public class EnemyFactory : MonoBehaviour, IEnemyFactory
{
    [SerializeField] List<GameObject> _enemyPrefabs = new();
    [SerializeField] Transform _enemiesParent;
    public GameObject CreateRandomEnemy(Vector3 pos, Quaternion rot)
    {
        if (_enemyPrefabs == null || _enemyPrefabs.Count == 0)
        {
            Debug.LogWarning("No prefabs assigned on Enemy Factory.");
            return null;
        }

        int randomIndex = Random.Range(0, _enemyPrefabs.Count);
        GameObject prefab = _enemyPrefabs[randomIndex];
        return Instantiate(prefab, pos, rot, _enemiesParent);
    }
}
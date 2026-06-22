using UnityEngine;

public interface IEnemyFactory
{
    GameObject CreateRandomEnemy(Vector3 pos, Quaternion rot);
}

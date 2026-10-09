using GreenAbyss.Entities;
using ImageCampus.ToolBox.Services;
using System.Collections.Generic;
using UnityEngine;

public static class TargetSelector
{
    private static List<Transform> _validTargets = new List<Transform>();
    private static EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
    private static RuntimeDebugVisual DebugVisual => ServiceProvider.Instance.GetService<RuntimeDebugVisual>();

    public static Transform GetBestTarget(Vector3 origin, float range, LayerMask targetLayer, bool debug)
    {
        Transform closest = null;
        float closestDistanceSquared = Mathf.Infinity;

        if (debug)
        {
            Debug.Log("Selecting target from " + origin + " in range " + range);
            DebugVisual.DrawCircle(origin, range, Color.azure, Time.deltaTime);
        }

        _validTargets.Clear();

        foreach (BaseEntity damageable in EntityRegistry.GetAllEntitiesInRadius<BaseEntity>(origin, range))
        {
            if ((targetLayer.value & (1 << damageable.gameObject.layer)) == 0)
            {
                if (debug)
                    Debug.Log("Target " + damageable.name + " does not possess the required layer. From " + origin + " in range " + range);
                continue;
            }

            if (debug)
                Debug.Log("Found target from " + origin + " in range " + range + ": " + damageable.name);

            _validTargets.Add(damageable.transform);

            float distanceSquared = ((Vector2)damageable.transform.position - (Vector2)origin).sqrMagnitude;

            if (distanceSquared < closestDistanceSquared)
            {
                closestDistanceSquared = distanceSquared;
                closest = damageable.transform;
            }
        }

        return closest;
    }
}
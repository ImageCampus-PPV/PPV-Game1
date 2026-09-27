using GreenAbyss.Entities;
using ImageCampus.ToolBox.Services;
using System.Collections.Generic;
using UnityEngine;

public static class TargetSelector
{
    private static List<Transform> _validTargets = new List<Transform>();
    private static EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

    public static Transform GetBestTarget(Vector3 origin, float range, LayerMask targetLayer)
    {
        Transform closest = null;
        float closestDistanceSquared = Mathf.Infinity;

        _validTargets.Clear();

        foreach (DamageableEntity damageable in EntityRegistry.GetAllEntitiesInRadius<DamageableEntity>(origin, range))
        {
            if ((targetLayer.value & (1 << damageable.gameObject.layer)) == 0)
                continue;

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